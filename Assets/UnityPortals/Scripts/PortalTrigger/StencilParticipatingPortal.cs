[System.Serializable]
public class StencilParticipatingPortal : ParticipatingPortalBehaviour
{
    public StencilPortal portal;
    public bool insidePortal;

    protected override void HandleRenderSide()
    {
        if (insidePortal)
        {
            if (renderSide == RenderSide.Back)
                portal.renderSideInside = PortalBehaviour.RenderSide.Back;
            else if (renderSide == RenderSide.Front)
                portal.renderSideInside = PortalBehaviour.RenderSide.Front;
            else if (renderSide == RenderSide.Switch)
                portal.renderSideInside = portal.renderSideInside == PortalBehaviour.RenderSide.Back ? PortalBehaviour.RenderSide.Front : PortalBehaviour.RenderSide.Back;
        }
        else 
        {
            if (renderSide == RenderSide.Back)
                portal.renderSideOutside = PortalBehaviour.RenderSide.Back;
            else if (renderSide == RenderSide.Front)
                portal.renderSideOutside = PortalBehaviour.RenderSide.Front;
            else if (renderSide == RenderSide.Switch)
                portal.renderSideOutside = portal.renderSideOutside == PortalBehaviour.RenderSide.Back ? PortalBehaviour.RenderSide.Front : PortalBehaviour.RenderSide.Back;
        }
    }

    protected override void HandleRenderType()
    {
        if (insidePortal)
        {
            if (renderType == RenderType.TwoSided)
                portal.oneSidedPortalInside = false;
            else if (renderType == RenderType.OneSided)
                portal.oneSidedPortalInside = true;
            else if (renderType == RenderType.Switch)
                portal.oneSidedPortalInside = !portal.oneSidedPortalInside;
        }
        else 
        {
            if (renderType == RenderType.TwoSided)
                portal.oneSidedPortalOutside = false;
            else if (renderType == RenderType.OneSided)
                portal.oneSidedPortalOutside = true;
            else if (renderType == RenderType.Switch)
                portal.oneSidedPortalOutside = !portal.oneSidedPortalOutside;
        }
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