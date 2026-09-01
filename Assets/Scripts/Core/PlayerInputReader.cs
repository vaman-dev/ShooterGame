using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference sprintAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference fireActionRef;

    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool IsSprintHeld { get; private set; }
    public bool IsAimHeld { get; private set; }
    public bool IsFirePressed { get; private set; }

    private bool jumpPressed;
    private bool coverPressed;
    private bool reloadPressed;

    private InputAction aimAction;
    private InputAction coverAction;
    private InputAction fireAction;
    private InputAction reloadAction;

    private void Awake()
    {
        // Aim, Cover, Fire, and Reload all belong to the same Player action map as movement.
        // Looking them up from an existing serialized action avoids fragile scene references.
        aimAction = moveAction.action.actionMap.FindAction("Aim", throwIfNotFound: true);
        coverAction = moveAction.action.actionMap.FindAction("Cover", throwIfNotFound: true);
        fireAction = fireActionRef.action.actionMap.FindAction("Fire", throwIfNotFound: true);
        reloadAction = moveAction.action.actionMap.FindAction("Reload", throwIfNotFound: true);
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        sprintAction.action.Enable();
        jumpAction.action.Enable();
        lookAction.action.Enable();
        aimAction.Enable();
        coverAction.Enable();
        fireAction.Enable();
        reloadAction.Enable();

        jumpAction.action.performed += OnJumpPerformed;
        coverAction.performed += OnCoverPerformed;
        reloadAction.performed += OnReloadPerformed;
    }

    private void OnDisable()
    {
        jumpAction.action.performed -= OnJumpPerformed;
        coverAction.performed -= OnCoverPerformed;
        reloadAction.performed -= OnReloadPerformed;

        moveAction.action.Disable();
        sprintAction.action.Disable();
        jumpAction.action.Disable();
        lookAction.action.Disable();
        aimAction.Disable();
        coverAction.Disable();
        fireAction.Disable();
        reloadAction.Disable();
    }

    private void Update()
    {
        MoveInput = moveAction.action.ReadValue<Vector2>();
        LookInput = lookAction.action.ReadValue<Vector2>();
        IsSprintHeld = sprintAction.action.IsPressed();
        IsAimHeld = aimAction.IsPressed();

        // Fire is held (automatic weapons), not a one-shot consume — matches your original intent.
        IsFirePressed = fireAction.IsPressed();
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        jumpPressed = true;
    }

    public bool ConsumeJumpPressed()
    {
        if (!jumpPressed)
            return false;

        jumpPressed = false;
        return true;
    }

    private void OnCoverPerformed(InputAction.CallbackContext context)
    {
        coverPressed = true;
    }

    public bool ConsumeCoverPressed()
    {
        if (!coverPressed)
            return false;

        coverPressed = false;
        return true;
    }

    private void OnReloadPerformed(InputAction.CallbackContext context)
    {
        reloadPressed = true;
    }

    public bool ConsumeReloadPressed()
    {
        if (!reloadPressed)
            return false;

        reloadPressed = false;
        return true;
    }
}