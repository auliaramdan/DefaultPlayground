using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardInput : IPlayerInput
{
    public Vector2 Destination { get; private set; }

    public bool Aim { get; private set; }
    public bool Jump { get; private set; }

    public void ReadInput()
    {
        Destination = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) ;
        Aim = Input.GetMouseButton(1);
        Jump = Input.GetKeyDown(KeyCode.Space);
    }
}
