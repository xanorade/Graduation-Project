
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerHiding : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject hideMask;

    [Header("Camera Look")]
    [SerializeField] private float hiddenLookSensitivity = 0.15f;
    [SerializeField] private float horizontalLookLimit = 25f;
    [SerializeField] private float verticalLookLimit = 12f;

    [Header("Debug")]
    [SerializeField] private bool isHiding;

    private HideSpot currentHideSpot;

    private Vector3 savedCameraLocalPosition;
    private Quaternion savedCameraLocalRotation;

    private Quaternion hiddenCameraBaseRotation;

    private float hiddenYaw;
    private float hiddenPitch;

    private bool canExitHide;

    private bool previousControllerEnabled;

    public bool IsHiding => isHiding;

    private void Awake()
    {
        if (characterController == null)
            characterController =
                GetComponent<CharacterController>();

        if (playerController == null)
            playerController =
                GetComponent<PlayerController>();

        if (playerCamera == null)
            playerCamera =
                GetComponentInChildren<Camera>();

        if (hideMask != null)
            hideMask.SetActive(false);
    }

    private void Update()
    {
        if (!isHiding)
            return;

        HandleHiddenCamera();

        if (Keyboard.current == null)
            return;

        if (!Keyboard.current.eKey.isPressed)
        {
            canExitHide = true;
            return;
        }

        if (canExitHide &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            ExitHide();
        }
    }

    public void EnterHide(HideSpot spot)
    {
        if (isHiding || spot == null)
            return;

        if (spot.HidePoint == null ||
            spot.CameraPoint == null ||
            spot.ExitPoint == null)
        {
            Debug.LogError(
                "HideSpot position referanslari eksik!",
                spot
            );

            return;
        }

        if (playerController == null ||
            characterController == null ||
            playerCamera == null)
        {
            Debug.LogError(
                "PlayerHiding referanslari eksik!",
                this
            );

            return;
        }

        currentHideSpot = spot;

        playerController.PrepareForHiding();

        savedCameraLocalPosition =
            playerCamera.transform.localPosition;

        savedCameraLocalRotation =
            playerCamera.transform.localRotation;

        previousControllerEnabled =
            playerController.enabled;

        playerController.enabled = false;

        characterController.enabled = false;

        transform.SetPositionAndRotation(
            spot.HidePoint.position,
            spot.HidePoint.rotation
        );

        characterController.enabled = true;

        playerCamera.transform.position =
            spot.CameraPoint.position;

        playerCamera.transform.rotation =
            spot.CameraPoint.rotation;

        hiddenCameraBaseRotation =
            spot.CameraPoint.rotation;

        hiddenYaw = 0f;
        hiddenPitch = 0f;

        isHiding = true;
        canExitHide = false;

        if (hideMask != null)
            hideMask.SetActive(true);
    }

    private void HandleHiddenCamera()
    {
        if (Mouse.current == null)
            return;

        Vector2 mouseDelta =
            Mouse.current.delta.ReadValue();

        hiddenYaw +=
            mouseDelta.x * hiddenLookSensitivity;

        hiddenPitch -=
            mouseDelta.y * hiddenLookSensitivity;

        hiddenYaw = Mathf.Clamp(
            hiddenYaw,
            -horizontalLookLimit,
            horizontalLookLimit
        );

        hiddenPitch = Mathf.Clamp(
            hiddenPitch,
            -verticalLookLimit,
            verticalLookLimit
        );

        playerCamera.transform.rotation =
            hiddenCameraBaseRotation *
            Quaternion.Euler(
                hiddenPitch,
                hiddenYaw,
                0f
            );
    }

    public void ExitHide()
    {
        if (!isHiding || currentHideSpot == null)
            return;

        if (hideMask != null)
            hideMask.SetActive(false);

        characterController.enabled = false;

        transform.SetPositionAndRotation(
            currentHideSpot.ExitPoint.position,
            currentHideSpot.ExitPoint.rotation
        );

         playerCamera.transform.localPosition =
            savedCameraLocalPosition;

        playerCamera.transform.localRotation =
            savedCameraLocalRotation;

        characterController.enabled = true;

        isHiding = false;
        currentHideSpot = null;

        if (playerController != null)
        {
            playerController.SyncLookAngleFromCamera();

            playerController.enabled =
                previousControllerEnabled;
        }
    }
}
