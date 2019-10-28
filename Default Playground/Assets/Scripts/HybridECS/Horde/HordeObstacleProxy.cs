using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct HordeObstacle : IComponentData { }

[DisallowMultipleComponent]
public class HordeObstacleProxy : ComponentDataProxy<HordeObstacle>
{
}
