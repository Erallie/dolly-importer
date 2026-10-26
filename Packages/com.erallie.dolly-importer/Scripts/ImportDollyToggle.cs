using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;

public class ImportDollyToggle : UdonSharpBehaviour
{
    [Tooltip("The Camera Dolly Animation that will be imported when this object is interacted with.")]
    public VRCCameraDollyAnimation cameraDolly;

    public override void Interact()
    {
        if (cameraDolly != null)
        {
            cameraDolly.Import();
        }
    }
}