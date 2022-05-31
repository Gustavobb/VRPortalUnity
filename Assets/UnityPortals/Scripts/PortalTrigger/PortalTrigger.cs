using UnityEngine;
using System.Collections.Generic;

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
    List<CameraParticipatingPortal> cameraParticipatingPortalsCopy;

    [SerializeField]    
    List<StencilParticipatingPortal> stencilParticipatingPortals = new List<StencilParticipatingPortal>();
    List<StencilParticipatingPortal> stencilParticipatingPortalsCopy;

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
        cameraParticipatingPortalsCopy = new List<CameraParticipatingPortal>(cameraParticipatingPortals);
        stencilParticipatingPortalsCopy = new List<StencilParticipatingPortal>(stencilParticipatingPortals);
    }

    void OnEnable()
    {
        if (triggerType == TriggerType.OnTravelerSwitchSides)
            travelerPreviousSide = portalActor.playerSide;
    }

    void LateUpdate()
    {
        if (triggered || triggerType == TriggerType.NoTrigger)
            HandleParticipatingPortalsTrigger();
        
        if (triggerType == TriggerType.OnTravelerSwitchSides && !triggered)
        {
            if (travelerPreviousSide != portalActor.playerSide && traveler.portal == null)
                triggered = true;

            travelerPreviousSide = portalActor.playerSide;
        }
    }

    void OnTravelerTeleported()
    {
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

        cameraParticipatingPortals = new List<CameraParticipatingPortal>(cameraParticipatingPortalsCopy);
        stencilParticipatingPortals = new List<StencilParticipatingPortal>(stencilParticipatingPortalsCopy);

        if (disableAfterTrigger)
            gameObject.SetActive(false);
        
        triggered = false;
    }
}