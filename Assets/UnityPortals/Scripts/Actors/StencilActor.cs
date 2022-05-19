using UnityEngine;

public class StencilActor : MonoBehaviour
{
    [SerializeField]
    Material actorMaterial, outsidePortalMaterialFront;
    StencilActorsManager stencilActorsManager;

    Collider col;
    public bool insidePortal = false;

    [SerializeField]
    bool needsCuttingAdjustment = false, needsRenderingAdjustment = false;

    [HideInInspector]
    public int stencilInsideIndex;
    int stencilStartIndex;
    MeshRenderer meshRenderer;
    
    void Awake()
    {
        col = GetComponent<Collider>();
        stencilStartIndex = actorMaterial.GetInt("_StencilRef");
        meshRenderer = GetComponent<MeshRenderer>();
        stencilActorsManager = FindObjectOfType<StencilActorsManager>();

        if (stencilActorsManager != null && stencilActorsManager.getActiveActors && !insidePortal)
            stencilActorsManager.AddActiveActor(this);
    }

    void EnterPortal()
    {
        if (col != null)
            col.enabled = true;

        actorMaterial.SetInt("_StencilRef", 0);
        actorMaterial.SetVector("sliceNormal", new Vector4(0, 0, 0, 0));
        actorMaterial.SetVector("sliceCentre", new Vector4(0, 0, 0, 0));
    }

    void ExitPortal()
    {
        if (col != null)
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

        if (stencilActorsManager != null && stencilActorsManager.getActiveActors)
            stencilActorsManager.AddActiveActor(this);

        EnterPortal();
    }

    public void HandleCuttingPlane(Vector3 normal, Vector3 point)
    {
        if (!needsCuttingAdjustment || !insidePortal) return;

        actorMaterial.SetVector("sliceNormal", new Vector4(normal.x, normal.y, normal.z, 0));
        actorMaterial.SetVector("sliceCentre", new Vector4(point.x, point.y, point.z, 0));
    }

    public void HandleRenderingAdjustment(bool side, bool playerSide)
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