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

    void Start()
    {
        UpdateSpeedText(speedSlider.value);
        speedSlider.onValueChanged.AddListener(UpdateSpeedText);
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

        BallisticResult angles = launcher.FireFromUI(launcherPos, targetPos, speed);

        if (angles.success)
        {
            if (yawText != null)
                yawText.text = $"{angles.yaw:F1}°";

            if (pitchText != null)
                pitchText.text = $"{angles.pitch:F1}°";
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
}