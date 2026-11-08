
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float SlowWalkSpeed = 2.5f;
    [SerializeField] private float WalkSpeed = 3f;
    [SerializeField] private float RunSpeed = 1.5f;

    [Header("Stamina Settings")]
    [SerializeField] public float RunDuration = 5f;
    [SerializeField] public float currentRunDuration = 0.0f;

    [Header("Jump & Physics")]
    [SerializeField] private float JumpForce = 8.0f;
    [SerializeField] private float Gravity = 20f;

    [Header("Look Settings")]
    [SerializeField] private float LookSensitivity = 0.2f;
    [SerializeField] private float LookAngleLimit = 90f;

    [Header("Crouch Settings")]
    [SerializeField] private float crouchingHeight = 1.1f;
    [SerializeField] private float crouchCameraDrop = 0.55f;
    [SerializeField] private float crouchSpeedMultiplier = 0.55f;
    [SerializeField] private float crouchTransitionSpeed = 4f;

    [SerializeField] private bool isCrouching;

    [SerializeField] private Camera mainCamera;

    private CharacterController characterController;

    private InputAction moveInput;
    private InputAction runInput;
    private InputAction jumpInput;

    private bool jumped = false;
    private float currentMoveSpeed = 0.0f;

    private bool isExhausted = false;
    public bool IsExhausted => isExhausted;

    public bool IsCrouching => isCrouching;

    private Vector3 moveDirection = Vector3.zero;
    private float lookAngle = 0.0f;

    private float standingHeight;
    private Vector3 standingCenter;
    private Vector3 standingCameraPosition;

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
        currentRunDuration = RunDuration;

        standingHeight = characterController.height;
        standingCenter = characterController.center;

        if (mainCamera != null)
        {
            standingCameraPosition =
                mainCamera.transform.localPosition;
        }
        else
        {
            Debug.LogError("Main Camera null!", this);
        }

        crouchingHeight = Mathf.Clamp(
            crouchingHeight,
            characterController.radius * 2f,
            standingHeight
        );
    }

    private void Update()
    {
        Vector2 moveVector = moveInput.ReadValue<Vector2>();

        Vector2 mouseDelta = Vector2.zero;

        if (Mouse.current != null)
        {
            mouseDelta = new Vector2(
                Mouse.current.delta.x.ReadValue(),
                Mouse.current.delta.y.ReadValue()
            );
        }

        HandleCrouching();

        if (isExhausted && currentRunDuration < RunDuration)
        {
            currentMoveSpeed = SlowWalkSpeed;
            currentRunDuration += Time.deltaTime;

            if (currentRunDuration >= RunDuration)
            {
                currentRunDuration = RunDuration;
                isExhausted = false;
            }
        }
        else
        {
            if (!isCrouching &&
                runInput.IsPressed() &&
                moveVector.sqrMagnitude > 0f)
            {
                currentMoveSpeed = RunSpeed;
                currentRunDuration -= Time.deltaTime;

                if (currentRunDuration <= 0f)
                {
                    currentRunDuration = 0f;
                    isExhausted = true;
                }
            }
            else
            {
                currentMoveSpeed = WalkSpeed;

                currentRunDuration += Time.deltaTime;

                currentRunDuration = Mathf.Clamp(
                    currentRunDuration,
                    0f,
                    RunDuration
                );
            }
        }

        float finalMoveSpeed = currentMoveSpeed;

        if (isCrouching)
        {
            finalMoveSpeed *= crouchSpeedMultiplier;
        }

        HandleMovement(moveVector, finalMoveSpeed);
        HandleLooking(mouseDelta);
    }

    private void HandleCrouching()
    {
        if (Keyboard.current == null)
            return;

        bool crouchHeld =
            Keyboard.current.leftCtrlKey.isPressed ||
            Keyboard.current.rightCtrlKey.isPressed;

        if (crouchHeld)
        {
            isCrouching = true;
        }
        else if (CanStandUp())
        {
            isCrouching = false;
        }

        float targetHeight = isCrouching
            ? crouchingHeight
            : standingHeight;

        characterController.height = Mathf.MoveTowards(
            characterController.height,
            targetHeight,
            crouchTransitionSpeed * Time.deltaTime
        );

        Vector3 newCenter = standingCenter;

        newCenter.y -=
            (standingHeight - characterController.height) * 0.5f;

        characterController.center = newCenter;

        if (mainCamera != null)
        {
            Vector3 targetCameraPosition =
                standingCameraPosition;

            if (isCrouching)
            {
                targetCameraPosition +=
                    Vector3.down * crouchCameraDrop;
            }

            mainCamera.transform.localPosition =
                Vector3.MoveTowards(
                    mainCamera.transform.localPosition,
                    targetCameraPosition,
                    crouchTransitionSpeed * Time.deltaTime
                );
        }
    }

    private bool CanStandUp()
    {
        if (characterController.height >= standingHeight - 0.01f)
            return true;

        float bottomY =
            characterController.center.y -
            characterController.height * 0.5f;

        float radius = characterController.radius *
            Mathf.Max(
                Mathf.Abs(transform.lossyScale.x),
                Mathf.Abs(transform.lossyScale.z)
            );

        Vector3 lowerPoint = transform.TransformPoint(
            new Vector3(
                standingCenter.x,
                bottomY + radius,
                standingCenter.z
            )
        );

        Vector3 upperPoint = transform.TransformPoint(
            new Vector3(
                standingCenter.x,
                bottomY + standingHeight - radius,
                standingCenter.z
            )
        );

        Collider[] overlaps = Physics.OverlapCapsule(
            lowerPoint,
            upperPoint,
            radius * 0.95f,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider col in overlaps)
        {
            if (col == characterController ||
                col.transform == transform ||
                col.transform.IsChildOf(transform))
            {
                continue;
            }

            return false;
        }

        return true;
    }

    private void HandleMovement(Vector2 moveVector, float moveSpeed)
    {
        Vector3 forward =
            transform.TransformDirection(Vector3.forward);

        Vector3 right =
            transform.TransformDirection(Vector3.right);

        float oldY = moveDirection.y;

        Vector2 newSpeed = new Vector2(
            moveVector.x * moveSpeed,
            moveVector.y * moveSpeed
        );

        moveDirection =
            (forward * newSpeed.y) +
            (right * newSpeed.x);

        if (jumped && characterController.isGrounded)
            moveDirection.y = JumpForce;
        else
            moveDirection.y = oldY;

        if (!characterController.isGrounded)
            moveDirection.y -= Gravity * Time.deltaTime;

        characterController.Move(
            moveDirection * Time.deltaTime
        );
    }

    private void HandleLooking(Vector2 mouseDelta)
    {
        lookAngle += -mouseDelta.y * LookSensitivity;

        lookAngle = Mathf.Clamp(
            lookAngle,
            -LookAngleLimit,
            LookAngleLimit
        );

        mainCamera.transform.localRotation =
            Quaternion.Euler(lookAngle, 0, 0);

        transform.rotation *= Quaternion.Euler(
            0,
            mouseDelta.x * LookSensitivity,
            0
        );
    }
    public void PrepareForHiding()
    {
        isCrouching = false;

        characterController.height = standingHeight;
        characterController.center = standingCenter;

        if (mainCamera != null)
        {
            mainCamera.transform.localPosition =
                standingCameraPosition;
        }
    }

    public void SyncLookAngleFromCamera()
    {
        if (mainCamera == null)
            return;

        float cameraAngle =
            mainCamera.transform.localEulerAngles.x;

        if (cameraAngle > 180f)
            cameraAngle -= 360f;

        lookAngle = Mathf.Clamp(
            cameraAngle,
            -LookAngleLimit,
            LookAngleLimit
        );
    }
}
