using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerInput
{
    void ReadInput();
    Vector2 Destination { get; }
    bool Aim { get; }
    bool Jump { get; }
}
