using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class EnemyMoveSystem : JobComponentSystem
{
    [BurstCompile]
    struct EnemyMoveJob : IJobForEach<EnemySpeed_Component, EnemyTarget_Component, Translation>
    {
        public float DeltaTime;

        public void Execute(ref EnemySpeed_Component c0, ref EnemyTarget_Component c1, ref Translation c2)
        {
            //float3 enemyDest = 
            c2.Value = c1.Value;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new EnemyMoveJob
        {
            DeltaTime = Time.deltaTime
        };

        return job.Schedule(this, inputDeps);
    }
}
