using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CameraPortal : PortalBehaviour
{
    #region Constants
    static readonly Vector3[] cubeCornerOffsets = {
        new Vector3 (1, 1, 1),
        new Vector3 (-1, 1, 1),
        new Vector3 (-1, -1, 1),
        new Vector3 (-1, -1, -1),
        new Vector3 (-1, 1, -1),
        new Vector3 (1, -1, -1),
        new Vector3 (1, 1, -1),
        new Vector3 (1, -1, 1),
    };
    #endregion

    #region Portal Settings
    [Header ("Camera Portal General Settings")]
    [SerializeField]
    Shader portalShader;
    public CameraPortal portalThatRendersMe, portalToRender;
    
    [SerializeField]
    Color disbledColor = new Color (0, 0, 0, 0);
    public bool portalScaler = false;
    
    [SerializeField]
    bool disablePlaneOnNotRendering = false, recursive = false;

    [SerializeField]
    int recursionLimit = 5;
    
    [HideInInspector]
    public Camera portalCamera;

    [Header ("Camera Portal One Sided Portal Settings")]
    public bool oneSidedPortalCamera = false;
    public RenderSide renderSideCamera;
    
    [Header ("Camera Portal Optimization Settings")]
    [SerializeField]
    [Tooltip("Some optimization for recursive portals. Use only if self recursive portals exist in scene.")]
    bool optimizeRecursivePortal = false;
    #endregion

    #region Portal Variables
    CameraPortal[] allPortalsInScene;
    Matrix4x4[] recursionMatrix;
    RenderTexture viewTexture;
    Matrix4x4 localToWorldMatrix;

    int startRecursionIndex = 0;
    #endregion

    #region Portal Initialization
    protected override void Setup()
    {
        base.Setup();
        oneSidedPortal = oneSidedPortalCamera;
        renderSide = renderSideCamera;
        recursionLimit = recursive ? recursionLimit : 1;
        allPortalsInScene = FindObjectsOfType<CameraPortal>();
        portalCamera = GetComponentInChildren<Camera>();
        Material portalPlaneMaterial = new Material(portalShader);
        portalPlaneMaterial.SetColor("_DisabledColor", disbledColor);
        portalPlane.material = portalPlaneMaterial;
    }
    #endregion

    #region Portal Rendering
    public override void PreRender()
    {
        base.PreRender();
    }

    public override void Render()
    {
        if (!portalToRender.needsToBeRendered)
            return;

        portalPlane.enabled = false;
        portalThatRendersMe.SetScreenSizeToPreventClipping(portalCamera.transform.position);
        CreateRenderTexture();

        startRecursionIndex = 0;
        HandlePortalCameraMovement();
    
        for (int i = startRecursionIndex; i < recursionLimit; i++)
        {
            portalCamera.transform.SetPositionAndRotation(recursionMatrix[i].GetColumn(3), recursionMatrix[i].rotation);
            HandleObliqueProjection();
            portalCamera.Render();
        }

        portalPlane.enabled = needsToBeRendered;
    }

    public override void PostRender()
    {
        if (!portalToRender.needsToBeRendered)
            return;

        // SetScreenSizeToPreventClipping(playerCamera.transform.position);
        canBeSeenByOtherPortal = false;
    }

    public override void TurnOffPortal()
    {
        portalThatRendersMe.portalCamera.enabled = false;
        Material portalPlaneMaterial = portalPlane.material;
        portalPlaneMaterial.SetFloat("_Enabled", 0);
        portalPlane.material = portalPlaneMaterial;

        if (disablePlaneOnNotRendering)
            portalPlane.enabled = false;
    }

    public override void TurnOnPortal()
    {
        portalThatRendersMe.portalCamera.enabled = true;
        Material portalPlaneMaterial = portalPlane.material;
        portalPlaneMaterial.SetFloat("_Enabled", 1);
        portalPlane.material = portalPlaneMaterial;
        portalPlane.enabled = true;
    }
    #endregion

    #region Portal optimization methods
    bool BoundsOverlap (MeshFilter nearObject, MeshFilter farObject, Camera camera)
    {
        var near = GetScreenRectFromBounds(nearObject, camera);
        var far = GetScreenRectFromBounds(farObject, camera);

        // ensure far object is indeed further away than near object
        if (far.zMax > near.zMin) 
        {
            // Doesn't overlap on x axis
            if (far.xMax < near.xMin || far.xMin > near.xMax) return false;
            
            // Doesn't overlap on y axis
            if (far.yMax < near.yMin || far.yMin > near.yMax) return false;
            
            // Overlaps
            return true;
        }

        return false;
    }

    // http://www.turiyaware.com/a-solution-to-unitys-camera-worldtoscreenpoint-causing-ui-elements-to-display-when-object-is-behind-the-camera/
    MinMax3D GetScreenRectFromBounds (MeshFilter renderer, Camera mainCamera) 
    {
        MinMax3D minMax = new MinMax3D (float.MaxValue, float.MinValue);

        Vector3[] screenBoundsExtents = new Vector3[8];
        var localBounds = renderer.sharedMesh.bounds;
        bool anyPointIsInFrontOfCamera = false;

        for (int i = 0; i < 8; i++) 
        {
            Vector3 localSpaceCorner = localBounds.center + Vector3.Scale (localBounds.extents, cubeCornerOffsets[i]);
            Vector3 worldSpaceCorner = renderer.transform.TransformPoint (localSpaceCorner);
            Vector3 viewportSpaceCorner = mainCamera.WorldToViewportPoint (worldSpaceCorner);

            if (viewportSpaceCorner.z > 0) 
            {
                anyPointIsInFrontOfCamera = true;
            } 

            else 
            {
                // If point is behind camera, it gets flipped to the opposite side
                // So clamp to opposite edge to correct for this
                viewportSpaceCorner.x = (viewportSpaceCorner.x <= 0.5f) ? 1 : 0;
                viewportSpaceCorner.y = (viewportSpaceCorner.y <= 0.5f) ? 1 : 0;
            }

            // Update bounds with new corner point
            minMax.AddPoint (viewportSpaceCorner);
        }

        // All points are behind camera so just return empty bounds
        if (!anyPointIsInFrontOfCamera) return new MinMax3D ();

        return minMax;
    }
    #endregion

    #region Portal utility methods
    void HandlePortalCameraMovement()
    {
        localToWorldMatrix = playerCamera.transform.localToWorldMatrix;
        recursionMatrix = new Matrix4x4[recursionLimit];
        portalCamera.projectionMatrix = playerCamera.projectionMatrix;

        for (int i = 0; i < recursionLimit; i++)
        {
            if (optimizeRecursivePortal && i > 0) 
                if (!BoundsOverlap(portalPlaneMeshFilter, portalToRender.portalPlaneMeshFilter, portalCamera))
                    break; 
            
            startRecursionIndex = recursionLimit - i - 1;
            localToWorldMatrix = transform.localToWorldMatrix * portalToRender.transform.worldToLocalMatrix * localToWorldMatrix;
            recursionMatrix[startRecursionIndex] = localToWorldMatrix;
        }
    }

    void CreateRenderTexture()
    {
        if (portalCamera.targetTexture == null || viewTexture.width != Screen.width || viewTexture.height != Screen.height)
        {
            if (portalCamera.targetTexture != null) portalCamera.targetTexture.Release();
            viewTexture = new RenderTexture(Screen.width, Screen.height, 0);
            portalCamera.targetTexture = viewTexture;
            portalToRender.portalPlane.material.SetTexture("_MainTex", viewTexture);
        }        
    }

    public void SetTexture(RenderTexture texture)
    {
        portalPlane.material.SetTexture("_MainTex", texture);
    }

    protected override void HandleObliqueProjection()
    {
        // Learning resource:
        // http://www.terathon.com/lengyel/Lengyel-Oblique.pdf
        // https://danielilett.com/2019-12-18-tut4-3-matrix-matching/
        int dotProduct = Math.Sign (Vector3.Dot (transform.forward, transform.position - portalCamera.transform.position));

        Vector3 camWorldPos = portalCamera.worldToCameraMatrix.MultiplyPoint (transform.position);
        Vector3 normal = portalCamera.worldToCameraMatrix.MultiplyVector (transform.forward) * dotProduct;
        float camSpaceDst = -Vector3.Dot (camWorldPos, normal) + nearClipOffset;

        if (Mathf.Abs (camSpaceDst) > nearClipLimit) portalCamera.projectionMatrix = playerCamera.CalculateObliqueMatrix (new Vector4 (normal.x, normal.y, normal.z, camSpaceDst));
        else portalCamera.projectionMatrix = playerCamera.projectionMatrix;
    }
    #endregion

    #region Structs
    public struct MinMax3D 
    {
        public float xMin;
        public float xMax;
        public float yMin;
        public float yMax;
        public float zMin;
        public float zMax;

        public MinMax3D (float min, float max) 
        {
            this.xMin = min;
            this.xMax = max;
            this.yMin = min;
            this.yMax = max;
            this.zMin = min;
            this.zMax = max;
        }

        public void AddPoint (Vector3 point) 
        {
            xMin = Mathf.Min (xMin, point.x);
            xMax = Mathf.Max (xMax, point.x);
            yMin = Mathf.Min (yMin, point.y);
            yMax = Mathf.Max (yMax, point.y);
            zMin = Mathf.Min (zMin, point.z);
            zMax = Mathf.Max (zMax, point.z);
        }
    }
    #endregion
}
