// http://blog.three-eyed-games.com/2018/05/03/gpu-ray-tracing-in-unity-part-1/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTracingMaster : MonoBehaviour {
    private const int KernelIndex = 0;
    // The default thread group size as defined in the Unity compute shader template is [numthreads(8,8,1)]
    // Spawn one thread group per 8×8 pixels
    private const float ThreadGroupSize = 8.0f;

    public ComputeShader RayTracingShader;
    private RenderTexture _target;

    private Camera _camera;

    private void Awake() {
        _camera = GetComponent<Camera>();
    }

    private void SetShaderParameters() {
        // Generate some camera rays
        RayTracingShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
        RayTracingShader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);
    }

    private void OnRenderImage(RenderTexture src, RenderTexture destination) {
        Render(destination);
    }

    private void Render(RenderTexture destination) {
        // Make sure we have a current render target
        InitRenderTexture();
        // Set the target and dispatch the compute shader
        RayTracingShader.SetTexture(KernelIndex, "Result", _target);
        // The total amount of compute shader invocations is the group count multiplied by the thread group size
        int threadGroupsX = Mathf.CeilToInt(Screen.width / ThreadGroupSize);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / ThreadGroupSize);
        int threadGroupsZ = 1;
        RayTracingShader.Dispatch(KernelIndex, threadGroupsX, threadGroupsY, threadGroupsZ);
        // Blit the result texture to the screen
        Graphics.Blit(_target, destination);
    }

    private void InitRenderTexture() {
        if (_target == null || _target.width != Screen.width || _target.height != Screen.height) {
            // Release render texture if we already have one
            if (_target != null) {
                _target.Release();
            }

            // Get a render target for Ray Tracing
            _target = new RenderTexture(
                Screen.width, 
                Screen.height, 
                0, 
                RenderTextureFormat.ARGBFloat, 
                RenderTextureReadWrite.Linear
            );
            _target.enableRandomWrite = true;
            _target.Create();
        }
    }
}