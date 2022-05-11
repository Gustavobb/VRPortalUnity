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
    Vector3 previousOffsetFromPortal, teleportPoint, initialRotation;
    float initialDistance;

    public bool canTeleport = false;

    void Teleport(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot, float scale, Camera playerCamera, Camera portalCamera) 
    {
        transform.position = pos;
        transform.rotation = rot;
        
        if (portal.warpTraveler) 
            StartCoroutine(RotateWithLerp());

        transform.localScale *= scale;
        playerController.gravity *= scale;

        if (portal.invertGravity) playerController.gravity *= -1;
        playerController.jumpHeight *= scale;
        playerController.speed *= scale;
        playerController.velocity = toPortal.TransformVector (fromPortal.InverseTransformVector (playerController.velocity));
        playerController.rb.velocity = toPortal.TransformVector (fromPortal.InverseTransformVector (playerController.rb.velocity));
        // playerCamera.GetComponent<OutlinePostProcess>().postprocessMaterial = portalCamera.GetComponent<OutlinePostProcess>().postprocessMaterial;
        Physics.SyncTransforms();
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
            {
                Matrix4x4 m = portal.portalThatRendersMe.transform.localToWorldMatrix * portal.transform.worldToLocalMatrix * transform.localToWorldMatrix;    
                float scale =  portal.portalThatRendersMe.portalScaler ?  portal.portalThatRendersMe.transform.localScale.y/portal.transform.localScale.y : 1f;
                Teleport(portal.transform, portal.portalThatRendersMe.transform, m.GetColumn(3), m.rotation, scale, portal.portalCamera, portal.portalThatRendersMe.portalCamera);
                OnExitPortal();
            }
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
            initialRotation = transform.rotation.eulerAngles;
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