
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [Header("Inner Monologue")]
    [SerializeField] private GameObject innerMonologuePanel;
    [SerializeField] private TMP_Text innerMonologueText;

    [Header("Inspection")]
    [SerializeField] private GameObject inspectionPanel;
    [SerializeField] private TMP_Text inspectionText;

    [Header("Conversation")]
    [SerializeField] private GameObject conversationPanel;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text conversationText;

    private bool typingWasSkipped;

    private void Awake()
    {
        HideAll();
    }

    public IEnumerator PlayLine(
        DialogueType type,
        DialogueLine line,
        bool advanceWithInput)
    {
        if (line == null)
            yield break;

        HideAll();

        TMP_Text targetText = null;

        switch (type)
        {
            case DialogueType.InnerMonologue:

                innerMonologuePanel.SetActive(true);
                targetText = innerMonologueText;
                break;

            case DialogueType.Inspection:

                inspectionPanel.SetActive(true);
                targetText = inspectionText;
                break;

            case DialogueType.Conversation:

                conversationPanel.SetActive(true);
                targetText = conversationText;

                if (speakerNameText != null)
                    speakerNameText.text = line.SpeakerName;

                break;
        }

        if (targetText == null)
        {
            Debug.LogError("Dialogue UI text reference is missing!");
            HideAll();
            yield break;
        }

        targetText.gameObject.SetActive(true);

        if (type == DialogueType.Conversation &&
            speakerNameText != null)
        {
            speakerNameText.gameObject.SetActive(true);
        }

        targetText.text = line.DialogueText;
        targetText.maxVisibleCharacters = 0;

        targetText.ForceMeshUpdate();

        yield return null;

        typingWasSkipped = false;

        switch (line.RevealMode)
        {
            case DialogueRevealMode.Instant:

                ShowAllCharacters(targetText);
                break;

            case DialogueRevealMode.CharacterByCharacter:

                yield return RevealCharacters(
                    targetText,
                    line.CharactersPerSecond
                );
                break;

            case DialogueRevealMode.WordByWord:

                yield return RevealWords(
                    targetText,
                    line.WordsPerSecond
                );
                break;
        }

        if (advanceWithInput)
        {
            if (typingWasSkipped)
                yield return WaitForKeyRelease();

            yield return WaitForAdvanceInput();
        }
        else
        {
            yield return new WaitForSecondsRealtime(
                line.Duration
            );
        }

        HideAll();
    }

    private IEnumerator RevealCharacters(
        TMP_Text targetText,
        float charactersPerSecond)
    {
        int totalCharacters =
            targetText.textInfo.characterCount;

        float delay =
            1f / Mathf.Max(1f, charactersPerSecond);

        targetText.maxVisibleCharacters = 0;

        for (int i = 1; i <= totalCharacters; i++)
        {
            if (WasAdvancePressed())
            {
                ShowAllCharacters(targetText);
                typingWasSkipped = true;
                yield break;
            }

            targetText.maxVisibleCharacters = i;

            yield return new WaitForSecondsRealtime(delay);
        }

        ShowAllCharacters(targetText);
    }

    private IEnumerator RevealWords(
        TMP_Text targetText,
        float wordsPerSecond)
    {
        targetText.ForceMeshUpdate();

        int wordCount = targetText.textInfo.wordCount;
        int totalCharacters = targetText.textInfo.characterCount;

        float delay =
            1f / Mathf.Max(1f, wordsPerSecond);

        targetText.maxVisibleCharacters = 0;

        if (wordCount == 0)
        {
            ShowAllCharacters(targetText);
            yield break;
        }

        for (int i = 0; i < wordCount; i++)
        {
            if (WasAdvancePressed())
            {
                ShowAllCharacters(targetText);
                typingWasSkipped = true;
                yield break;
            }

            int lastCharacterIndex =
                targetText.textInfo.wordInfo[i].lastCharacterIndex;

            targetText.maxVisibleCharacters =
                lastCharacterIndex + 1;

            yield return new WaitForSecondsRealtime(delay);
        }

        ShowAllCharacters(targetText);
    }

    private void ShowAllCharacters(TMP_Text targetText)
    {
        targetText.maxVisibleCharacters =
            targetText.textInfo.characterCount;
    }

    private bool WasAdvancePressed()
    {
        if (Keyboard.current == null)
            return false;

        return Keyboard.current.eKey.wasPressedThisFrame ||
               Keyboard.current.spaceKey.wasPressedThisFrame;
    }

    private IEnumerator WaitForAdvanceInput()
    {
        while (!WasAdvancePressed())
        {
            yield return null;
        }

        yield return WaitForKeyRelease();
    }

    private IEnumerator WaitForKeyRelease()
    {
        while (Keyboard.current != null &&
              (Keyboard.current.eKey.isPressed ||
               Keyboard.current.spaceKey.isPressed))
        {
            yield return null;
        }
    }

    public void HideAll()
    {
        if (innerMonologuePanel != null)
            innerMonologuePanel.SetActive(false);

        if (inspectionPanel != null)
            inspectionPanel.SetActive(false);

        if (conversationPanel != null)
            conversationPanel.SetActive(false);
    }
}