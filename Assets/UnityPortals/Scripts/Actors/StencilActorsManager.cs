using System.Collections.Generic;
using UnityEngine;

public class StencilActorsManager : MonoBehaviour
{
    public bool getActiveActors = false;
    public static List<StencilActor> activeActors = new List<StencilActor>();

    public void AddActiveActor(StencilActor actor)
    {
        activeActors.Add(actor);
    }

    public void ResetActiveMaterials()
    {
        activeActors = new List<StencilActor>();
    }
}
