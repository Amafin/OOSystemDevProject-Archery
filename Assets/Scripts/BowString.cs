using NUnit.Framework;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class BowString : XRBaseInteractable
{
    [SerializeField] XRGrabInteractable bowGrab;
    [SerializeField] Transform restPoint;
    [SerializeField] Transform fullDrawPoint;
    [SerializeField] Transform StringBone;

    public float PullAmount { get; private set; }
    IXRSelectInteractor puller;

    // tirer cord que si on tient déjà l'arc
    public override bool IsSelectableBy(IXRSelectInteractor interactor)
    {
        return base.IsSelectableBy(interactor) && bowGrab.isSelected && !bowGrab.interactorsSelecting.Contains(interactor);
    }
}
