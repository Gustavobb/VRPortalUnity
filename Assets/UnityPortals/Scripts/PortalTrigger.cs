using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTrigger : MonoBehaviour
{
    [SerializeField]
    TravelerBehaviour traveler;

    enum TriggerType {Area, Teleport};
    [SerializeField]
    TriggerType triggerType;

    [SerializeField]
    bool disableAfterTrigger = true;
    bool triggered = false;

    [SerializeField]
    GameObject nextPortalEventTrigger;

    enum NextPortalEventTriggerActionType {On, Off, Switch};
    [SerializeField]
    NextPortalEventTriggerActionType nextPortalEventTriggerActionType;

    [SerializeField]
    public List<ParticipatingPortal> participatingPortals = new List<ParticipatingPortal>();

    void Awake() 
    {
        if (triggerType == TriggerType.Area)
            GetComponent<BoxCollider>().isTrigger = true;

        else if (triggerType == TriggerType.Teleport)
        {
            traveler.onTraveledPortal += OnTravelerTeleported;
            GetComponent<BoxCollider>().enabled = false;
        }
    }

    void Start()
    {
        if (nextPortalEventTrigger != null)
            nextPortalEventTrigger.SetActive(false);
    }

    void LateUpdate()
    {
        if (triggered)
            HandleParticipatingPortalsTrigger();
    }

    void OnTravelerTeleported()
    {
        triggered = true;
    }

    void OnTriggerEnter(Collider other) 
    {
        if (other.GetComponent<TravelerBehaviour>() == traveler && triggerType == TriggerType.Area)
            triggered = true;         
    }

    void HandleParticipatingPortalsTrigger()
    {
        if (participatingPortals.Count == 0) return;
        List<ParticipatingPortal> copy = new List<ParticipatingPortal>(participatingPortals);
        foreach (ParticipatingPortal portal in copy)
        {
            if (portal.HandleTrigger())
            {
                participatingPortals.Remove(portal);
            }
        }

        if (participatingPortals.Count != 0) return;
        if (nextPortalEventTrigger != null)
        {
            if (nextPortalEventTriggerActionType == NextPortalEventTriggerActionType.Off) 
                nextPortalEventTrigger.SetActive(false);
            else if (nextPortalEventTriggerActionType == NextPortalEventTriggerActionType.On) 
                nextPortalEventTrigger.SetActive(true);
            else if (nextPortalEventTriggerActionType == NextPortalEventTriggerActionType.Switch) 
                nextPortalEventTrigger.SetActive(!nextPortalEventTrigger.activeSelf);
        }
        
        triggered = false;
        if (disableAfterTrigger)
            gameObject.SetActive(false);
    }
}

[System.Serializable]
public class ParticipatingPortal
{
    public PortalBehaviour portal;
    public enum TriggerCond {NoCond, OnRendering, OnNotRendering, OnSaw, OnNotSaw};
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
    
    void HandleRenderSide()
    {
        if (renderSide == RenderSide.Back)
            portal.renderSide = PortalBehaviour.RenderSide.Back;
        else if (renderSide == RenderSide.Front)
            portal.renderSide = PortalBehaviour.RenderSide.Front;
        else if (renderSide == RenderSide.Switch)
            portal.renderSide = portal.renderSide == PortalBehaviour.RenderSide.Back ? PortalBehaviour.RenderSide.Front : PortalBehaviour.RenderSide.Back;
    }

    void HandleRenderState()
    {
        if (renderState == RenderState.Off)
            portal.render = false;
        else if (renderState == RenderState.On)
            portal.render = true;
        else if (renderState == RenderState.Switch)
            portal.render = !portal.render;
    }

    void HandleRenderType()
    {
        if (renderType == RenderType.TwoSided)
            portal.oneSidedPortal = false;
        else if (renderType == RenderType.OneSided)
            portal.oneSidedPortal = true;
        else if (renderType == RenderType.Switch)
            portal.oneSidedPortal = !portal.oneSidedPortal;
    }

    public bool HandleTrigger()
    {
        if (triggerCond == TriggerCond.OnRendering)
        {
            if (!portal.needsToBeRendered) return false;
        }
        else if (triggerCond == TriggerCond.OnNotRendering)
        {
            if (portal.needsToBeRendered) return false;
        }
        else if (triggerCond == TriggerCond.OnSaw)
        {
            if (!portal.canBeSeen && !portal.oneSidedPlaneCanBeSeen) return false;
        }
        else if (triggerCond == TriggerCond.OnNotSaw)
        {
            if (portal.canBeSeen && portal.oneSidedPlaneCanBeSeen) return false;
        }
    
        if (changeRenderType)
            HandleRenderType();
        if (actionType == ActionType.ChangeRenderSide)
            HandleRenderSide();
        else if (actionType == ActionType.ChangeRenderState)
            HandleRenderState();
        
        return true;
    }
}