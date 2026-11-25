
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    [Header("Speaker")]
    [SerializeField] private string speakerName;

    [Header("Content")]
    [SerializeField, TextArea(2, 5)]
    private string dialogueText;

    [Header("Reveal Settings")]
    [SerializeField]
    private DialogueRevealMode revealMode =
        DialogueRevealMode.CharacterByCharacter;

    [SerializeField, Min(1f)]
    private float charactersPerSecond = 35f;

    [SerializeField, Min(1f)]
    private float wordsPerSecond = 8f;

    [Header("Timing")]
    [SerializeField, Min(0f)]
    private float duration = 3.5f;

    [SerializeField, Min(0f)]
    private float pauseAfter = 0f;

    [Header("Audio (Future Use)")]
    [SerializeField] private AudioClip voiceClip;

    public string SpeakerName => speakerName;
    public string DialogueText => dialogueText;

    public DialogueRevealMode RevealMode => revealMode;

    public float CharactersPerSecond => charactersPerSecond;
    public float WordsPerSecond => wordsPerSecond;

    public float Duration => duration;
    public float PauseAfter => pauseAfter;

    public AudioClip VoiceClip => voiceClip;
}
