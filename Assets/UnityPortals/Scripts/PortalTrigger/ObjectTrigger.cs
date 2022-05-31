using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ObjectTrigger
{
    public GameObject gameObject;
    public enum RenderState {Off, On, Switch, Destroy};
    public RenderState objectActiveState;
    
    public void HandleRenderState()
    {
        if (objectActiveState == RenderState.Off)
            gameObject.SetActive(false);
        else if (objectActiveState == RenderState.On)
            gameObject.SetActive(true);
        else if (objectActiveState == RenderState.Switch)
            gameObject.SetActive(!gameObject.activeSelf);
        else if (objectActiveState == RenderState.Destroy)
            GameObject.Destroy(gameObject);
    }
}