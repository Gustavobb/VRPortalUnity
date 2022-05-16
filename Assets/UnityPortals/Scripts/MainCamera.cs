using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MainCamera : MonoBehaviour
{
    [SerializeField] 
    bool URP = false, showRenderingTime = false;
    
    static PortalBehaviour[] portals;

    delegate void PreRenderPortals();
    static PreRenderPortals preRenderPortals;

    delegate void RenderPortals();
    static RenderPortals renderPortals;

    delegate void PostRenderPortals();
    static RenderPortals postRenderPortals;

    public enum PortalType {Camera, Stencil};
    public PortalType portalType;

    void Awake() 
    {
        GetPortalCameras();

        Camera mainCamera = GetComponent<Camera>();
        if (URP)
            RenderPipelineManager.beginFrameRendering += (context, mainCamera) =>
            {
                HandlePortalRenderProcess();
            };;
    }

    void GetPortalCameras()
    {
        if (portalType == PortalType.Camera)
            portals = FindObjectsOfType<CameraPortal>();
        else if (portalType == PortalType.Stencil)
            portals = FindObjectsOfType<StencilPortal>();

        renderPortals = null;
        postRenderPortals = null;

        for (int i = 0; i < portals.Length; i++)
        {
            preRenderPortals += portals[i].PreRender;
            renderPortals += portals[i].Render;
            postRenderPortals += portals[i].PostRender;
        }
    }

    void OnPreCull() 
    {
        HandlePortalRenderProcess();
    }

    void HandlePortalRenderProcess()
    {
        double lastInterval = Time.realtimeSinceStartup;
        if (renderPortals != null) 
        {
            preRenderPortals();
            renderPortals();
            postRenderPortals();
        }
        double timeNow = Time.realtimeSinceStartup;
        if (showRenderingTime) Debug.Log(timeNow - lastInterval);
    }
}
