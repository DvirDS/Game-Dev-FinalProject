using UnityEngine;

public class PlayerInputReader : MonoBehaviour
{
    private PlayerInputActions actions;

    public Vector2 Move { get; private set; }
    public bool FireHeld { get; private set; }
    public bool SwitchNextPressed { get; private set; }
    public bool SwitchPrevPressed { get; private set; }
    public bool InteractPressed { get; private set; }
    public bool PausePressed { get; private set; }
    public bool JumpPressed { get; private set; }    
    public bool SprintHeld { get; private set; }  

    void Awake()
    {
        actions = new PlayerInputActions();

        actions.Player.Move.performed += ctx => Move = ctx.ReadValue<Vector2>();
        actions.Player.Move.canceled += _ => Move = Vector2.zero;

        actions.Player.Fire.performed += _ => FireHeld = true;
        actions.Player.Fire.canceled += _ => FireHeld = false;

        actions.Player.SwitchNext.performed += _ => SwitchNextPressed = true;
        actions.Player.SwitchPrev.performed += _ => SwitchPrevPressed = true;
        actions.Player.Interact.performed += _ => InteractPressed = true;

        actions.Player.Jump.performed += _ => JumpPressed = true;
        actions.Player.Sprint.performed += _ => SprintHeld = true;
        actions.Player.Sprint.canceled += _ => SprintHeld = false;
        
        actions.Player.Pause.performed += _ => PausePressed = true;
    }

    void OnEnable() => actions.Enable();
    void OnDisable() => actions.Disable();

    void LateUpdate()
    {
        SwitchNextPressed = false;
        SwitchPrevPressed = false;
        InteractPressed = false;
        JumpPressed = false;
        PausePressed = false;
    }
}
