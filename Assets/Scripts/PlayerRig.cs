using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class PlayerRig : MonoBehaviour
{
    public static PlayerRig Instance;   // accessible partout : PlayerRig.Instance

    [Header("Hands")]
    public Transform rightHand; // Right Controller
    public Transform leftHand;
    public InputActionReference rightGrip;  // XRI Right Interaction/Select
    public InputActionReference leftGrip;
    public XRBaseInteractor rightInteractor;    // Right Controller > Near-Far Interactor
    public XRBaseInteractor leftInteractor;

    private void Awake()
    {
        Instance = this;
    }
}
