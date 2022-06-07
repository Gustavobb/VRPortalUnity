using UnityEngine;
using UnityEngine.Rendering;

public class MainCamera : MonoBehaviour
{
    [SerializeField] 
    bool URP = false, showRenderingTime = false;
    
    static PortalBehaviour[] portals;

    public delegate void PreRenderPortals();
    public static PreRenderPortals preRenderPortals;

    public delegate void RenderPortals();
    public static RenderPortals renderPortals;

    public delegate void PostRenderPortals();
    public static RenderPortals postRenderPortals;

    public enum PortalType {Camera, Stencil};
    public PortalType portalType;

    void Awake() 
    {
        GetPortalCameras();

        Camera mainCamera = GetComponent<Camera>();
        if (URP)
            RenderPipelineManager.beginCameraRendering += URPRender;
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

    void URPRender(ScriptableRenderContext SRC, Camera camera)
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
