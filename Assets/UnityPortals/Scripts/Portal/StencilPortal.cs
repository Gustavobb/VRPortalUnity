using System.Collections.Generic;
using UnityEngine;

public class StencilPortal : PortalBehaviour
{
    #region Portal Settings
    [Header ("Stencil Portal General Settings")]
    [SerializeField]
    Material portalMaterial;

    [SerializeField]
    List<StencilActor> stencilActors = new List<StencilActor>();

    [Header ("Stencil Portal One Sided Portal Settings")]
    public bool oneSidedPortalInside = false;
    public RenderSide renderSideInside;
    public bool oneSidedPortalOutside = false;
    public RenderSide renderSideOutside;

    [Header ("Stencil Portal Optimization Settings")]
    [SerializeField]
    bool isStatic = false;
    #endregion

    #region Portal Variables
    [HideInInspector]
    StencilPortal[] allPortalsInScene;
    bool previousSide, insideOut;
    #endregion

    #region Unity Methods
    protected override void Start() 
    {
        bool isStaticCopy = isStatic;
        isStatic = false;
        HandleObliqueProjection();
        isStatic = isStaticCopy;
        base.Start();
    }
    #endregion

    #region Portal Initialization
    protected override void Setup()
    {
        base.Setup();
        insideOut = false;
        alwaysComputeSidePlayerIsOn = true;

        allPortalsInScene = FindObjectsOfType<StencilPortal>();
        int stencilInsideIndex = portalPlane.material.GetInt("_StencilRef");

        foreach (StencilActor actor in stencilActors)
            actor.stencilInsideIndex = stencilInsideIndex;
    }
    #endregion

    #region Portal Rendering
    public override void PreRender()
    {
        if (insideOut)
        {
            if (oneSidedPortalInside)
            {
                oneSidedPortal = true;
                renderSide = renderSideInside;
            }
            else
                oneSidedPortal = false;
        }
        else
        {
            if (oneSidedPortalOutside)
            {
                oneSidedPortal = true;
                renderSide = renderSideOutside;
            }
            else
                oneSidedPortal = false;
        }

        base.PreRender();
    }

    public override void Render()
    {
        if (!needsToBeRendered)
            return;
        
        HandleObliqueProjection();
        SetScreenSizeToPreventClipping(playerCamera.transform.position);
    }

    public override void PostRender()
    {
        if (!needsToBeRendered)
            return;

        // SetScreenSizeToPreventClipping(playerCamera.transform.position);
        canBeSeenByOtherPortal = false;
    }

    public override void TurnOffPortal()
    {
        base.TurnOffPortal();
        portalPlane.enabled = false;
    }

    public override void TurnOnPortal()
    {
        base.TurnOnPortal();
        portalPlane.enabled = true;
    }

    void ChangeRenders()
    {
        foreach (StencilActor actor in stencilActors)
            actor.HandleRenderOnPortalChange();

        insideOut = !insideOut;
    }
    #endregion

    #region Portal optimization methods
    #endregion

    #region Portal utility methods
    public override void TeleportedSomeone()
    {
        ChangeRenders();
    }

    protected override void HandleObliqueProjection()
    {
        if (isStatic)
        {
            if (playerSide == previousSide)
                return;
        }

        int side = playerSide ? 1 : -1;
        Vector3 normal = transform.rotation * Vector3.forward * side;
        foreach (StencilActor actor in stencilActors)
        {
            actor.HandleCuttingPlane(normal, transform.position);
            bool actorSide = GetViewPointSide(actor.transform.position);
            actor.HandleRenderingAdjustment(actorSide, playerSide);
        }
        
        previousSide = playerSide;
    }
    #endregion
}
