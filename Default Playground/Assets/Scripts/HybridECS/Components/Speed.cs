using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace HybridECS.Components
{
    [RequiresEntityConversion]
    public class Speed : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float MoveSpeed = 10f;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var data = new SpeedComponent { Value = MoveSpeed };
            dstManager.AddComponentData(entity, data);
        }
    }
}
