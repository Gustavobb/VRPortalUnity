[System.Serializable]
public class ParticipatingPortalBehaviour
{
    public enum TriggerCond {NoCond, OnRendering, OnNotRendering, OnSaw, OnNotSaw, OnSwitchSides, OnFrontSide, OnBackSide};
    public TriggerCond triggerCond;
    public bool changeRenderType;
    public enum RenderType {TwoSided, OneSided, Switch};
    public RenderType renderType;
    public enum ActionType {DontChange, ChangeRenderSide, ChangeRenderState};
    public ActionType actionType;
    public enum RenderState {Off, On, Switch};
    public RenderState renderState;
    public enum RenderSide {Back, Front, Switch};
    public RenderSide renderSide;
    public bool removeAfterTrigger;
    protected int previousSide = -1;
    
    protected virtual void HandleRenderSide()
    {
    }
    
    protected virtual void HandleRenderType()
    {
    }

    protected virtual void HandleRenderState(PortalBehaviour p)
    {
        if (renderState == RenderState.Off)
            p.render = false;
        else if (renderState == RenderState.On)
            p.render = true;
        else if (renderState == RenderState.Switch)
            p.render = !p.render;
    }

    protected virtual bool HandleConditions(PortalBehaviour p, TravelerBehaviour t)
    {
        if (triggerCond == TriggerCond.OnRendering)
        {
            if (!p.needsToBeRendered) return false;
        }
        else if (triggerCond == TriggerCond.OnNotRendering)
        {
            if (p.needsToBeRendered) return false;
        }
        else if (triggerCond == TriggerCond.OnSaw)
        {
            if (!p.canBeSeen && !p.oneSidedPlaneCanBeSeen) return false;
        }
        else if (triggerCond == TriggerCond.OnNotSaw)
        {
            if (p.canBeSeen && p.oneSidedPlaneCanBeSeen) return false;
        }
        else if (triggerCond == TriggerCond.OnFrontSide)
        {
            if (!p.playerSide) return false;
        }
        else if (triggerCond == TriggerCond.OnBackSide)
        {
            if (p.playerSide) return false;
        }
        else if (triggerCond == TriggerCond.OnSwitchSides)
        {
            if (previousSide == -1 || t.portal != null)
            {
                previousSide = p.playerSide ? 1 : 0;
                return false;
            }

            int currentSide = p.playerSide ? 1 : 0;
            if (previousSide == currentSide) 
                return false;
            
            previousSide = currentSide;
        }
    
        if (changeRenderType)
            HandleRenderType();
        if (actionType == ActionType.ChangeRenderSide)
            HandleRenderSide();
        else if (actionType == ActionType.ChangeRenderState)
            HandleRenderState(p);
        
        return true;
    }

    public virtual bool HandleTrigger(TravelerBehaviour t)
    {
        return true;
    }
}