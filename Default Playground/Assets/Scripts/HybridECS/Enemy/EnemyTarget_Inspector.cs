using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[RequiresEntityConversion]
public class EnemyTarget_Inspector : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] private Vector3 targetPos;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var data = new EnemyTarget_Component { Value = targetPos };
        dstManager.AddComponentData(entity, data);
    }
}
