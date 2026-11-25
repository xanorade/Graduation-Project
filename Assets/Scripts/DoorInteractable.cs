
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class DoorInteractable : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private bool canOpen = false;
    [SerializeField] private string inspectPrompt = "E - Inspect";

    [Header("Dialogue")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueSequence inspectionSequence;
    [SerializeField] private DialogueSequence firstOpenSequence;

    [Header("Door Pivot")]
    [SerializeField] private Transform doorPivot;

    [SerializeField] private bool startsAjar = false;

    [SerializeField] private float closedAngle = 0f;
    [SerializeField] private float ajarAngle = 8f;
    [SerializeField] private float openAngle = 95f;

    [SerializeField] private float rotationDuration = 0.5f;

    [Header("Story Conditions")]
    [SerializeField] private ToiletInteractable toiletInteractable;

    [Header("Events")]
    [SerializeField] private UnityEvent onFirstOpened;

    private Quaternion baseLocalRotation;

    private bool isOpen;
    private bool isAnimating;

    private bool hasShownOpenMessage = false;
    private bool hasTriggeredFirstOpenEvent = false;

    private Collider[] doorColliders;

    private void Awake()
    {
        if (doorPivot == null)
            doorPivot = transform;

        baseLocalRotation = doorPivot.localRotation;

        doorColliders =
            GetComponentsInChildren<Collider>(true);

        float initialAngle = startsAjar
            ? ajarAngle
            : closedAngle;

        doorPivot.localRotation =
            baseLocalRotation *
            Quaternion.Euler(0f, initialAngle, 0f);
    }

    public string GetPrompt()
    {
        if (!canOpen)
            return inspectPrompt;

        return isOpen ? "E - Close" : "E - Open";
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (isAnimating)
            return;

        if (!canOpen)
        {
            if (dialogueManager != null &&
                inspectionSequence != null)
            {
                dialogueManager.PlayDialogue(
                    inspectionSequence,
                    transform
                );
            }

            return;
        }

        isOpen = !isOpen;

        if (isOpen)
        {
            if (!hasTriggeredFirstOpenEvent &&
                toiletInteractable != null &&
                toiletInteractable.HasBeenUsed)
            {
                hasTriggeredFirstOpenEvent = true;
                onFirstOpened?.Invoke();
            }

            if (!hasShownOpenMessage &&
                firstOpenSequence != null &&
                dialogueManager != null)
            {
                bool started = dialogueManager.PlayDialogue(
                    firstOpenSequence
                );

                if (started)
                    hasShownOpenMessage = true;
            }
        }

        float targetAngle = isOpen
            ? openAngle
            : closedAngle;

        StartCoroutine(RotateDoor(targetAngle));
    }

    private IEnumerator RotateDoor(float targetAngle)
    {
        isAnimating = true;

        Quaternion startRotation =
            doorPivot.localRotation;

        Quaternion targetRotation =
            baseLocalRotation *
            Quaternion.Euler(0f, targetAngle, 0f);

        float elapsed = 0f;

        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;

            float t = rotationDuration > 0f
                ? Mathf.Clamp01(elapsed / rotationDuration)
                : 1f;

            doorPivot.localRotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                t
            );

            yield return null;
        }

        doorPivot.localRotation = targetRotation;

        isAnimating = false;
    }

    public bool IsWithinInteractionRange(
        Vector3 playerPosition,
        float maxDistance)
    {
        if (doorColliders == null ||
            doorColliders.Length == 0)
        {
            return Vector3.Distance(
                playerPosition,
                transform.position
            ) <= maxDistance;
        }

        foreach (Collider col in doorColliders)
        {
            if (col == null || !col.enabled)
                continue;

            Vector3 closestPoint =
                col.ClosestPoint(playerPosition);

            if (Vector3.Distance(
                playerPosition,
                closestPoint
            ) <= maxDistance)
            {
                return true;
            }
        }

        return false;
    }
}