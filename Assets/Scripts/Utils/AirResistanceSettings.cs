using UnityEngine;

/// <summary>
/// Stores all parameters related to air resistance and projectile physics
/// used for ballistic calculations and Rigidbody setup. 
/// This structure allows passing all projectile-related physics
/// properties as a single object, improving readability and maintainability.
/// </summary>
[System.Serializable]
public struct AirResistanceSettings
{
    /// <summary>
    /// If true, the projectile will be affected by air resistance
    /// in trajectory calculations and Rigidbody physics.
    /// </summary>
    [Tooltip("Enable/disable air resistance")]
    public bool useAirDrag;
    /// <summary>
    /// Drag coefficient (Cd) of the projectile.
    /// </summary>
    [Tooltip("Drag coefficient (Cd)")]
    public float dragCoefficient;

    /// <summary>
    /// Cross-sectional area of the projectile (in square meters).
    /// This area is used to calculate drag force: Fd = 0.5 * Cd * A * ρ * v².
    /// </summary>
    [Tooltip("Cross-sectional area (m²)")]
    public float crossSectionArea;

    /// <summary>
    /// Air density in kg/m³.
    /// Standard sea-level density is 1.225 kg/m³.
    /// Used in drag calculations.
    /// </summary>
    [Tooltip("Air density (kg/m³)")]
    public float airDensity;

    /// <summary>
    /// Mass of the projectile in kilograms.
    /// Important for gravity, momentum, and drag calculations.
    /// </summary>
    [Tooltip("Projectile mass (kg)")]
    public float mass;
}