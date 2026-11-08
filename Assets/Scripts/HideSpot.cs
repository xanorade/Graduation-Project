
using UnityEngine;

public class HideSpot : MonoBehaviour
{
    [Header("Hide Spot Positions")]
    [SerializeField] private Transform hidePoint;
    [SerializeField] private Transform cameraPoint;
    [SerializeField] private Transform exitPoint;

    [Header("Interaction")]
    [SerializeField] private string interactionPrompt = "E - Hide";

    public Transform HidePoint => hidePoint;
    public Transform CameraPoint => cameraPoint;
    public Transform ExitPoint => exitPoint;

    public string GetPrompt()
    {
        return interactionPrompt;
    }

    private void OnDrawGizmosSelected()
    {
        if (hidePoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(hidePoint.position, 0.12f);
        }

        if (cameraPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(cameraPoint.position, 0.12f);
            Gizmos.DrawRay(
                cameraPoint.position,
                cameraPoint.forward * 0.5f
            );
        }

        if (exitPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(exitPoint.position, 0.12f);
        }
    }
}
