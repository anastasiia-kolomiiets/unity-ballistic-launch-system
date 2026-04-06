using UnityEngine;

/// <summary>
/// Simple data structure that holds the result of a ballistic calculation.
/// Used to return release point, time and success status from BallisticCalculator methods.
/// </summary>
/// <remarks>
/// This struct is marked [System.Serializable] so it can be inspected in the Unity Inspector
/// (if needed for debugging or custom editor tools) and serialized in ScriptableObjects/JSON/etc.
/// </remarks>
[System.Serializable]
public struct ReleaseResult
{
    /// <summary>
    /// Indicates whether a valid ballistic solution was found.
    /// True if target is reachable with given parameters, false otherwise.
    /// </summary>
    public bool success;

    /// <summary>
    /// Point in space at which a projectile must be dropped to hit the target
    /// </summary>
    public Vector3 releasePoint;

    /// <summary>
    /// Amount of seconds from start of the flight to a release point
    /// </summary>
    public float timeToRelease;
}