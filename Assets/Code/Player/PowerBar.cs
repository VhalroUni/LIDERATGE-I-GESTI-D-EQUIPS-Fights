using UnityEngine;
using UnityEngine.UI;

public class PowerBar : MonoBehaviour
{
    public Slider powerBar;

    [Header("Power Levels Colors")]
    public Color level1Color = Color.white;
    public Color level2Color = Color.yellow;
    public Color level3Color = Color.red;
    public Color level4Color = Color.magenta;

    [Header("Bar Smooth Settings")]
    public float fillSpeedUp = 5f;
    public float fillSpeedDown = 9f;

    [Header("Bar Settings")]
    public float totalPower = 0f;
    private float displayedPower = 0f;
    private const float maxTotalPower = 4f;
    private Image powerFillImage;

    private bool rainbowActive = false;

    private void Start()
    {
        if (powerBar != null)
            powerFillImage = powerBar.fillRect.GetComponent<Image>();
    }

    private void Update()
    {
        if (powerBar != null)
        {
            float speed = (displayedPower < totalPower) ? fillSpeedUp : fillSpeedDown;
            displayedPower = Mathf.MoveTowards(displayedPower, totalPower, speed * Time.deltaTime);

            int visualLevel = Mathf.FloorToInt(displayedPower);
            float visualLocal = displayedPower - visualLevel;

            powerBar.value = visualLocal;
        }

        if (rainbowActive && powerFillImage != null)
        {
            float hue = Mathf.Repeat(Time.time * 0.5f, 1f);
            powerFillImage.color = Color.HSVToRGB(hue, 1f, 1f);
        }
    }

    public void ModifyPower(float amount)
    {
        totalPower += (amount / 100);
        totalPower = Mathf.Clamp(totalPower, 0f, maxTotalPower);
        UpdatePowerVisual();
    }

    void UpdatePowerVisual()
    {
        if (powerBar == null || powerFillImage == null) return;

        int level = Mathf.FloorToInt(totalPower);

        switch (level)
        {
            case 0: rainbowActive = false; powerFillImage.color = level1Color; break;
            case 1: rainbowActive = false; powerFillImage.color = level2Color; break;
            case 2: rainbowActive = false; powerFillImage.color = level3Color; break;
            default: rainbowActive = true; break;
        }
    }
}
