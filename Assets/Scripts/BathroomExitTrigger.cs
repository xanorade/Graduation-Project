
using System.Collections;
using UnityEngine;

public class BathroomExitTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ToiletInteractable toiletInteractable;
    [SerializeField] private PlayerInteractor playerInteractor;
    [SerializeField] private GameObject bossObject;

    [Header("Inner Monologue")]
    [SerializeField, TextArea(2, 4)]
    private string exitThought =
        "Something is wrong... I should go back to my bed.";

    [SerializeField] private float thoughtDuration = 4f;
    [SerializeField] private float bossActivationDelay = 0.25f;

    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerUI playerUI;

    [Header("Debug")]
    [SerializeField] private bool hasTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered)
            return;

        if (toiletInteractable == null ||
            !toiletInteractable.HasBeenUsed)
            return;

        PlayerController player =
            other.GetComponentInParent<PlayerController>();

        if (player == null)
            return;

        if (playerInteractor == null)
        {
            playerInteractor =
                player.GetComponent<PlayerInteractor>();
        }

        if (playerInteractor == null)
        {
            Debug.LogError(
                "PlayerInteractor bulunamadi!",
                this
            );
            return;
        }

        hasTriggered = true;

        playerInteractor.ShowInnerMonologue(
            exitThought,
            thoughtDuration
        );

        StartCoroutine(ActivateBossSequence());
    }

    private IEnumerator ActivateBossSequence()
    {
        yield return new WaitForSeconds(
            thoughtDuration + bossActivationDelay
        );

        if (playerController != null)
        {
            playerController.UnlockSprint();
        }

        if (playerUI != null)
        {
            playerUI.ShowStaminaUI();
        }

        if (bossObject != null)
        {
            bossObject.SetActive(true);

            Debug.Log("Boss activated! Chase started.");
        }
        else
        {
            Debug.LogError("Boss Object null", this);
        }
    }
}
