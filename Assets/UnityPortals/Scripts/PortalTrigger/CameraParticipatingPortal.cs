[System.Serializable]
public class CameraParticipatingPortal : ParticipatingPortalBehaviour
{
    public CameraPortal portal;

    protected override void HandleRenderSide()
    {
        if (renderSide == RenderSide.Back)
            portal.renderSide = PortalBehaviour.RenderSide.Back;
        else if (renderSide == RenderSide.Front)
            portal.renderSide = PortalBehaviour.RenderSide.Front;
        else if (renderSide == RenderSide.Switch)
            portal.renderSide = portal.renderSide == PortalBehaviour.RenderSide.Back ? PortalBehaviour.RenderSide.Front : PortalBehaviour.RenderSide.Back;
    }
    
    protected override void HandleRenderType()
    {
        if (renderType == RenderType.TwoSided)
            portal.oneSidedPortal = false;
        else if (renderType == RenderType.OneSided)
            portal.oneSidedPortal = true;
        else if (renderType == RenderType.Switch)
            portal.oneSidedPortal = !portal.oneSidedPortal;
    }

    protected override bool HandleConditions(PortalBehaviour p, TravelerBehaviour t)
    {
        return base.HandleConditions(p, t);
    }

    public override bool HandleTrigger(TravelerBehaviour t)
    {
        return HandleConditions(portal, t);
    }
}