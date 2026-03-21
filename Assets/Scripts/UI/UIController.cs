using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Launcher inputs")]
    public TMP_InputField launcherX;
    public TMP_InputField launcherY;
    public TMP_InputField launcherZ;

    [Header("Target inputs")]
    public TMP_InputField targetX;
    public TMP_InputField targetY;
    public TMP_InputField targetZ;

    [Header("Speed")]
    public Slider speedSlider;
    public TMP_Text speedValueText;

    [Header("Angle labels")]
    public TMP_Text yawText;
    public TMP_Text pitchText;

    [Header("Launcher reference")]
    public Launcher launcher;

    [Header("Air Resistance & Projectile")]
    public Toggle airResistanceToggle;
    public TMP_InputField cdInput;
    public TMP_InputField massInput;
    public TMP_InputField areaInput;

    [Header("Presets")]
    public Button presetGrenade;
    public Button presetMine82;
    public Button presetMine120;
    public Button presetSphere;

    void Start()
    {
        UpdateSpeedText(speedSlider.value);
        speedSlider.onValueChanged.AddListener(UpdateSpeedText);

        presetGrenade.onClick.AddListener(() => ApplyPreset(0.8f, 1.5f, 0.008f));
        presetMine82.onClick.AddListener(() => ApplyPreset(0.9f, 3.5f, 0.015f));
        presetMine120.onClick.AddListener(() => ApplyPreset(0.85f, 16f, 0.028f));
        presetSphere.onClick.AddListener(() => ApplyPreset(0.47f, 2f, 0.012f));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Launch();
        }
    }

    public void Launch()
    {
        float lx = ParseFloat(launcherX.text);
        float ly = ParseFloat(launcherY.text);
        float lz = ParseFloat(launcherZ.text);

        float tx = ParseFloat(targetX.text);
        float ty = ParseFloat(targetY.text);
        float tz = ParseFloat(targetZ.text);

        float speed = speedSlider.value;

        Vector3 launcherPos = new Vector3(lx, ly, lz);
        Vector3 targetPos = new Vector3(tx, ty, tz);

        BallisticResult result = launcher.FireFromUI(launcherPos, targetPos, speed);

        if (result.success)
        {
            if (yawText != null)
                yawText.text = $"{result.yaw:F1}°";

            if (pitchText != null)
                pitchText.text = $"{result.pitch:F1}°";

            launcher.ApplySettingsToLastProjectile(
                airResistanceToggle.isOn,
                ParseFloat(cdInput.text),
                ParseFloat(massInput.text),
                ParseFloat(areaInput.text)
            );
        }
        else
        {
            if (yawText != null)
                yawText.text = "—";

            if (pitchText != null)
                pitchText.text = "out of reach";
        }
    }

    private float ParseFloat(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0f;

        value = value.Replace(",", ".");
        return float.Parse(value, CultureInfo.InvariantCulture);
    }

    void UpdateSpeedText(float value)
    {
        speedValueText.text = value.ToString("F1") + " m/s";
    }

    private void ApplyPreset(float cd, float massVal, float area)
    {
        cdInput.text = cd.ToString("F2");
        massInput.text = massVal.ToString("F1");
        areaInput.text = area.ToString("F3");
    }
}