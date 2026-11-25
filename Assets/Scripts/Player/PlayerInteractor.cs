
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

    [SerializeField] private PlayerHiding playerHiding;

    private HideSpot currentHideSpot;

    private DoorInteractable currentDoor;
    private DoorInteractable messageDoor;

    private InspectableObject currentInspectable;
    private InspectableObject messageInspectable;

    private ToiletInteractable currentToilet;
    private bool interactionLocked;
    public bool IsInteractionLocked => interactionLocked;

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

        if (interactionLocked)
        {
            if (interactionPrompt != null)
                interactionPrompt.gameObject.SetActive(false);

            return;
        }
        if (playerHiding != null && playerHiding.IsHiding)
        {
            if (interactionPrompt != null)
                interactionPrompt.gameObject.SetActive(false);

            return;
        }

        if (IsMessageVisible())
        {
            if (interactionPrompt != null)
                interactionPrompt.gameObject.SetActive(false);

            return;
        }

        if (currentHideSpot != null && playerHiding != null)
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.text = currentHideSpot.GetPrompt();
                interactionPrompt.gameObject.SetActive(true);
            }

            if (Keyboard.current != null &&
                Keyboard.current.eKey.wasPressedThisFrame)
            {
                playerHiding.EnterHide(currentHideSpot);
            }
        }
        else if (currentToilet != null)
        {
            if (currentToilet.CanInteract)
            {
                if (interactionPrompt != null)
                {
                    interactionPrompt.text = currentToilet.GetPrompt();
                    interactionPrompt.gameObject.SetActive(true);
                }

                if (Keyboard.current != null &&
                    Keyboard.current.eKey.wasPressedThisFrame)
                {
                    currentToilet.Interact(this);
                }
            }
            else
            {
                if (interactionPrompt != null)
                    interactionPrompt.gameObject.SetActive(false);
            }
        }
        else if (currentDoor != null)
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
        else if (currentInspectable != null)
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.text =
                    currentInspectable.GetPrompt();

                interactionPrompt.gameObject.SetActive(true);
            }

            if (Keyboard.current != null &&
                Keyboard.current.eKey.wasPressedThisFrame)
            {
                currentInspectable.Interact(this);
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
        currentHideSpot = null;
        currentInspectable = null;
        currentToilet = null;

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
            QueryTriggerInteraction.Collide))
        {
            currentHideSpot =
                hit.collider.GetComponentInParent<HideSpot>();

            if (currentHideSpot == null)
            {
                currentToilet =
                    hit.collider.GetComponentInParent<ToiletInteractable>();
            }

            if (currentHideSpot == null && currentToilet == null)
            {
                currentDoor =
                    hit.collider.GetComponentInParent<DoorInteractable>();
            }

            if (currentHideSpot == null &&
                currentDoor == null)
            {
                currentInspectable =
                    hit.collider.GetComponentInParent<InspectableObject>();
            }
        }
    }


    private void UpdateMessages()
    {
        // Door Inspection / Object Inspection kontrolü
        if (doorInspectionText != null &&
            doorInspectionText.gameObject.activeSelf)
        {
            bool outOfRange = true;

            if (messageDoor != null)
            {
                outOfRange =
                    !messageDoor.IsWithinInteractionRange(
                        transform.position,
                        interactionDistance
                    );
            }
            else if (messageInspectable != null)
            {
                outOfRange =
                    !messageInspectable.IsWithinInteractionRange(
                        transform.position,
                        interactionDistance
                    );
            }

            bool expired = Time.time >= doorMessageEndTime;

            if (outOfRange || expired)
            {
                doorInspectionText.gameObject.SetActive(false);

                messageDoor = null;
                messageInspectable = null;
            }
        }

        // Inner Monologue kontrolü
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
        messageInspectable = null;
        doorMessageEndTime = Time.time + duration;

        if (interactionPrompt != null)
            interactionPrompt.gameObject.SetActive(false);
    }
    public void ShowObjectInspection(
    string message,
    InspectableObject sourceObject,
    float duration = 3.5f)
    {
        if (doorInspectionText == null ||
            string.IsNullOrWhiteSpace(message))
            return;

        if (innerMonologueText != null)
            innerMonologueText.gameObject.SetActive(false);

        doorInspectionText.text = message;
        doorInspectionText.gameObject.SetActive(true);

        messageDoor = null;
        messageInspectable = sourceObject;

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
        messageInspectable = null;

        innerMonologueText.text = message;
        innerMonologueText.gameObject.SetActive(true);

        monologueEndTime = Time.time + duration;

        if (interactionPrompt != null)
            interactionPrompt.gameObject.SetActive(false);
    }

    public void SetInteractionLocked(bool locked)
    {
        interactionLocked = locked;

        if (locked && interactionPrompt != null)
        {
            interactionPrompt.gameObject.SetActive(false);
        }
    }
}
