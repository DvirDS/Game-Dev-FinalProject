using UnityEngine;

public class PlayerInputReader : MonoBehaviour
{
    private PlayerInputActions _actions;

    public Vector2 Move { get; private set; }
    public bool FireHeld { get; private set; }
    public bool SwitchNextPressed { get; private set; }
    public bool SwitchPrevPressed { get; private set; }
    public bool InteractPressed { get; private set; }

    // חדש: קפיצה וספרינט
    public bool JumpPressed { get; private set; }     // אירוע חד-פעמי לפריים
    public bool SprintHeld { get; private set; }     // מוחזק/לא מוחזק

    void Awake()
    {
        _actions = new PlayerInputActions();

        _actions.Player.Move.performed += ctx => Move = ctx.ReadValue<Vector2>();
        _actions.Player.Move.canceled += _ => Move = Vector2.zero;

        _actions.Player.Fire.performed += _ => FireHeld = true;
        _actions.Player.Fire.canceled += _ => FireHeld = false;

        _actions.Player.SwitchNext.performed += _ => SwitchNextPressed = true;
        _actions.Player.SwitchPrev.performed += _ => SwitchPrevPressed = true;
        _actions.Player.Interact.performed += _ => InteractPressed = true;

        // חדש: Jump + Sprint
        _actions.Player.Jump.performed += _ => JumpPressed = true;
        _actions.Player.Sprint.performed += _ => SprintHeld = true;
        _actions.Player.Sprint.canceled += _ => SprintHeld = false;
    }

    void OnEnable() => _actions.Enable();
    void OnDisable() => _actions.Disable();

    void LateUpdate()
    {
        // איפוס טריגרים של פריים אחד
        SwitchNextPressed = false;
        SwitchPrevPressed = false;
        InteractPressed = false;
        JumpPressed = false;
    }
}
