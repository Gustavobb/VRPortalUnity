using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StencilTraveler : TravelerBehaviour
{
    protected override void Teleport() 
    {
        base.Teleport();
        portal.TeleportedSomeone();
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