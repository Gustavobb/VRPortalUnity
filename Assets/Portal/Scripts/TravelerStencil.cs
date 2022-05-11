using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TravelerStencil : MonoBehaviour
{
    [SerializeField]
    PlayerController playerController;
    delegate void HandlePortal();
    HandlePortal handlePortal;
    
    PortalStencil portal;
    Vector3 previousOffsetFromPortal, teleportPoint;
    float initialDistance;

    public bool canTeleport = false;

    void Teleport() 
    {
        if (portal.warpTraveler) 
            StartCoroutine(RotateWithLerp());

        if (portal.invertGravity) playerController.gravity *= -1;
        portal.ChangeRenders();
        // playerCamera.GetComponent<OutlinePostProcess>().postprocessMaterial = portalCamera.GetComponent<OutlinePostProcess>().postprocessMaterial;
        if (!portal.warpTraveler) playerController.distanceToTheGround = playerController.GetComponent<Collider>().bounds.extents.y;
    }

    void HandleTeleportation()
    {
        if (!portal.gameObject.activeSelf) 
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
            canTeleport = !(portal.oneSidedPortal && portal.SameSideAsRenderPlane(transform));
            
            if (canTeleport)
                Teleport();
        }
        
        previousOffsetFromPortal = offsetFromPortal;
    }

    public void LateUpdate()
    {
        if (handlePortal != null) handlePortal();
    }

    public void OnEnterPortal(PortalStencil portalArg)
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

    public void OnExitPortal()
    {
        handlePortal -= HandleTeleportation;
        portal = null;
    }

    void OnTriggerEnter(Collider other) 
    {
        PortalStencil portal = other.GetComponent<PortalStencil>();
        if (portal != null)
            OnEnterPortal(portal);
    }

    void OnTriggerExit(Collider other) 
    {
        PortalStencil portal = other.GetComponent<PortalStencil>();
        if (portal != null)
            OnExitPortal();
    }

    public IEnumerator RotateWithLerp()
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