using System.Collections.Generic;

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
    public List<ObjectTrigger> objectsToTrigger = new List<ObjectTrigger>();
    public bool shrinkAndGrow = false;
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
        {
            if (shrinkAndGrow)
                p.ShrinkPortal(.5f);
            else
                p.render = false;
        }
        else if (renderState == RenderState.On)
        {
            if (shrinkAndGrow)
                p.GrowPortal(.5f);
            else
                p.render = true;
        }
        else if (renderState == RenderState.Switch)
        {
            if (shrinkAndGrow)
            {
                if (p.render)
                    p.ShrinkPortal(.5f);
                else
                    p.GrowPortal(.5f);
            }
            else
                p.render = !p.render;
        }
    }

    protected virtual bool HandleConditions(PortalBehaviour p, TravelerBehaviour t)
    {
        if (triggerCond == TriggerCond.OnRendering && !p.needsToBeRendered)
            return false;
        else if (triggerCond == TriggerCond.OnNotRendering && p.needsToBeRendered)
            return false;
        else if (triggerCond == TriggerCond.OnSaw && (!p.canBeSeen && !p.oneSidedPlaneCanBeSeen))
            return false;
        else if (triggerCond == TriggerCond.OnNotSaw && (p.canBeSeen && p.oneSidedPlaneCanBeSeen))
            return false;
        else if (triggerCond == TriggerCond.OnFrontSide && !p.playerSide)
            return false;
        else if (triggerCond == TriggerCond.OnBackSide && p.playerSide)
            return false;
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
        
        foreach (ObjectTrigger ot in objectsToTrigger)
            ot.HandleRenderState();

        return true;
    }

    public virtual bool HandleTrigger(TravelerBehaviour t)
    {
        return true;
    }
}