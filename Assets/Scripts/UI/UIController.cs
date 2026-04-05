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
    [Header("Start Position Inputs")]
    public TMP_InputField startX;
    public TMP_InputField startY;
    public TMP_InputField startZ;

    [Header("Target Position Inputs")]
    public TMP_InputField targetX;
    public TMP_InputField targetY;
    public TMP_InputField targetZ;

    [Header("Launch Speed")]
    [Tooltip("Slider to set initial projectile speed")]
    public Slider speedSlider;

    [Tooltip("Text display showing current speed value")]
    public TMP_Text speedValueText;

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

    [Header("Results")]
    // Launcher mode only
    public GameObject anglesSection;
    public TMP_Text yawText;
    public TMP_Text pitchText;
    // Drone mode only
    public GameObject releaseSection;
    public TMP_Text releasePointText;
    public TMP_Text releaseTimeText;

    [Header("References")]
    [Tooltip("Reference to the Target visual component")]
    public Transform targetVisual;
    [Tooltip("Reference to the Launcher prefab")]
    public GameObject launcherPrefab;
    [Tooltip("Reference to the Drone prefab")]
    public GameObject dronePrefab;

    [Header("Mode Selection")]
    public TMP_Dropdown modeDropdown;
    public enum GameMode { Launcher, Drone }
    private GameMode currentMode = GameMode.Launcher;

    private GameObject currentObject;
    private Launcher currentLauncher;
    private Drone currentDrone;
    

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

        // Subscribe to mode dropdown changes
        modeDropdown.onValueChanged.AddListener(OnModeChanged);
        OnModeChanged(0); // initial mode
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
        Vector3 startPos = GetStartPosition();
        Vector3 targetPos = GetTargetPosition();
        float speed = speedSlider.value;
        AirResistanceSettings settings = new AirResistanceSettings
        {
            useAirDrag = airResistanceToggle.isOn,
            dragCoefficient = ParseFloat(cdInput.text),
            crossSectionArea = ParseFloat(areaInput.text),
            airDensity = 1.225f,
            mass = ParseFloat(massInput.text)
        };

        if (targetVisual != null) 
            targetVisual.position = targetPos;


        if (currentMode == GameMode.Launcher && currentLauncher != null)
        {
            currentLauncher.transform.position = startPos;
            BallisticResult result = currentLauncher.FireFromUI(startPos, targetPos, speed, settings);

            // Update UI with results
            if (result.success)
            {
                yawText.text = $"{result.yaw:F1}°";
                pitchText.text = $"{result.pitch:F1}°";
            }
            else
            {
                yawText.text = "—";
                pitchText.text = "out of reach";
            }
        }
        else if (currentMode == GameMode.Drone && currentDrone != null)
        {
            currentDrone.StartFlight(startPos, targetPos, speed);
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

    private Vector3 GetStartPosition() => new Vector3(ParseFloat(startX.text), ParseFloat(startY.text), ParseFloat(startZ.text));
    private Vector3 GetTargetPosition() => new Vector3(ParseFloat(targetX.text), ParseFloat(targetY.text), ParseFloat(targetZ.text));

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

    /// <summary>
    /// Creates instance of an object and changes UI depending on chosen mode
    /// </summary>
    /// <param name="index"></param>
    private void OnModeChanged(int index)
    {
        currentMode = (GameMode)index;

        if (currentObject != null)
            Destroy(currentObject);

        Vector3 startPos = GetStartPosition();

        if (currentMode == GameMode.Launcher)
        {
            currentObject = Instantiate(launcherPrefab, startPos, Quaternion.identity);
            currentLauncher = currentObject.GetComponent<Launcher>();

            anglesSection.SetActive(true);
            releaseSection.SetActive(false);
        }
        else
        {
            currentObject = Instantiate(dronePrefab, startPos, Quaternion.identity);
            currentDrone = currentObject.GetComponent<Drone>();

            anglesSection.SetActive(false);
            releaseSection.SetActive(true);
        }
    }
}