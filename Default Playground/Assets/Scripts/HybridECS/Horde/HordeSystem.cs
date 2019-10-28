using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
//using static Unity.Mathematics.math;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(TransformSystemGroup))]
public class HordeSystem : JobComponentSystem
{
    private EntityQuery hordeQuery;
    private EntityQuery targetQuery;
    private EntityQuery obstacleQuery;

    private List<HordeComponent> uniqueTypes = new List<HordeComponent>(3);
    private List<NativeMultiHashMap<int, int>> prevFrameHashmaps = new List<NativeMultiHashMap<int, int>>();

    protected override void OnCreate()
    {
        hordeQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[] {ComponentType.ReadOnly<HordeComponent>(), ComponentType.ReadWrite<LocalToWorld>()},
        });

        targetQuery = GetEntityQuery(new EntityQueryDesc {
            All = new[] {ComponentType.ReadOnly<HordeTarget>(), ComponentType.ReadOnly<LocalToWorld>()}
        });

        obstacleQuery = GetEntityQuery(new EntityQueryDesc {
            All = new[] {ComponentType.ReadOnly<HordeObstacle>(), ComponentType.ReadOnly<LocalToWorld>()}
        });
    }

    #region Extract positions

    [BurstCompile]
    struct CopyHeadings : IJobForEachWithEntity<LocalToWorld>
    {
        public NativeArray<float3> headings;

        public void Execute(Entity entity, int index, [ReadOnly] ref LocalToWorld localToWorld)
        {
            headings[index] = localToWorld.Forward;
        }
    }

    [BurstCompile]
    struct CopyPositions : IJobForEachWithEntity<LocalToWorld>
    {
        public NativeArray<float3> positions;

        public void Execute(Entity entity, int index, [ReadOnly] ref LocalToWorld localToWorld)
        {
            positions[index] = localToWorld.Position;
        }
    }

    #endregion

    #region Process positioning

    [BurstCompile]
    [RequireComponentTag(typeof(HordeComponent))]
    struct HashPositions : IJobForEachWithEntity<LocalToWorld>
    {
        public NativeMultiHashMap<int, int>.ParallelWriter hashMap;
        public float cellRadius;

        public void Execute(Entity entity, int index, [ReadOnly] ref LocalToWorld localToWorld)
        {
            var hash = (int)math.hash(new int3(math.floor(localToWorld.Position / cellRadius)));
            hashMap.Add(hash, index);
        }
    }

    [BurstCompile]
    struct MergeCells : IJobNativeMultiHashMapMergedSharedKeyIndices
    {
        public NativeArray<int> cellIndices;
        public NativeArray<float3> cellAlignment;
        public NativeArray<float3> cellSeparation;
        public NativeArray<int> cellObstaclePositionIndex;
        public NativeArray<float> cellObstacleDistance;
        public NativeArray<int> cellTargetPositionIndex;
        public NativeArray<int> cellCount;
        [ReadOnly] public NativeArray<float3> targetPositions;
        [ReadOnly] public NativeArray<float3> obstaclePositions;

        void NearestPosition(NativeArray<float3> targets, float3 position, out int nearestPositionIndex, out float nearestDistance)
        {
            nearestPositionIndex = 0;
            nearestDistance = math.lengthsq(position - targets[0]);
            for (int i = 1; i < targets.Length; i++)
            {
                var targetPosition = targets[i];
                var distance = math.lengthsq(position - targetPosition);
                var nearest = distance < nearestDistance;

                nearestDistance = math.select(nearestDistance, distance, nearest);
                nearestPositionIndex = math.select(nearestPositionIndex, i, nearest);
            }
            nearestDistance = math.sqrt(nearestDistance);
        }

        public void ExecuteFirst(int index)
        {
            var position = cellSeparation[index] / cellCount[index];

            int obstaclePositionIndex;
            float obstacleDistance;
            NearestPosition(obstaclePositions, position, out obstaclePositionIndex, out obstacleDistance);
            cellObstaclePositionIndex[index] = obstaclePositionIndex;
            cellObstacleDistance[index] = obstacleDistance;

            int targetPositionIndex;
            float targetDistance;
            NearestPosition(targetPositions, position, out targetPositionIndex, out targetDistance);
            cellTargetPositionIndex[index] = targetPositionIndex;

            cellIndices[index] = index;
        }

        public void ExecuteNext(int cellIndex, int index)
        {
            cellCount[cellIndex] += 1;
            cellAlignment[cellIndex] = cellAlignment[cellIndex] + cellAlignment[index];
            cellSeparation[cellIndex] = cellSeparation[cellIndex] + cellSeparation[index];
            cellIndices[cellIndex] = cellIndex;
        }
    }
    #endregion

    [BurstCompile]
    [RequireComponentTag(typeof(HordeComponent))]
    struct Steer : IJobForEachWithEntity<LocalToWorld>
    {
        public float dt;
        [ReadOnly] public HordeComponent settings;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<int> cellIndices;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float3> targetPositions;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float3> obstaclePositions;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float3> cellAlignment;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float3> cellSeparation;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<int> cellObstaclePositionIndex;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float> cellObstacleDistance;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<int> cellTargetPositionIndex;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<int> cellCount;


        public void Execute(Entity entity, int index, ref LocalToWorld localToWorld)
        {
            var forward                         = localToWorld.Forward;
            var currentPosition                 = localToWorld.Position;
            var cellIndex                       = cellIndices[index];
            var neighborCount                   = cellCount[cellIndex];
            var alignment                       = cellAlignment[cellIndex];
            var separation                      = cellSeparation[cellIndex];
            var nearestObstacleDistance         = cellObstacleDistance[cellIndex];
            var nearestObstaclePositionIndex    = cellObstaclePositionIndex[cellIndex];
            var nearestTargetPositionIndex      = cellTargetPositionIndex[cellIndex];
            var nearestObstaclePosition         = obstaclePositions[nearestObstaclePositionIndex];
            var nearestTargetPosition           = targetPositions[nearestTargetPositionIndex];

            var obstacleSteering                    = currentPosition - nearestObstaclePosition;
            var avoidObstacleHeading                = (nearestObstaclePosition + math.normalizesafe(obstacleSteering) * settings.ObstacleAversionDistance) - currentPosition;
            var targetHeading                       = settings.TargetWeight * math.normalizesafe(nearestTargetPosition - currentPosition);
            var nearestObstacleDistanceFromRadius   = nearestObstacleDistance - settings.ObstacleAversionDistance;
            var alignmentResult                     = settings.AlignmentWeight * math.normalizesafe((alignment / neighborCount) - forward);
            var separationResult                    = settings.SeparationWeight * math.normalizesafe((currentPosition * neighborCount) - separation);
            var normalHeading                       = math.normalizesafe(alignmentResult + separationResult + targetHeading);
            var targetForward                       = math.select(normalHeading, avoidObstacleHeading, nearestObstacleDistanceFromRadius < 0);
            var nextHeading                         = math.normalizesafe(forward + dt * (targetForward - forward));

            localToWorld = new LocalToWorld
            {
                Value = float4x4.TRS(new float3(localToWorld.Position + (nextHeading * settings.MoveSpeed * dt)), quaternion.LookRotationSafe(nextHeading, math.up()), new float3(1.0f, 1.0f, 1.0f))
            };
        }
    }

    protected override void OnStopRunning()
    {
        for (int i = 0; i < prevFrameHashmaps.Count; i++)
        {
            prevFrameHashmaps[i].Dispose();
        }
        prevFrameHashmaps.Clear();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        EntityManager.GetAllUniqueSharedComponentData(uniqueTypes);

        var obstacleCount = obstacleQuery.CalculateEntityCount();
        var targetCount = targetQuery.CalculateEntityCount();

        for (int i = 0; i < prevFrameHashmaps.Count; i++)
        {
            prevFrameHashmaps[i].Dispose();
        }
        prevFrameHashmaps.Clear();

        for (int hordeVariantIndex = 0; hordeVariantIndex < uniqueTypes.Count; hordeVariantIndex++)
        {
            var settings = uniqueTypes[hordeVariantIndex];
            hordeQuery.SetFilter(settings);
            var hordeCount = hordeQuery.CalculateEntityCount();

            if (hordeCount == 0) continue;
            //Debug.Log(hordeCount);

            #region Initial vars

            var hashMap = new NativeMultiHashMap<int, int>(hordeCount, Allocator.TempJob);

            var cellIndices = new NativeArray<int>(hordeCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var cellObstaclePositionIndex = new NativeArray<int>(hordeCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var cellTargetPositionIndex = new NativeArray<int>(hordeCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var cellCount = new NativeArray<int>(hordeCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var cellObstacleDistance = new NativeArray<float>(hordeCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var cellAlignment = new NativeArray<float3>(hordeCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var cellSeparation = new NativeArray<float3>(hordeCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var copyTargetPositions = new NativeArray<float3>(targetCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var copyObstaclePositions = new NativeArray<float3>(obstacleCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            #endregion

            #region Initial jobs

            var initialCellAlignmentJob = new CopyHeadings { headings = cellAlignment };
            var initialCellAlignmentJobHandle = initialCellAlignmentJob.Schedule(hordeQuery, inputDependencies);

            var initialCellSeparationJob = new CopyPositions { positions = cellSeparation};
            var initialCellSeparationJobHandle = initialCellSeparationJob.Schedule(hordeQuery, inputDependencies);

            var copyTargetPositionsJob = new CopyPositions { positions = copyTargetPositions };
            var copyTargetPositionsJobHandle = copyTargetPositionsJob.Schedule(targetQuery, inputDependencies);

            var copyObstaclePositionsJob = new CopyPositions { positions = copyObstaclePositions };
            var copyObstaclePositionsJobHandle = copyObstaclePositionsJob.Schedule(obstacleQuery, inputDependencies);

            prevFrameHashmaps.Add(hashMap);

            var hashPositionsJob = new HashPositions
            {
                hashMap = hashMap.AsParallelWriter(),
                cellRadius = settings.CellRadius
            };
            var hashPositionsJobHandle = hashPositionsJob.Schedule(hordeQuery, inputDependencies);

            var initialCellCountJob = new MemsetNativeArray<int>
            {
                Source = cellCount,
                Value = 1
            };
            var initialCellCountJobHandle = initialCellCountJob.Schedule(hordeCount, 64, inputDependencies);

            #endregion

            var initialCellBarrierJobHandle = JobHandle.CombineDependencies(initialCellAlignmentJobHandle, initialCellSeparationJobHandle, initialCellCountJobHandle);
            var copyTargetObstacleBarrierJobHandle = JobHandle.CombineDependencies(copyTargetPositionsJobHandle, copyObstaclePositionsJobHandle);
            var mergeCellsBarrierJobHandle = JobHandle.CombineDependencies(hashPositionsJobHandle, initialCellBarrierJobHandle, copyTargetObstacleBarrierJobHandle);

            var mergeCellsJob = new MergeCells
            {
                cellIndices = cellIndices,
                cellAlignment = cellAlignment,
                cellSeparation = cellSeparation,
                cellObstacleDistance = cellObstacleDistance,
                cellObstaclePositionIndex = cellObstaclePositionIndex,
                cellTargetPositionIndex = cellTargetPositionIndex,
                cellCount = cellCount,
                targetPositions = copyTargetPositions,
                obstaclePositions = copyObstaclePositions
            };
            var mergeCellsJobHandle = mergeCellsJob.Schedule(hashMap, 64, mergeCellsBarrierJobHandle);

            var steerJob = new Steer
            {
                cellIndices = cellIndices,
                settings = settings,
                cellAlignment = cellAlignment,
                cellSeparation = cellSeparation,
                cellObstacleDistance = cellObstacleDistance,
                cellObstaclePositionIndex = cellObstaclePositionIndex,
                cellTargetPositionIndex = cellTargetPositionIndex,
                cellCount = cellCount,
                targetPositions = copyTargetPositions,
                obstaclePositions = copyObstaclePositions,
                dt = Time.deltaTime,
            };
            var steerJobHandle = steerJob.Schedule(hordeQuery, mergeCellsJobHandle);

            inputDependencies = steerJobHandle;
            hordeQuery.AddDependency(inputDependencies);

        }
        uniqueTypes.Clear();

        return inputDependencies;
    }
}