using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using System;

[Serializable]
public struct EnemyTarget_Component : IComponentData
{
    private Vector3 targetPos;

    public Vector3 Value { get => targetPos; set => targetPos = value; }
}
