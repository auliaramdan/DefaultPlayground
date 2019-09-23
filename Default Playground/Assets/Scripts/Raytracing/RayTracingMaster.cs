using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTracingMaster : MonoBehaviour
{
    [SerializeField] private ComputeShader rayTracingShader;
    [SerializeField] private Texture skyboxTexture;

    private RenderTexture target;
    private Camera camera;

    public ComputeShader RayTracingShader { get => rayTracingShader; private set => rayTracingShader = value; }

    private void Awake()
    {
        camera = GetComponent<Camera>();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        SetShaderParameters();
        Render(destination);
    }

    private void Render(RenderTexture destination)
    {
        //Set current render target
        InitRenderTexture();

        RayTracingShader.SetTexture(0, "Result", target);

        // Set one thread per pixel
        // Default thread group size : [numthreads(8,8,1)]
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8f);

        // Dispatch the shader
        RayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        Graphics.Blit(target, destination);
    }

    private void InitRenderTexture()
    {
        if (!target || target.width != Screen.width || target.height != Screen.height)
        {
            if (target) target.Release();

            target = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            target.enableRandomWrite = true;
            target.Create();
            Debug.Log("Target created");
        }
    }

    private void SetShaderParameters()
    {
        RayTracingShader.SetMatrix("cameraToWorld", camera.cameraToWorldMatrix);
        RayTracingShader.SetMatrix("cameraInverseProjection", camera.projectionMatrix.inverse);
        RayTracingShader.SetTexture(0, "skyboxTexture", skyboxTexture);
    }
}
