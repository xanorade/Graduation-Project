
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Image staminaBarImage;
    [SerializeField] private CanvasGroup staminaCanvasGroup;

    private void Awake()
    {
        HideStaminaUI();
    }

    private void Update()
    {
        if (playerController != null &&
            staminaBarImage != null)
        {
            float staminaPercentage =
                playerController.currentRunDuration /
                playerController.RunDuration;

            staminaBarImage.fillAmount = staminaPercentage;

            if (playerController.IsExhausted)
            {
                float targetHue =
                    Mathf.Lerp(0f, 50f, staminaPercentage);

                Color dynamicColor = Color.HSVToRGB(
                    targetHue / 360f,
                    1f,
                    1f
                );

                staminaBarImage.color = dynamicColor;
            }
            else
            {
                staminaBarImage.color = Color.HSVToRGB(
                    50f / 360f,
                    1f,
                    1f
                );
            }
        }
    }

    public void ShowStaminaUI()
    {
        if (staminaCanvasGroup == null)
            return;

        staminaCanvasGroup.alpha = 1f;
        staminaCanvasGroup.interactable = false;
        staminaCanvasGroup.blocksRaycasts = false;
    }

    public void HideStaminaUI()
    {
        if (staminaCanvasGroup == null)
            return;

        staminaCanvasGroup.alpha = 0f;
        staminaCanvasGroup.interactable = false;
        staminaCanvasGroup.blocksRaycasts = false;
    }
}
