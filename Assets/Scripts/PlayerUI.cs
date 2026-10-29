using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Image staminaBarImage;

    private void Update()
    {
        if (playerController != null && staminaBarImage != null)
        {
            float staminaPercentage = playerController.currentRunDuration / playerController.RunDuration;
            staminaBarImage.fillAmount = staminaPercentage;

            if (playerController.IsExhausted)
            {
                float targetHue = Mathf.Lerp(0f, 50f, staminaPercentage);
                Color dynamicColor = Color.HSVToRGB(targetHue / 360f, 1f, 1f);
                staminaBarImage.color = dynamicColor;
            }
            else
            {
                staminaBarImage.color = Color.HSVToRGB(50f / 360f, 1f, 1f); // 50 = Sarı
            }
        }
    }
}