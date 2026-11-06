
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerInteractor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private TMP_Text interactionPrompt;

    [Header("Message UI")]
    [SerializeField] private TMP_Text doorInspectionText;
    [SerializeField] private TMP_Text innerMonologueText;

    [Header("Settings")]
    [SerializeField] private float interactionDistance = 2.5f;

    private DoorInteractable currentDoor;
    private DoorInteractable messageDoor;

    private float doorMessageEndTime;
    private float monologueEndTime;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        if (interactionPrompt != null)
            interactionPrompt.gameObject.SetActive(false);

        if (doorInspectionText != null)
            doorInspectionText.gameObject.SetActive(false);

        if (innerMonologueText != null)
            innerMonologueText.gameObject.SetActive(false);
    }

    private void Update()
    {
        FindTarget();
        UpdateMessages();

        if (IsMessageVisible())
        {
            if (interactionPrompt != null)
                interactionPrompt.gameObject.SetActive(false);

            return;
        }

        if (currentDoor != null)
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.text = currentDoor.GetPrompt();
                interactionPrompt.gameObject.SetActive(true);
            }

            if (Keyboard.current != null &&
                Keyboard.current.eKey.wasPressedThisFrame)
            {
                currentDoor.Interact(this);
            }
        }
        else
        {
            if (interactionPrompt != null)
                interactionPrompt.gameObject.SetActive(false);
        }
    }

    private void FindTarget()
    {
        currentDoor = null;

        if (playerCamera == null)
            return;

        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactionDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore))
        {
            currentDoor =
                hit.collider.GetComponentInParent<DoorInteractable>();
        }
    }

    private void UpdateMessages()
    {
        if (doorInspectionText != null &&
            doorInspectionText.gameObject.activeSelf)
        {
            bool outOfRange =
                messageDoor == null ||
                !messageDoor.IsWithinInteractionRange(
                    transform.position,
                    interactionDistance
                );

            bool expired = Time.time >= doorMessageEndTime;

            if (outOfRange || expired)
            {
                doorInspectionText.gameObject.SetActive(false);
                messageDoor = null;
            }
        }

        if (innerMonologueText != null &&
            innerMonologueText.gameObject.activeSelf)
        {
            if (Time.time >= monologueEndTime)
            {
                innerMonologueText.gameObject.SetActive(false);
            }
        }
    }

    private bool IsMessageVisible()
    {
        bool doorMessageActive =
            doorInspectionText != null &&
            doorInspectionText.gameObject.activeSelf;

        bool monologueActive =
            innerMonologueText != null &&
            innerMonologueText.gameObject.activeSelf;

        return doorMessageActive || monologueActive;
    }

    public void ShowDoorInspection(
        string message,
        DoorInteractable sourceDoor,
        float duration = 3.5f)
    {
        if (doorInspectionText == null ||
            string.IsNullOrWhiteSpace(message))
            return;

        if (innerMonologueText != null)
            innerMonologueText.gameObject.SetActive(false);

        doorInspectionText.text = message;
        doorInspectionText.gameObject.SetActive(true);

        messageDoor = sourceDoor;
        doorMessageEndTime = Time.time + duration;

        if (interactionPrompt != null)
            interactionPrompt.gameObject.SetActive(false);
    }

    public void ShowInnerMonologue(
        string message,
        float duration = 3.5f)
    {
        if (innerMonologueText == null ||
            string.IsNullOrWhiteSpace(message))
            return;

        if (doorInspectionText != null)
            doorInspectionText.gameObject.SetActive(false);

        messageDoor = null;

        innerMonologueText.text = message;
        innerMonologueText.gameObject.SetActive(true);

        monologueEndTime = Time.time + duration;

        if (interactionPrompt != null)
            interactionPrompt.gameObject.SetActive(false);
    }
}
