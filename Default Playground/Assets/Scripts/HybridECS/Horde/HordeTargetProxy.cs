using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct HordeTarget : IComponentData { }

[DisallowMultipleComponent]
public class HordeTargetProxy : ComponentDataProxy<HordeTarget>
{
    
}
