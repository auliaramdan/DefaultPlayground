using Unity.Entities;
using UnityEngine;

[RequiresEntityConversion]
public class EnemySpeed_Inspector : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] private float value = 10;

    public float Value { get => value;}

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var data = new EnemySpeed_Component { Value = value };
        dstManager.AddComponentData(entity, data);
    }
}
