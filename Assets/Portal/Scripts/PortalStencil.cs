using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PortalStencil : MonoBehaviour
{
    #region Portal Settings
    [Header ("General Settings")]
    [SerializeField]
    Material portalMaterial;

    [SerializeField]
    List<Material> stencilsOutsidePortal = new List<Material>(), stencilsInsidePortal = new List<Material>();
    List<Material> originalStencilsOutsidePortal = new List<Material>(), originalStencilsInsidePortal = new List<Material>();
    [SerializeField]
    bool render = true, progressiveWarping = false;
    public bool warpTraveler = false, invertGravity = false, portalScaler = false;

    [HideInInspector]
    public Camera playerCamera, portalCamera;
    public enum RenderSide {Back, Front};
    
    [Header ("One Sided Portal Settings")]
    public bool oneSidedPortalInside = false;
    public RenderSide renderSideInside;
    public bool oneSidedPortalOutside = false;
    public RenderSide renderSideOutside;

    public bool oneSidedPortal = false;
    public RenderSide renderSide;

    [Header ("Optimization Settings")]
    [SerializeField]
    [Tooltip("Frustum Culling. Note that this does not take occluded objects into consideration. This is used to stop any portal that is not visible from rendering. 10x time gain. Not recommended for portals that can see other portals.")]
    bool cullIfNotInFrustum = true;
    [SerializeField]
    [Tooltip("Only use if frustum culling is active. Portals that can be seen also check if they can see portals. Fix the fact that frustum culling doesn't allow portals to see other portals not in view frustum. If portals can't be seen by others in your scene, do not use this.")]
    bool portalsCanSeeOtherPortals = false;
    [SerializeField]
    #endregion

    #region Portal Variables
    [HideInInspector]
    public bool canBeSeen = true, oneSidedPlaneCanBeSeen = true, canBeSeenByOtherPortal = false, needsToBeRendered = false;
    PortalStencil[] allPortalsInScene;
    MeshRenderer portalPlane;
    MeshFilter portalPlaneMeshFilter;
    int stencilInsideIndex;
    bool portalIsSwitchedToInside = false;
    float nearClipOffset = 0.05f, nearClipLimit = 0.2f;
    #endregion

    #region Unity Methods
    void Start() 
    {
        Setup();
    }

    void OnApplicationQuit()
    {
        if (portalIsSwitchedToInside)
            ChangeRenders();
    }
    #endregion

    #region Portal Initialization
    void Setup()
    {
        originalStencilsOutsidePortal = new List<Material>(stencilsOutsidePortal);
        originalStencilsInsidePortal = new List<Material>(stencilsInsidePortal);
        stencilInsideIndex = stencilsInsidePortal[0].GetInt("_StencilRef");
        allPortalsInScene = FindObjectsOfType<PortalStencil>();
        playerCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        portalCamera = GetComponentInChildren<Camera>();
        
        portalPlane = GetComponentInChildren<MeshRenderer>();
        portalPlaneMeshFilter = portalPlane.GetComponent<MeshFilter> ();
        needsToBeRendered = false;
    }
    #endregion

    #region Portal Rendering
    public void PreRender()
    {
        bool previousCondition = needsToBeRendered;
        needsToBeRendered = render;
        CheckCameraCanSee(portalPlane, playerCamera);

        if (cullIfNotInFrustum)
        {
            if (portalsCanSeeOtherPortals)
            {
                // todo - check if other portals can see this portal
                // todo - raycast to other portals
            }

            needsToBeRendered = needsToBeRendered && canBeSeen;
        }

        if (oneSidedPortal)
        {
            PlayerIsInSameSideAsRender();
            needsToBeRendered = needsToBeRendered && oneSidedPlaneCanBeSeen;
            portalPlane.enabled = oneSidedPlaneCanBeSeen;
        }

        if (needsToBeRendered != previousCondition)
        {
            if (needsToBeRendered)
                TurnOnPortal();
            else
                TurnOffPortal();
        }
    }

    public void Render()
    {
        if (!needsToBeRendered)
            return;
        
        SetScreenSizeToPreventClipping(playerCamera.transform.position);
    }

    public void PostRender()
    {
        if (!needsToBeRendered)
            return;

        // SetScreenSizeToPreventClipping(playerCamera.transform.position);
        canBeSeenByOtherPortal = false;
    }

    public void TurnOffPortal()
    {
        portalPlane.enabled = false;
    }

    public void TurnOnPortal()
    {
        portalPlane.enabled = true;
    }

    public void ChangeRenders()
    {
        portalIsSwitchedToInside = !portalIsSwitchedToInside;
        if (portalIsSwitchedToInside)
        {
            foreach (Material stencil in stencilsOutsidePortal)
                stencil.SetInt("_StencilRef", stencilInsideIndex);

            foreach (Material stencil in stencilsInsidePortal)
                stencil.SetInt("_StencilRef", 0);

            return;
        }

        foreach (Material stencil in stencilsOutsidePortal)
            stencil.SetInt("_StencilRef", 0);

        foreach (Material stencil in stencilsInsidePortal)
            stencil.SetInt("_StencilRef", stencilInsideIndex);
    }
    #endregion

    #region Portal optimization methods
    void PlayerIsInSameSideAsRender()
    {
        oneSidedPlaneCanBeSeen = SameSideAsRenderPlane(playerCamera.transform);
    }

    public bool SameSideAsRenderPlane(Transform t)
    {
        if (renderSide == RenderSide.Front) return Vector3.Dot (transform.forward, transform.position - t.position) < 0;
        else if (renderSide == RenderSide.Back) return Vector3.Dot (transform.forward, transform.position - t.position) > 0;

        return false;
    }

    public void CheckCameraCanSee(Renderer renderer, Camera camera)
    {
        // http://wiki.unity3d.com/index.php/IsVisibleFrom
        Plane[] cameraFrustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
		canBeSeen = GeometryUtility.TestPlanesAABB(cameraFrustumPlanes, renderer.bounds);
    }

    void CheckIfCameraSeesOtherPortal()
    {
    }
    #endregion

    #region Portal utility methods
    void HandleObliqueProjection()
    {
    }

    void SetScreenSizeToPreventClipping (Vector3 viewPoint) {
        // // Learning resource:
        // // https://docs.unity3d.com/Manual/FrustumSizeAtDistance.html
        float frustumHeight = playerCamera.nearClipPlane * Mathf.Tan(playerCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float frustumWidth  = frustumHeight * playerCamera.aspect;

        float screenThickness = new Vector3 (frustumWidth, frustumHeight, playerCamera.nearClipPlane).magnitude;
        bool viewFacingSameDirAsPortal = Vector3.Dot (transform.forward, transform.position - viewPoint) > 0;

        portalPlane.transform.localScale = new Vector3 (portalPlane.transform.localScale.x, portalPlane.transform.localScale.y, screenThickness);
        portalPlane.transform.localPosition = Vector3.forward * screenThickness * ((viewFacingSameDirAsPortal) ? .5f : -.5f);
    }
    #endregion
}
