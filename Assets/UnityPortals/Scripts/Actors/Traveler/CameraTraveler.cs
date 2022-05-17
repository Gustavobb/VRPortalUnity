using UnityEngine;

public class CameraTraveler : TravelerBehaviour
{
    GravityBody gravityBody;
    CameraPortal camPortal;

    void Start()
    {
        gravityBody = GetComponent<GravityBody>();
    }

    protected override void Teleport()
    {
        base.Teleport();
        Matrix4x4 m = camPortal.portalThatRendersMe.transform.localToWorldMatrix * camPortal.transform.worldToLocalMatrix * transform.localToWorldMatrix;    
        float scale =  camPortal.portalThatRendersMe.portalScaler ? camPortal.portalThatRendersMe.transform.localScale.y / camPortal.transform.localScale.y : 1f;
        GravityBodyTeleportHandler(camPortal.transform, camPortal.portalThatRendersMe.transform, m.GetColumn(3), m.rotation, scale, camPortal.portalCamera, camPortal.portalThatRendersMe.portalCamera);
        OnExitPortal();

        if (onTraveledPortal != null)
            onTraveledPortal();
    }

    void GravityBodyTeleportHandler(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot, float scale, Camera playerCamera, Camera portalCamera) 
    {
        transform.position = pos;
        transform.rotation = rot;
        
        if (portal.warpTraveler) 
            StartCoroutine(RotateWithLerp());

        transform.localScale *= scale;
        gravityBody.gravity *= scale;

        if (portal.invertGravity) gravityBody.gravity *= -1;
        gravityBody.jumpHeight *= scale;
        gravityBody.speed *= scale;
        gravityBody.velocity = toPortal.TransformVector (fromPortal.InverseTransformVector(gravityBody.velocity));
        gravityBody.rb.velocity = toPortal.TransformVector (fromPortal.InverseTransformVector(gravityBody.rb.velocity));
        // playerCamera.GetComponent<OutlinePostProcess>().postprocessMaterial = portalCamera.GetComponent<OutlinePostProcess>().postprocessMaterial;
        Physics.SyncTransforms();
        if (!portal.warpTraveler) gravityBody.distanceToTheGround = gravityBody.GetComponent<Collider>().bounds.extents.y;
    }

    protected override void OnExitPortal()
    {
        base.OnExitPortal();
        camPortal = null;
    }

    protected override void OnTriggerEnter(Collider other) 
    {
        CameraPortal portal = other.GetComponent<CameraPortal>();
        if (portal != null)
        {
            camPortal = portal;
            OnEnterPortal(portal);
        }
    }

    protected override void OnTriggerExit(Collider other) 
    {
        CameraPortal portal = other.GetComponent<CameraPortal>();
        if (portal != null)
            OnExitPortal();
    }
}