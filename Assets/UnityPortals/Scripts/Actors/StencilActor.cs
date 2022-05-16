using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StencilActor : MonoBehaviour
{
    [SerializeField]
    Material actorMaterial, outsidePortalMaterialFront;

    Collider col;

    [SerializeField]
    bool insidePortal = false, needsCuttingAdjustment = false, needsRenderingAdjustment = false;

    [HideInInspector]
    public int stencilInsideIndex;
    int stencilStartIndex;
    MeshRenderer meshRenderer;

    void Start()
    {
        col = GetComponent<Collider>();
        stencilStartIndex = actorMaterial.GetInt("_StencilRef");
        meshRenderer = GetComponent<MeshRenderer>();
    }

    void EnterPortal()
    {
        col.enabled = true;
        actorMaterial.SetInt("_StencilRef", 0);
        actorMaterial.SetVector("sliceNormal", new Vector4(0, 0, 0, 0));
        actorMaterial.SetVector("sliceCentre", new Vector4(0, 0, 0, 0));
    }

    void ExitPortal()
    {
        col.enabled = false;
        meshRenderer.material = actorMaterial;
        actorMaterial.SetInt("_StencilRef", stencilInsideIndex);
    }

    public void HandleRenderOnPortalChange()
    {
        insidePortal = !insidePortal;
        if (insidePortal)
        {
            ExitPortal();
            return;
        }

        EnterPortal();
    }

    public void HandleCuttingPlane(Vector3 normal, Vector3 point)
    {
        if (!needsCuttingAdjustment || !insidePortal) return;

        actorMaterial.SetVector("sliceNormal", new Vector4(normal.x, normal.y, normal.z, 0));
        actorMaterial.SetVector("sliceCentre", new Vector4(point.x, point.y, point.z, 0));
    }

    public void HandleRenderingAdjustment(int side, int playerSide)
    {
        if (!needsRenderingAdjustment || insidePortal) return;

        if (side == playerSide)
            meshRenderer.material = outsidePortalMaterialFront;
        else
            meshRenderer.material = actorMaterial;
    }

    void OnApplicationQuit()
    {
        actorMaterial.SetVector("sliceNormal", new Vector4(0, 0, 0, 0));
        actorMaterial.SetVector("sliceCentre", new Vector4(0, 0, 0, 0));

        if (stencilStartIndex != actorMaterial.GetInt("_StencilRef"))
            actorMaterial.SetInt("_StencilRef", stencilStartIndex);
        
        meshRenderer.material = actorMaterial;
    }
}