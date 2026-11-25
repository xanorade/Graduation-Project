
using System.Collections;
using UnityEngine;

public class WakeUpIntro : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Transform cameraPivot;

    [Header("Wake Up Settings")]
    [SerializeField] private float lyingPitch = -75f;

    [SerializeField]
    private Vector3 lyingPositionOffset =
        new Vector3(0f, -0.85f, 0f);

    [SerializeField] private float initialPause = 0.8f;
    [SerializeField] private float wakeDuration = 2.5f;

    [SerializeField]
    private AnimationCurve wakeCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Dialogue")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueSequence wakeUpSequence;

    private Vector3 standingPosition;
    private Vector3 lyingPosition;

    private Quaternion standingRotation;
    private Quaternion lyingRotation;


    private void Start()
    {
        if (cameraPivot == null)
        {
            Debug.LogError("CameraPivot atanmamis!", this);
            enabled = false;
            return;
        }

        if (dialogueManager == null || wakeUpSequence == null)
        {
            Debug.LogError(
                "DialogueManager veya WakeUpSequence atanmamis!",
                this
            );
            enabled = false;
            return;
        }

        standingPosition = cameraPivot.localPosition;
        standingRotation = cameraPivot.localRotation;

        lyingPosition = standingPosition + lyingPositionOffset;

        lyingRotation =
            standingRotation *
            Quaternion.Euler(lyingPitch, 0f, 0f);

        StartCoroutine(WakeUpSequence());
    }

    private IEnumerator WakeUpSequence()
    {
        cameraPivot.localPosition = lyingPosition;
        cameraPivot.localRotation = lyingRotation;

        bool dialogueStarted =
            dialogueManager.PlayDialogue(wakeUpSequence);

        Debug.Log(
    "WakeUp Dialogue Started: " + dialogueStarted
);

        Debug.Log(
            "WakeUp Sequence: " + wakeUpSequence.name
        );

        if (!dialogueStarted)
        {
            Debug.LogError(
                "Wake Up dialogue baslatilamadi!",
                this
            );
        }

        yield return new WaitForSeconds(initialPause);

        float elapsed = 0f;

        while (elapsed < wakeDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsed / wakeDuration
            );

            float smoothProgress = wakeCurve.Evaluate(progress);

            cameraPivot.localPosition = Vector3.Lerp(
                lyingPosition,
                standingPosition,
                smoothProgress
            );

            cameraPivot.localRotation = Quaternion.Slerp(
                lyingRotation,
                standingRotation,
                smoothProgress
            );

            yield return null;
        }

        cameraPivot.localPosition = standingPosition;
        cameraPivot.localRotation = standingRotation;
    }
}
