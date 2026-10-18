using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ArrowQuiver : MonoBehaviour
{
    [Header("Quiver")]
    public Arrow arrowPrefab;
    public float quiverRadius = 0.2f; // rayon de la zone derrière la nuque

    [Header("Hands")]
    public Transform rightHand; // Right Controller
    public Transform leftHand;
    public InputActionReference rightGrip;  // XRI Right Interaction/Select
    public InputActionReference leftGrip;
    public XRBaseInteractor rightInteractor; // Right Controller > Near-Far Interactor
    public XRBaseInteractor leftInteractor;

 
       private void Start()
    {
        if (leftHand == null) leftHand = PlayerRig.Instance.leftHand;
        if (rightHand == null) rightHand = PlayerRig.Instance.rightHand;
        if (leftGrip == null) leftGrip = PlayerRig.Instance.leftGrip;
        if (rightGrip == null) rightGrip = PlayerRig.Instance.rightGrip;
        if (leftInteractor == null) leftInteractor = PlayerRig.Instance.leftInteractor;
        if (rightInteractor == null) rightInteractor = PlayerRig.Instance.rightInteractor;
    }


    private void Update()
    {
        TryGiveArrow(rightHand, rightGrip, rightInteractor);
        TryGiveArrow(leftHand, leftGrip, leftInteractor);
    }

    private void TryGiveArrow(Transform hand, InputActionReference gripAction, XRBaseInteractor interactor) 
    {
        bool handInQuiver = Vector3.Distance(hand.position, transform.position) < quiverRadius;
        bool justPressed = gripAction.action.WasPressedThisFrame();
        bool handEmpty = !interactor.hasSelection;
        if (handInQuiver && justPressed && handEmpty)
        {
            Arrow arrow = Instantiate(arrowPrefab, hand.position, hand.rotation);
            interactor.interactionManager.SelectEnter((IXRSelectInteractor)interactor, (IXRSelectInteractable)arrow.Grab);
        }
    }

}
