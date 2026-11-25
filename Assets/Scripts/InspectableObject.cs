
using UnityEngine;

public class InspectableObject : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private string interactionPrompt = "E - Inspect";

    [Header("Dialogue")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueSequence inspectionSequence;

    private Collider[] objectColliders;

    private void Awake()
    {
        objectColliders =
            GetComponentsInChildren<Collider>(true);
    }

    public string GetPrompt()
    {
        return interactionPrompt;
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (dialogueManager == null)
        {
            Debug.LogError("DialogueManager is missing!", this);
            return;
        }

        if (inspectionSequence == null)
        {
            Debug.LogWarning(
                "Inspection Sequence is not assigned!",
                this
            );
            return;
        }

        dialogueManager.PlayDialogue(
            inspectionSequence,
            transform
        );
    }

    public bool IsWithinInteractionRange(
        Vector3 playerPosition,
        float maxDistance)
    {
        if (objectColliders == null ||
            objectColliders.Length == 0)
        {
            return Vector3.Distance(
                playerPosition,
                transform.position
            ) <= maxDistance;
        }

        foreach (Collider col in objectColliders)
        {
            if (col == null || !col.enabled)
                continue;

            Vector3 closestPoint =
                col.ClosestPoint(playerPosition);

            float distance = Vector3.Distance(
                playerPosition,
                closestPoint
            );

            if (distance <= maxDistance)
                return true;
        }

        return false;
    }
}
