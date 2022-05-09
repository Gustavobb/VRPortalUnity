using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    [SerializeField] 
    bool showRenderingTime = false;
    
    static Portal[] portals;

    delegate void PreRenderPortals();
    static PreRenderPortals preRenderPortals;

    delegate void RenderPortals();
    static RenderPortals renderPortals;

    delegate void PostRenderPortals();
    static RenderPortals postRenderPortals;

    void Awake() 
    {
        GetPortalCameras();
    }

    public static void GetPortalCameras()
    {
        portals = FindObjectsOfType<Portal>();
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
