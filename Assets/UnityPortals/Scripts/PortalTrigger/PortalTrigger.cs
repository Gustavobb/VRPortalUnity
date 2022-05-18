using System.Collections.Generic;
using UnityEngine;

public class PortalTrigger : MonoBehaviour
{
    [SerializeField]
    TravelerBehaviour traveler;

    [SerializeField]
    PortalBehaviour portalActor;

    enum TriggerType {Area, Teleport, OnTravelerSwitchSides, NoTrigger};
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
    List<CameraParticipatingPortal> cameraParticipatingPortals = new List<CameraParticipatingPortal>();

    [SerializeField]    
    List<StencilParticipatingPortal> stencilParticipatingPortals = new List<StencilParticipatingPortal>();

    bool travelerPreviousSide;
    void Awake() 
    {
        if (triggerType == TriggerType.Area)
        {
            GetComponent<BoxCollider>().isTrigger = true;
            return;
        }

        else if (triggerType == TriggerType.Teleport)
            traveler.onTraveledPortal += OnTravelerTeleported;

        GetComponent<BoxCollider>().enabled = false;
    }

    void Start()
    {
        if (triggerType == TriggerType.OnTravelerSwitchSides)
            travelerPreviousSide = portalActor.playerSide;
    }

    void LateUpdate()
    {
        if (triggered || triggerType == TriggerType.NoTrigger)
            HandleParticipatingPortalsTrigger();
        
        if (triggerType == TriggerType.OnTravelerSwitchSides)
        {
            if (travelerPreviousSide != portalActor.playerSide)
                HandleParticipatingPortalsTrigger();

            travelerPreviousSide = portalActor.playerSide;
        }
    }

    void OnTravelerTeleported()
    {
        print("OnTravelerTeleported");
        if (portalActor == null)
            triggered = true;
        else if (portalActor == traveler.portal)
            triggered = true;
    }

    void OnTriggerEnter(Collider other) 
    {
        if (other.GetComponent<TravelerBehaviour>() == traveler && triggerType == TriggerType.Area)
            triggered = true;         
    }

    void HandleParticipatingPortalsTrigger()
    {
        if (cameraParticipatingPortals.Count == 0 && stencilParticipatingPortals.Count == 0)
            return;

        List<CameraParticipatingPortal> copy = new List<CameraParticipatingPortal>(cameraParticipatingPortals);
        foreach (CameraParticipatingPortal portal in copy)
        {
            if (portal.HandleTrigger(traveler) && portal.removeAfterTrigger)
                cameraParticipatingPortals.Remove(portal);
        }

        List<StencilParticipatingPortal> copy2 = new List<StencilParticipatingPortal>(stencilParticipatingPortals);
        foreach (StencilParticipatingPortal portal in copy2)
        {
            if (portal.HandleTrigger(traveler) && portal.removeAfterTrigger)
                stencilParticipatingPortals.Remove(portal);
        }

        if (cameraParticipatingPortals.Count != 0 || stencilParticipatingPortals.Count != 0)
            return;

        if (nextPortalEventTrigger != null)
        {
            if (nextPortalEventTriggerActionType == NextPortalEventTriggerActionType.Off) 
                nextPortalEventTrigger.SetActive(false);
            else if (nextPortalEventTriggerActionType == NextPortalEventTriggerActionType.On) 
                nextPortalEventTrigger.SetActive(true);
            else if (nextPortalEventTriggerActionType == NextPortalEventTriggerActionType.Switch) 
                nextPortalEventTrigger.SetActive(!nextPortalEventTrigger.activeSelf);
        }
        
        if (disableAfterTrigger)
            gameObject.SetActive(false);
        
        triggered = false;
    }
}