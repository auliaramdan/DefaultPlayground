using System.Collections;
using System.Collections.Generic;
using System;
using Unity.Entities;

[Serializable]
public struct SpeedComponent : IComponentData
{
    public float Value;
}
