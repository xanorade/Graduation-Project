
using System;
using System.Collections;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerInteractor playerInteractor;

    [Header("Debug")]
    [SerializeField] private bool isPlaying;
    [SerializeField] private DialogueSequence currentSequence;

    private Coroutine playbackCoroutine;

    private Transform currentSource;
    private Action currentOnComplete;

    private bool previousPlayerState;
    private bool previousInteractionLock;
    private bool previousInteractorEnabled;

    public bool IsPlaying => isPlaying;

    private void Awake()
    {
        if (dialogueUI == null)
            dialogueUI = GetComponent<DialogueUI>();

        if (playerController == null)
            Debug.LogError("PlayerController atanmamis!", this);

        if (playerInteractor == null)
            Debug.LogError("PlayerInteractor atanmamis!", this);

        if (dialogueUI == null)
            Debug.LogError("DialogueUI atanmamis!", this);
    }

    private void Update()
    {
        if (!isPlaying || currentSequence == null)
            return;

        if (!currentSequence.CancelWhenOutOfRange)
            return;

        if (currentSource == null || playerController == null)
            return;

        float distance = Vector3.Distance(
            playerController.transform.position,
            currentSource.position
        );

        if (distance > currentSequence.CancelDistance)
        {
            CancelCurrentDialogue();
        }
    }

    public bool PlayDialogue(
        DialogueSequence sequence,
        Transform source = null,
        Action onComplete = null)
    {
        if (sequence == null)
        {
            Debug.LogError("DialogueSequence is null!", this);
            return false;
        }

        if (sequence.Lines == null || sequence.Lines.Count == 0)
        {
            Debug.LogWarning(
                "DialogueSequence has no lines!",
                sequence
            );
            return false;
        }

        if (isPlaying)
        {
            if (sequence.Priority <= currentSequence.Priority)
            {
                return false;
            }

            CancelCurrentDialogue();
        }

        currentSequence = sequence;
        currentSource = source;
        currentOnComplete = onComplete;

        SavePlayerState(sequence);
        ApplyDialogueSettings(sequence);

        isPlaying = true;

        Debug.Log(
    "DialogueManager Started: " +
    sequence.name +
    " | Type: " + sequence.Type +
    " | Lines: " + sequence.Lines.Count
);

        playbackCoroutine = StartCoroutine(
            PlaySequence(sequence)
        );

        return true;
    }

    private IEnumerator PlaySequence(DialogueSequence sequence)
    {
        for (int i = 0; i < sequence.Lines.Count; i++)
        {
            DialogueLine line = sequence.Lines[i];

            yield return dialogueUI.PlayLine(
                sequence.Type,
                line,
                sequence.AdvanceWithInput
            );

            if (i < sequence.Lines.Count - 1)
            {
                if (line.PauseAfter > 0f)
                {
                    yield return new WaitForSecondsRealtime(
                        line.PauseAfter
                    );
                }
            }
        }

        FinishDialogue(true);
    }

    private void SavePlayerState(DialogueSequence sequence)
    {
        if (sequence.FreezePlayer && playerController != null)
        {
            previousPlayerState = playerController.enabled;
        }

        if (playerInteractor != null)
        {
            previousInteractionLock =
                playerInteractor.IsInteractionLocked;

            previousInteractorEnabled =
                playerInteractor.enabled;
        }
    }

    private void ApplyDialogueSettings(DialogueSequence sequence)
    {
        if (playerInteractor != null)
        {
            playerInteractor.SetInteractionLocked(true);

            playerInteractor.enabled = false;
        }

        if (sequence.FreezePlayer && playerController != null)
        {
            playerController.enabled = false;
        }
    }

    public void CancelCurrentDialogue()
    {
        if (!isPlaying)
            return;

        if (playbackCoroutine != null)
        {
            StopCoroutine(playbackCoroutine);
        }

        FinishDialogue(false);
    }

    private void FinishDialogue(bool invokeCallback)
    {
        Action callback = currentOnComplete;

        if (dialogueUI != null)
        {
            dialogueUI.HideAll();
        }

        if (currentSequence != null &&
        currentSequence.FreezePlayer &&
        playerController != null)
        {
            playerController.enabled = previousPlayerState;
        }

        if (playerInteractor != null)
        {
            playerInteractor.SetInteractionLocked(
                previousInteractionLock
            );

            playerInteractor.enabled =
                previousInteractorEnabled;
        }

        currentSequence = null;
        currentSource = null;
        currentOnComplete = null;

        playbackCoroutine = null;
        isPlaying = false;

        if (invokeCallback)
        {
            callback?.Invoke();
        }
    }
}