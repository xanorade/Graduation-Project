
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ToiletInteractable : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerInteractor playerInteractor;
    [SerializeField] private CanvasGroup blackoutCanvasGroup;

    [Header("Toilet Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float blackoutDuration = 5f;

    [Header("Inner Monologue")]
    [SerializeField, TextArea(2, 4)]
    private string afterUseThought =
        "I should get back to my room without making a sound.";

    [SerializeField] private float thoughtDuration = 4f;

    [Header("Events")]
    [SerializeField] private UnityEvent onToiletUsed;

    private bool isUsing;
    private bool hasBeenUsed;
    private bool previousControllerState;

    public bool CanInteract => !isUsing && !hasBeenUsed;
    public bool HasBeenUsed => hasBeenUsed;

    private void Awake()
    {
        if (blackoutCanvasGroup != null)
        {
            blackoutCanvasGroup.alpha = 0f;
            blackoutCanvasGroup.interactable = false;
            blackoutCanvasGroup.blocksRaycasts = false;
        }
    }

    public string GetPrompt()
    {
        return "E - Use";
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (!CanInteract)
            return;

        if (playerInteractor == null)
            playerInteractor = interactor;

        if (playerController == null ||
            playerInteractor == null ||
            blackoutCanvasGroup == null)
        {
            Debug.LogError(
                "ToiletInteractable references are missing!",
                this
            );
            return;
        }

        StartCoroutine(UseToilet());
    }

    private IEnumerator UseToilet()
    {
        isUsing = true;

        // Oyuncunun hareketini kaydet ve kapat.
        previousControllerState = playerController.enabled;
        playerController.enabled = false;

        // Diğer etkileşimleri geçici olarak engelle.
        playerInteractor.SetInteractionLocked(true);

        // Siyah ekranı aç.
        blackoutCanvasGroup.gameObject.SetActive(true);
        blackoutCanvasGroup.blocksRaycasts = true;

        yield return FadeScreen(0f, 1f);

        // Tuvaleti kullanma süresi.
        yield return new WaitForSecondsRealtime(
            blackoutDuration
        );

        hasBeenUsed = true;

        // İleride hikâye sistemine bağlayacağız.
        onToiletUsed?.Invoke();

        // Ekranı tekrar aç.
        yield return FadeScreen(1f, 0f);

        blackoutCanvasGroup.blocksRaycasts = false;

        // Oyuncu kontrolünü geri ver.
        playerController.enabled = previousControllerState;

        playerInteractor.SetInteractionLocked(false);

        // Karakterin iç sesi.
        playerInteractor.ShowInnerMonologue(
            afterUseThought,
            thoughtDuration
        );

        isUsing = false;
    }

    private IEnumerator FadeScreen(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;

        blackoutCanvasGroup.alpha = startAlpha;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = fadeDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / fadeDuration);

            blackoutCanvasGroup.alpha = Mathf.Lerp(
                startAlpha,
                endAlpha,
                t
            );

            yield return null;
        }

        blackoutCanvasGroup.alpha = endAlpha;
    }
}
