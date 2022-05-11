using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PortalStencil : MonoBehaviour
{
    #region Portal Settings
    [Header ("General Settings")]
    public List<Material> stencils = new List<Material>();
    public List<Material> stencilsToTeleport = new List<Material>();
    public bool warpTraveler = false, invertGravity = false, portalScaler = false;

    [SerializeField]
    bool render = true, progressiveWarping = false;

    [HideInInspector]
    public Camera playerCamera, portalCamera;
    
    [Header ("One Sided Portal Settings")]
    public bool oneSidedPortal = false;
    public enum RenderSide {Back, Front};
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

    float nearClipOffset = 0.05f, nearClipLimit = 0.2f;
    #endregion

    #region Unity Methods
    void Start() 
    {
        Setup();
    }
    #endregion

    #region Portal Initialization
    void Setup()
    {
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
        if (!render) return;
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
        
        SetScreenSizeToPreventClipping(portalCamera.transform.position);
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
    }

    public void TurnOnPortal()
    {
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
