
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "DS_NewDialogue",
    menuName = "Dialogue/Dialogue Sequence"
)]
public class DialogueSequence : ScriptableObject
{
    [Header("Dialogue Presentation")]
    [SerializeField] private DialogueType dialogueType;

    [SerializeField]
    private List<DialogueLine> lines =
        new List<DialogueLine>();

    [Header("Playback Settings")]
    [SerializeField] private bool freezePlayer = false;
    [SerializeField] private bool advanceWithInput = false;

    [Header("Cancellation")]
    [SerializeField] private bool cancelWhenOutOfRange = false;
    [SerializeField] private float cancelDistance = 2.5f;

    [Header("Priority")]
    [SerializeField, Range(0, 100)]
    private int priority = 0;

    public DialogueType Type => dialogueType;
    public IReadOnlyList<DialogueLine> Lines => lines;

    public bool FreezePlayer => freezePlayer;
    public bool AdvanceWithInput => advanceWithInput;

    public bool CancelWhenOutOfRange => cancelWhenOutOfRange;
    public float CancelDistance => cancelDistance;

    public int Priority => priority;
}