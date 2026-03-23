using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the user interface for the ballistic launcher simulator.
/// Handles input fields, sliders, presets, launch button (Space key), 
/// and displays calculated yaw/pitch angles or error messages.
/// </summary>
public class UIController : MonoBehaviour
{
    [Header("Launcher Position Inputs")]
    public TMP_InputField launcherX;
    public TMP_InputField launcherY;
    public TMP_InputField launcherZ;

    [Header("Target Position Inputs")]
    public TMP_InputField targetX;
    public TMP_InputField targetY;
    public TMP_InputField targetZ;

    [Header("Launch Speed")]
    [Tooltip("Slider to set initial projectile speed")]
    public Slider speedSlider;

    [Tooltip("Text display showing current speed value")]
    public TMP_Text speedValueText;

    [Header("Angle Display")]
    public TMP_Text yawText;
    public TMP_Text pitchText;

    [Header("Launcher Reference")]
    [Tooltip("Reference to the Launcher component that performs the shot")]
    public Launcher launcher;

    [Header("Air Resistance & Projectile Settings")]
    [Tooltip("Toggle to enable/disable air drag in calculations and simulation")]
    public Toggle airResistanceToggle;

    public TMP_InputField cdInput;       // Drag coefficient
    public TMP_InputField massInput;     // Projectile mass (kg)
    public TMP_InputField areaInput;     // Cross-sectional area (m²)

    [Header("Preset Buttons")]
    public Button presetGrenade;
    public Button presetMine82;
    public Button presetMine120;
    public Button presetSphere;

    void Start()
    {
        // Initialize speed display
        UpdateSpeedText(speedSlider.value);

        // Subscribe to slider changes
        speedSlider.onValueChanged.AddListener(UpdateSpeedText);

        // Assign preset button actions
        presetGrenade.onClick.AddListener(() => ApplyPreset(0.8f, 1.5f, 0.008f));
        presetMine82.onClick.AddListener(() => ApplyPreset(0.9f, 3.5f, 0.015f));
        presetMine120.onClick.AddListener(() => ApplyPreset(0.85f, 16f, 0.028f));
        presetSphere.onClick.AddListener(() => ApplyPreset(0.47f, 2f, 0.012f));
    }

    void Update()
    {
        // Press Space to launch
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Launch();
        }
    }

    /// <summary>
    /// Reads all input fields, parses values, triggers ballistic calculation 
    /// and updates UI with results (angles or error message).
    /// </summary>
    public void Launch()
    {
        // Parse launcher coordinates
        float lx = ParseFloat(launcherX.text);
        float ly = ParseFloat(launcherY.text);
        float lz = ParseFloat(launcherZ.text);

        // Parse target coordinates
        float tx = ParseFloat(targetX.text);
        float ty = ParseFloat(targetY.text);
        float tz = ParseFloat(targetZ.text);

        float speed = speedSlider.value;
        bool useDrag = airResistanceToggle.isOn;

        // Parse projectile parameters
        float cd = ParseFloat(cdInput.text);
        float massVal = ParseFloat(massInput.text);
        float area = ParseFloat(areaInput.text);

        Vector3 launcherPos = new Vector3(lx, ly, lz);
        Vector3 targetPos = new Vector3(tx, ty, tz);

        // Call the launcher to perform calculation and fire
        BallisticResult result = launcher.FireFromUI(
            launcherPos, targetPos, speed,
            useDrag, cd, area, 1.225f, massVal);

        // Update UI with results
        if (result.success)
        {
            if (yawText != null)
                yawText.text = $"{result.yaw:F1}°";

            if (pitchText != null)
                pitchText.text = $"{result.pitch:F1}°";
        }
        else
        {
            if (yawText != null)
                yawText.text = "—";

            if (pitchText != null)
                pitchText.text = "out of reach";
        }
    }

    /// <summary>
    /// Safely parses string to float, handling empty values, commas and dots.
    /// Uses invariant culture to ensure consistent decimal separator (.).
    /// </summary>
    /// <param name="value">Input string from TMP_InputField</param>
    /// <returns>Parsed float value or 0 if invalid/empty</returns>
    private float ParseFloat(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0f;

        // Replace comma with dot (common in different locales)
        value = value.Replace(",", ".");

        // Parse using invariant culture (always uses . as decimal separator)
        return float.Parse(value, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Updates the speed display text when slider value changes.
    /// </summary>
    /// <param name="value">Current slider value</param>
    void UpdateSpeedText(float value)
    {
        speedValueText.text = value.ToString("F1") + " m/s";
    }

    /// <summary>
    /// Applies preset values to the air drag / projectile input fields.
    /// Called from preset buttons.
    /// </summary>
    /// <param name="cd">Preset drag coefficient</param>
    /// <param name="massVal">Preset mass (kg)</param>
    /// <param name="area">Preset cross-sectional area (m²)</param>
    private void ApplyPreset(float cd, float massVal, float area)
    {
        cdInput.text = cd.ToString("F2");
        massInput.text = massVal.ToString("F1");
        areaInput.text = area.ToString("F3");
    }
}