using UnityEngine;

public class StencilTraveler : TravelerBehaviour
{
    protected override void Teleport() 
    {
        base.Teleport();
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