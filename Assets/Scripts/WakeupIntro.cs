
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

    [Header("Disable During Intro")]
    [SerializeField] private MonoBehaviour[] controlsToDisable;

    [Header("Inner Monologue")]
    [SerializeField] private PlayerInteractor playerInteractor;

    [SerializeField, TextArea(2, 4)]
    private string wakingThought =
        "ugh... i really need to use the bathroom.";

    [SerializeField, TextArea(2, 4)]
    private string gettingUpThought =
        "hopefully mom still asleep. i don't want to wake her up.";

    [SerializeField] private float thoughtDuration = 3.5f;

    private Vector3 standingPosition;
    private Vector3 lyingPosition;

    private Quaternion standingRotation;
    private Quaternion lyingRotation;

    private bool[] previousControlStates;

    private void Start()
    {
        standingPosition = cameraPivot.localPosition;
        standingRotation = cameraPivot.localRotation;

        lyingPosition = standingPosition + lyingPositionOffset;

        lyingRotation =
            standingRotation * Quaternion.Euler(lyingPitch, 0f, 0f);

        StartCoroutine(WakeUpSequence());
    }


    private IEnumerator WakeUpSequence()
    {
        DisableControls();

        cameraPivot.localPosition = lyingPosition;
        cameraPivot.localRotation = lyingRotation;

        if (playerInteractor != null)
        {
            playerInteractor.ShowInnerMonologue(
                wakingThought,
                thoughtDuration
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

        RestoreControls();

        if (playerInteractor != null)
        {
            playerInteractor.ShowInnerMonologue(
                gettingUpThought,
                thoughtDuration
            );
        }
    }


    private void DisableControls()
    {
        previousControlStates =
            new bool[controlsToDisable.Length];

        for (int i = 0; i < controlsToDisable.Length; i++)
        {
            if (controlsToDisable[i] == null)
                continue;

            previousControlStates[i] =
                controlsToDisable[i].enabled;

            controlsToDisable[i].enabled = false;
        }
    }

    private void RestoreControls()
    {
        if (previousControlStates == null)
            return;

        for (int i = 0; i < controlsToDisable.Length; i++)
        {
            if (controlsToDisable[i] == null)
                continue;

            controlsToDisable[i].enabled =
                previousControlStates[i];
        }
    }
}
