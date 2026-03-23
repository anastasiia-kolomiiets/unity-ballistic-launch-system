using UnityEngine;

/// <summary>
/// Simple data structure that holds the result of a ballistic calculation.
/// Used to return yaw, pitch angles and success status from BallisticCalculator methods.
/// </summary>
/// <remarks>
/// This struct is marked [System.Serializable] so it can be inspected in the Unity Inspector
/// (if needed for debugging or custom editor tools) and serialized in ScriptableObjects/JSON/etc.
/// </remarks>
[System.Serializable]
public struct BallisticResult
{
    /// <summary>
    /// Indicates whether a valid ballistic solution was found.
    /// True if target is reachable with given parameters, false otherwise.
    /// </summary>
    public bool success;

    /// <summary>
    /// Horizontal aiming angle (yaw) in degrees.
    /// Direction from launcher to target in the XZ plane (0° = north, positive clockwise).
    /// Calculated as Atan2(diff.x, diff.z) converted to degrees.
    /// </summary>
    public float yaw;

    /// <summary>
    /// Vertical elevation angle (pitch) in degrees.
    /// Positive values = upwards launch (low/high trajectory), negative = downwards.
    /// In no-drag mode: low-angle solution.
    /// In drag mode: numerically adjusted angle to compensate for air resistance.
    /// </summary>
    public float pitch;
}