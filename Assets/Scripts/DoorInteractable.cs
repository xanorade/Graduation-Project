
using System.Collections;
using UnityEngine;

public class DoorInteractable : MonoBehaviour
{
    private Collider[] doorColliders;

    [Header("Interaction")]
    [SerializeField] private bool canOpen = false;

    [SerializeField] private string inspectPrompt = "E - Open";

    [SerializeField, TextArea(2, 4)]
    private string roomMessage =
        "nothing special here.";

    [SerializeField, TextArea(2, 4)]
    private string openMessage = "";

    [Header("Door Pivot")]
    [SerializeField] private Transform doorPivot;

    [SerializeField] private bool startsAjar = false;

    [SerializeField] private float closedAngle = 0f;
    [SerializeField] private float ajarAngle = 8f;
    [SerializeField] private float openAngle = 95f;

    [SerializeField] private float rotationDuration = 0.5f;

    private Quaternion baseLocalRotation;
    private bool isOpen;
    private bool isAnimating;

    private void Awake()
    {
        if (doorPivot == null)
            doorPivot = transform;

        baseLocalRotation = doorPivot.localRotation;

        float initialAngle = startsAjar
            ? ajarAngle
            : closedAngle;

        doorPivot.localRotation =
            baseLocalRotation *
            Quaternion.Euler(0f, initialAngle, 0f);

        doorColliders = GetComponentsInChildren<Collider>(true);
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
            interactor.ShowDoorInspection(
                roomMessage,
                this
            );

            return;
        }

        isOpen = !isOpen;

        if (isOpen)
        {
            if (!string.IsNullOrWhiteSpace(openMessage))
            {
                interactor.ShowInnerMonologue(
                    openMessage,
                    2.5f
                );
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

        Quaternion startRotation = doorPivot.localRotation;

        Quaternion targetRotation =
            baseLocalRotation *
            Quaternion.Euler(0f, targetAngle, 0f);

        float elapsed = 0f;

        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / rotationDuration
            );

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
        if (doorColliders == null || doorColliders.Length == 0)
        {
            return Vector3.Distance(
                playerPosition,
                transform.position
            ) <= maxDistance;
        }

        float closestDistance = float.MaxValue;

        foreach (Collider col in doorColliders)
        {
            if (col == null || !col.enabled)
                continue;

            Vector3 closestPoint = col.ClosestPoint(playerPosition);

            float distance = Vector3.Distance(
                playerPosition,
                closestPoint
            );

            if (distance < closestDistance)
                closestDistance = distance;
        }

        return closestDistance <= maxDistance;
    }
}
