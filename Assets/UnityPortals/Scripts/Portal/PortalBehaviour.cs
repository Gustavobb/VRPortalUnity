using UnityEngine;
using System;
using System.Collections;

public class PortalBehaviour : MonoBehaviour
{
    #region Portal Settings
    [Header ("General Settings")]
    public bool render = true;
    public bool warpTraveler = false, invertGravity = false, progressiveWarping = false, alwaysComputeSidePlayerIsOn = false;

    [SerializeField]
    protected float positionOffset = 0.5f, thicknessOffset = 1f;

    [HideInInspector]
    public Camera playerCamera;

    [Header ("One Sided Portal Settings")]
    public bool oneSidedPortal = false;
    public RenderSide renderSide;
    public enum RenderSide {Back, Front};

    [Header ("Optimization Settings")]
    [SerializeField]
    [Tooltip("Frustum Culling. Note that this does not take occluded objects into consideration. This is used to stop any portal that is not visible from rendering. 10x time gain. Not recommended for portals that can see other portals.")]
    protected bool cullIfNotInFrustum = true;
    [SerializeField]
    [Tooltip("Only use if frustum culling is active. Portals that can be seen also check if they can see portals. Fix the fact that frustum culling doesn't allow portals to see other portals not in view frustum. If portals can't be seen by others in your scene, do not use this.")]
    protected bool portalsCanSeeOtherPortals = false;
    [SerializeField]
    #endregion

    #region Portal Variables
    [HideInInspector]
    public bool canBeSeen = true, oneSidedPlaneCanBeSeen = true, canBeSeenByOtherPortal = false, needsToBeRendered = false, playerSide = false;
    protected MeshRenderer portalPlane;
    protected MeshFilter portalPlaneMeshFilter;
    protected Collider portalCollider;
    protected float nearClipOffset = 0.05f, nearClipLimit = 0.2f;
    Vector3 startScale;
    #endregion

    #region Unity Methods
    protected virtual void Awake() 
    {
        Setup();
    }

    protected virtual void Start() 
    {
        TurnOffPortal();
    }

    void OnDestroy()
    {
        MainCamera.preRenderPortals -= PreRender;
        MainCamera.postRenderPortals -= PostRender;
        MainCamera.renderPortals -= Render;
    }
    #endregion

    #region Portal Initialization
    protected virtual void Setup()
    {
        startScale = transform.localScale;
        portalCollider = GetComponent<Collider>();
        playerCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        portalPlane = GetComponentInChildren<MeshRenderer>();
        portalPlaneMeshFilter = portalPlane.GetComponent<MeshFilter>();
        needsToBeRendered = false;
    }
    #endregion

    #region Portal Rendering
    public virtual void PreRender()
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
            playerSide = GetViewPointSide(playerCamera.transform.position);
            oneSidedPlaneCanBeSeen = SameSideAsRenderPlane(playerSide);
            needsToBeRendered = needsToBeRendered && oneSidedPlaneCanBeSeen;
            portalPlane.enabled = oneSidedPlaneCanBeSeen;
        }

        else if (alwaysComputeSidePlayerIsOn)
        {
            playerSide = GetViewPointSide(playerCamera.transform.position);
        }

        if (needsToBeRendered != previousCondition)
        {
            if (needsToBeRendered)
                TurnOnPortal();
            else
                TurnOffPortal();
        }
    }

    public virtual void Render()
    {
    }

    public virtual void PostRender()
    {
    }

    public virtual void TurnOffPortal()
    {
        portalCollider.enabled = false;
    }

    public virtual void TurnOnPortal()
    {
        portalCollider.enabled = true;
    }
    #endregion

    #region Portal optimization methods
    public virtual bool GetViewPointSide(Vector3 point)
    {
        return Math.Sign(Vector3.Dot(point - transform.position, transform.forward)) > 0 ? true : false;
    }

    public virtual bool SameSideAsRenderPlane(bool side)
    {
        if (renderSide == RenderSide.Back)
            return side == false;
        else
            return side == true;
    }

    protected void CheckCameraCanSee(Renderer renderer, Camera camera)
    {
        // http://wiki.unity3d.com/index.php/IsVisibleFrom
        Plane[] cameraFrustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
		canBeSeen = GeometryUtility.TestPlanesAABB(cameraFrustumPlanes, renderer.bounds);
    }

    protected void CheckIfCameraSeesOtherPortal()
    {
        // Plane[] cameraFrustumPlanes = GeometryUtility.CalculateFrustumPlanes(portalCamera);
        // Plane[] cameraFrustumPlanes = GeometryUtility.CalculateFrustumPlanes(playerCamera);
        
        // for (int i = 0; i < allPortalsInScene.Length; i++)
        //     if (!allPortalsInScene[i].canBeSeen && !allPortalsInScene[i].canBeSeenByOtherPortal && allPortalsInScene[i] != this)
        //     {
        //         allPortalsInScene[i].canBeSeenByOtherPortal = GeometryUtility.TestPlanesAABB(cameraFrustumPlanes, allPortalsInScene[i].portalPlane.bounds);
        //         allPortalsInScene[i].canBeSeen = allPortalsInScene[i].canBeSeenByOtherPortal;
        //     }
    }
    #endregion

    #region Portal utility methods
    public virtual void TeleportedSomeone()
    {
    }

    protected virtual void HandleObliqueProjection()
    {
    }

    protected void SetScreenSizeToPreventClipping (Vector3 viewPoint) {
        // // Learning resource:
        // // https://docs.unity3d.com/Manual/FrustumSizeAtDistance.html
        // Eu nao sei escrever codigo, so sei fazer drag and drop no unity
        float frustumHeight = playerCamera.nearClipPlane * Mathf.Tan(playerCamera.fieldOfView * .5f * Mathf.Deg2Rad);
        float frustumWidth  = frustumHeight * playerCamera.aspect;

        float screenThickness = new Vector3 (frustumWidth, frustumHeight, playerCamera.nearClipPlane).magnitude;
        bool viewFacingSameDirAsPortal = Vector3.Dot (transform.forward, transform.position - viewPoint) > 0;

        portalPlane.transform.localScale = new Vector3 (portalPlane.transform.localScale.x, portalPlane.transform.localScale.y, screenThickness * thicknessOffset);
        portalPlane.transform.localPosition = Vector3.forward * screenThickness * ((viewFacingSameDirAsPortal) ? positionOffset : -positionOffset);
    }

    public void ShrinkPortal(float shrinkDuration)
    {
        StartCoroutine(LerpPortalScale(startScale, Vector3.zero, shrinkDuration, false));
    }

    public void GrowPortal(float growDuration)
    {
        StartCoroutine(LerpPortalScale(Vector3.zero, startScale, growDuration, true));
    }

    IEnumerator LerpPortalScale(Vector3 startScale, Vector3 endScale, float time, bool renderP)
    {
        if (renderP) render = true;
        float elapsedTime = 0;
        while (elapsedTime < time)
        {
            transform.localScale = Vector3.Lerp(startScale, endScale, elapsedTime / time);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        render = renderP;
    }
    #endregion
}
