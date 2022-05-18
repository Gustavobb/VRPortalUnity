using UnityEngine;

public class StencilTraveler : TravelerBehaviour
{
    StencilActorsManager stencilActorsManager;

    protected void Start()
    {
        stencilActorsManager = FindObjectOfType<StencilActorsManager>();
    }
    
    protected override void Teleport()
    {
        base.Teleport();

        if (stencilActorsManager != null && stencilActorsManager.getActiveActors)
            stencilActorsManager.ResetActiveMaterials();

        portal.TeleportedSomeone();
        
        if (onTraveledPortal != null)
            onTraveledPortal();
    }

    protected override void OnTriggerEnter(Collider other) 
    {
        StencilPortal portal = other.GetComponent<StencilPortal>();
        if (portal != null)
            OnEnterPortal(portal);
    }

    protected override void OnTriggerExit(Collider other)
    {
        StencilPortal portal = other.GetComponent<StencilPortal>();
        if (portal != null)
            OnExitPortal();
    }
}