using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(CharacterController))]

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float WalkSpeed = 4.5f;
    [SerializeField] private float RunningSpeed = 7.5f;

    [SerializeField] private float JumpForce = 8.0f;
    [SerializeField] private float Gravity = 20f;

    [SerializeField] private float LookSensitivity = 0.2f;
    [SerializeField] private float LookAngleLimit = 90f;

    [SerializeField] private Camera mainCamera;
    private CharacterController characterController;

    private InputAction moveInput;
    private InputAction runInput;

    private InputAction jumpInput;
    private bool jumped = false;

    private float currentMoveSpeed = 0.0f;
    private Vector3 moveDirection = Vector3.zero;
    private float lookAngle = 0.0f;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();

        moveInput = InputSystem.actions.FindAction("Move");
        runInput = InputSystem.actions.FindAction("Sprint");

        /*
        jumpInput = InputSystem.actions.FindAction("Jump");
        jumpInput.started += Jumped;
        */

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentMoveSpeed = WalkSpeed;
    }

    private void Update()
    {
        Vector2 moveVector = moveInput.ReadValue<Vector2>();
        Vector2 mouseDelta = new Vector2(Mouse.current.delta.x.ReadValue(), Mouse.current.delta.y.ReadValue());

        if (!characterController.isGrounded) 
            jumped = false;

        //Ternary Operator example
        currentMoveSpeed = runInput.IsPressed() ? RunningSpeed : WalkSpeed;
        HandleMovement(moveVector);
        HandleLooking(mouseDelta);
    }

    private void HandleMovement(Vector2 moveVector) 
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float oldY = moveDirection.y;

        Vector2 newSpeed = new Vector2(moveVector.x * currentMoveSpeed, moveVector.y * currentMoveSpeed);

        moveDirection = (forward * newSpeed.y) + (right * newSpeed.x);

        if (jumped && characterController.isGrounded)
            moveDirection.y = JumpForce;
        else
            moveDirection.y = oldY;

        if (!characterController.isGrounded)
            moveDirection.y -= Gravity * Time.deltaTime; 

        characterController.Move(moveDirection * Time.deltaTime);
    }

    /*
    private void Jumped(InputAction.CallbackContext _)
    //CallbackContext contains data like button pressed holding or released 
    {
        jumped = true;
    }
    */

    private void HandleLooking(Vector2 mouseDelta) 
    {
        lookAngle += -mouseDelta.y * LookSensitivity;
        lookAngle = Mathf.Clamp(lookAngle, -LookAngleLimit, LookAngleLimit);

        mainCamera.transform.localRotation = Quaternion.Euler(lookAngle, 0, 0);
        transform.rotation *= Quaternion.Euler(0, mouseDelta.x * LookSensitivity, 0);
    }
}