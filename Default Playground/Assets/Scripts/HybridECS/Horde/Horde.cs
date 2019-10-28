using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;
using System;

[Serializable]
[WriteGroup(typeof(LocalToWorld))]
public struct HordeComponent : ISharedComponentData
{
    public float CellRadius;
    public float SeparationWeight;
    public float AlignmentWeight;
    public float TargetWeight;
    public float ObstacleAversionDistance;
    public float MoveSpeed;
}

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class Horde : MonoBehaviour, IConvertGameObjectToEntity
{
    // Add fields to your component here. Remember that:
    //
    // * The purpose of this class is to store data for authoring purposes - it is not for use while the game is
    //   running.
    // 
    // * Traditional Unity serialization rules apply: fields must be public or marked with [SerializeField], and
    //   must be one of the supported types.
    //
    // For example,
    //    public float scale;
    [SerializeField]
    private float cellRadius;
    [SerializeField]
    private float separationWeight;
    [SerializeField]
    private float alignmentWeight;
    [SerializeField]
    private float targetWeight;
    [SerializeField]
    private float obstacleAversionDistance;
    [SerializeField]
    private float moveSpeed;    

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        // Call methods on 'dstManager' to create runtime components on 'entity' here. Remember that:
        //
        // * You can add more than one component to the entity. It's also OK to not add any at all.
        //
        // * If you want to create more than one entity from the data in this class, use the 'conversionSystem'
        //   to do it, instead of adding entities through 'dstManager' directly.
        //
        // For example,
        //   dstManager.AddComponentData(entity, new Unity.Transforms.Scale { Value = scale });
        dstManager.AddSharedComponentData(entity, new HordeComponent
        {
            CellRadius = cellRadius,
            SeparationWeight = separationWeight,
            AlignmentWeight = alignmentWeight,
            TargetWeight = targetWeight,
            ObstacleAversionDistance = obstacleAversionDistance,
            MoveSpeed = moveSpeed
        });
        
    }
}
