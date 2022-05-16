using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
    int previousSide;
    #endregion

    #region Portal Initialization
    protected override void Setup()
    {
        base.Setup();
        allPortalsInScene = FindObjectsOfType<StencilPortal>();
        int stencilInsideIndex = portalPlane.material.GetInt("_StencilRef");

        foreach (StencilActor actor in stencilActors)
            actor.stencilInsideIndex = stencilInsideIndex;
    }
    #endregion

    #region Portal Rendering
    public override void PreRender()
    {
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

        oneSidedPortal = oneSidedPortalOutside;
        renderSide = renderSideOutside;
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
        Vector3 offset = playerCamera.transform.position - transform.position;
        int side = Math.Sign(Vector3.Dot(offset, transform.forward)) > 0 ? 1 : -1;

        if (isStatic)
        {
            if (side == previousSide)
                return;
        }

        Vector3 normal = transform.rotation * Vector3.forward * side;
        foreach (StencilActor actor in stencilActors)
        {
            actor.HandleCuttingPlane(normal, transform.position);

            offset = actor.transform.position - transform.position;
            int actorSide = Math.Sign(Vector3.Dot(offset, transform.forward)) > 0 ? 1 : -1;
            actor.HandleRenderingAdjustment(actorSide, side);
        }
        
        previousSide = side;
    }
    #endregion
}
