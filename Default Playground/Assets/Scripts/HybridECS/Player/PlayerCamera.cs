using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera
{
    private readonly Camera mainCam;

    private Vector3 camForward, camRight;

    public void NormalizedCameraForward()
    {
        camForward = mainCam.transform.forward;
        camRight = mainCam.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();
    }
}
