using System.Collections;
using UnityEngine;
using System;

public class TravelerBehaviour : MonoBehaviour
{
    [SerializeField]
    protected delegate void HandlePortal();
    protected HandlePortal handlePortal;

    public delegate void OnTraveledPortal();
    public OnTraveledPortal onTraveledPortal;
    
    [HideInInspector]
    public PortalBehaviour portal;
    
    protected Vector3 previousOffsetFromPortal, teleportPoint;
    protected float initialDistance;

    public bool canTeleport = false;

    protected virtual void Teleport() 
    {
        if (portal.warpTraveler)
            StartCoroutine(RotateWithLerp());
    }

    protected virtual void HandleTeleportation()
    {
        if (portal == null || !portal.gameObject.activeSelf) 
        {
            OnExitPortal();
            return;
        }

        teleportPoint = portal.playerCamera.transform.position;

        Vector3 offsetFromPortal = teleportPoint - portal.transform.position;
        int sideFromPortal = Math.Sign(Vector3.Dot(offsetFromPortal, portal.transform.forward));
        int sideFromPortalOld = Math.Sign(Vector3.Dot(previousOffsetFromPortal, portal.transform.forward));
        
        if (sideFromPortal != sideFromPortalOld)
        {
            canTeleport = !(portal.oneSidedPortal && portal.SameSideAsRenderPlane(portal.GetViewPointSide(transform.position)));
            
            if (canTeleport)
                Teleport();
        }
        
        previousOffsetFromPortal = offsetFromPortal;
    }

    protected virtual void LateUpdate()
    {
        if (handlePortal != null) handlePortal();
    }

    protected virtual void OnEnterPortal(PortalBehaviour portalArg)
    {
        if (portal == null)
        {
            portal = portalArg;
            initialDistance = Vector3.Distance(portal.transform.position, transform.position);
            teleportPoint = portal.playerCamera.transform.position;
            previousOffsetFromPortal = teleportPoint - portal.transform.position;
            handlePortal += HandleTeleportation;
        }
    }

    protected virtual void OnExitPortal()
    {
        handlePortal -= HandleTeleportation;
        portal = null;
    }

    protected virtual void OnTriggerEnter(Collider other) 
    {
    }

    protected virtual void OnTriggerExit(Collider other) 
    {
    }

    protected virtual IEnumerator RotateWithLerp()
    {
        // FIXME TIME SCALE FOR JITTER
        float timeElapsed = 0;

        while (timeElapsed < 2f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0), timeElapsed / 2f);
            timeElapsed += Time.deltaTime;

            yield return null;
        }

        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
    }
}