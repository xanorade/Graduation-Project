
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MomTouchDeath : MonoBehaviour
{
    [Header("Death Screen")]
    [SerializeField] private CanvasGroup deathScreen;

    [Header("Death Settings")]
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private float blackScreenDuration = 2f;

    [Header("Debug")]
    [SerializeField] private bool isPlayerDead = false;

    private void Awake()
    {
        if (deathScreen != null)
        {
            deathScreen.alpha = 0f;
            deathScreen.interactable = false;
            deathScreen.blocksRaycasts = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isPlayerDead)
            return;

        PlayerController player =
            other.GetComponentInParent<PlayerController>();

        if (player == null)
            return;

        isPlayerDead = true;

        StartCoroutine(DeathSequence(player));
    }

    private IEnumerator DeathSequence(PlayerController player)
    {
        if (player != null)
            player.enabled = false;

        PlayerInteractor interactor =
            player.GetComponent<PlayerInteractor>();

        if (interactor != null)
            interactor.SetInteractionLocked(true);

        PlayerHiding hiding =
            player.GetComponent<PlayerHiding>();

        if (hiding != null)
            hiding.enabled = false;

        if (deathScreen == null)
        {
            Debug.LogError(
                "DeathScreen CanvasGroup atanmamis!",
                this
            );

            yield break;
        }

        deathScreen.gameObject.SetActive(true);
        deathScreen.alpha = 0f;
        deathScreen.blocksRaycasts = true;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = fadeDuration > 0f
                ? Mathf.Clamp01(elapsed / fadeDuration)
                : 1f;

            deathScreen.alpha = progress;

            yield return null;
        }

        deathScreen.alpha = 1f;

        yield return new WaitForSecondsRealtime(
            blackScreenDuration
        );

        string currentScene =
            SceneManager.GetActiveScene().name;

        SceneManager.LoadScene(currentScene);
    }
}
