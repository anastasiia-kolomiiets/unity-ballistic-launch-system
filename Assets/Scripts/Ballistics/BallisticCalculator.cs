using System.Security.Cryptography.X509Certificates;
using UnityEngine;

/// <summary>
/// Static class containing ballistic trajectory calculation methods.
/// Supports both vacuum (no drag) and quadratic air drag models.
/// </summary>
public static class BallisticCalculator
{
    public static float gravity = Physics.gravity.magnitude;

    public static BallisticResult CalculateLaunchAngles(Vector3 start,
        Vector3 target,
        float launchSpeed,
        AirResistanceSettings airResistanceSettings)
    {
        if (!airResistanceSettings.useAirDrag)
        {
            return SolveBallisticArc(start, target, launchSpeed);
        }
        else
        {
            return SolveBallisticArcWithDrag(start, target, launchSpeed, airResistanceSettings);
        }
    }

    /// <summary>
    /// Calculates launch angles (yaw and pitch) assuming no air resistance (vacuum / analytical solution).
    /// Uses the standard low-angle ballistic formula (minus root solution).
    /// </summary>
    /// <param name="start">Starting position of the projectile</param>
    /// <param name="target">Target position</param>
    /// <param name="speed">Initial projectile speed (m/s)</param>
    /// <returns>BallisticResult with yaw, pitch and success flag</returns>
    private static BallisticResult SolveBallisticArc(
        Vector3 start,
        Vector3 target,
        float speed)
    {
        BallisticResult result = new BallisticResult { success = false, yaw = 0f, pitch = 0f };

        Vector3 diff = target - start;
        // Horizontal direction (yaw)
        result.yaw = Mathf.Atan2(diff.x, diff.z) * Mathf.Rad2Deg;

        float dxz = new Vector2(diff.x, diff.z).magnitude;
        float dy = diff.y;

        // Special case: almost vertical shot (avoid division by zero)
        if (dxz < 0.01f)
        {
            result.pitch = dy > 0 ? 90f : -90f;
            result.success = true;
            return result;
        }

        float v2 = speed * speed;

        // Discriminant of the quadratic equation for tan(theta)
        float discriminant = v2 * v2 - gravity * (gravity * dxz * dxz + 2 * dy * v2);

        if (discriminant < 0)
        {
            return result;  // target is out of reach
        }

        // Low trajectory solution (more stable, usually preferred)
        float sqrt = Mathf.Sqrt(discriminant);
        result.pitch = Mathf.Atan((v2 - sqrt) / (gravity * dxz)) * Mathf.Rad2Deg;
        result.success = true;

        return result;
    }

    /// <summary>
    /// Calculates launch pitch angle taking quadratic air drag into account.
    /// Uses secant method + forward Euler integration to find correct elevation angle.
    /// </summary>
    /// <param name="start">Launch position</param>
    /// <param name="target">Target position</param>
    /// <param name="speed">Muzzle velocity (m/s)</param>
    /// <param name="airResistanceSettings">All projectile physics parameters including air resistance, mass, and drag.</param>
    /// <param name="maxIterations">Maximum secant method iterations</param>
    /// <param name="tolerance">Acceptable vertical error at target distance (meters)</param>
    /// <returns>BallisticResult with computed yaw, pitch and success flag</returns>
    private static BallisticResult SolveBallisticArcWithDrag(
        Vector3 start,
        Vector3 target,
        float speed,
        AirResistanceSettings airResistanceSettings,
        int maxIterations = 35,
        float tolerance = 0.08f)
    {
        BallisticResult result = new BallisticResult { success = false, yaw = 0f, pitch = 0f };

        Vector3 diff = target - start;
        result.yaw = Mathf.Atan2(diff.x, diff.z) * Mathf.Rad2Deg;

        float dxz = new Vector2(diff.x, diff.z).magnitude;
        float dy = diff.y;

        if (dxz < 0.01f)
        {
            result.pitch = dy > 0 ? 90f : -90f;
            result.success = true;
            return result;
        }

        // Use vacuum solution as first guess (usually underestimates pitch when drag is present)
        float noDragPitch = 45f;
        {
            float v2 = speed * speed;
            float discriminant = v2 * v2 - gravity * (gravity * dxz * dxz + 2 * dy * v2);
            if (discriminant >= 0)
            {
                float sqrtD = Mathf.Sqrt(discriminant);
                noDragPitch = Mathf.Atan((v2 - sqrtD) / (gravity * dxz)) * Mathf.Rad2Deg;
            }
        }

        float p0 = noDragPitch;
        float p1 = noDragPitch + 8f;

        for (int i = 0; i < maxIterations; i++)
        {
            float y0 = SimulateHitHeight(dxz, speed, p0, airResistanceSettings);
            float y1 = SimulateHitHeight(dxz, speed, p1, airResistanceSettings);

            if (y0 < -500f || y1 < -500f) return result; // cannot reach

            float e0 = y0 - dy;
            float e1 = y1 - dy;

            if (Mathf.Abs(e0) < tolerance)
            {
                result.pitch = p0;
                result.success = true;
                return result;
            }
            if (Mathf.Abs(e1) < tolerance)
            {
                result.pitch = p1;
                result.success = true;
                return result;
            }

            if (Mathf.Abs(e1 - e0) < 0.001f) break;

            // Secant method update
            float p2 = p1 - e1 * (p1 - p0) / (e1 - e0);
            p0 = p1;
            p1 = Mathf.Clamp(p2, 5f, 85f);
        }

        // Final validation with slightly relaxed tolerance
        float finalY = SimulateHitHeight(dxz, speed, p1, airResistanceSettings);
        if (Mathf.Abs(finalY - dy) < 2f)
        {
            result.pitch = p1;
            result.success = true;
        }

        return result;
    }

    /// <summary>
    /// Forward Euler integration of projectile motion with quadratic drag and gravity.
    /// Returns interpolated height when horizontal distance reaches dxz.
    /// </summary>
    private static float SimulateHitHeight(float dxz, float speed, float pitchDeg, AirResistanceSettings airResistanceSettings)
    {
        float pitchRad = pitchDeg * Mathf.Deg2Rad;
        float vx = speed * Mathf.Cos(pitchRad);
        float vy = speed * Mathf.Sin(pitchRad);

        float x = 0f;
        float y = 0f;
        float dt = 0.005f;          // integration step — smaller = more accurate, but slower
        float maxT = 200f;
        float t = 0f;
        float prevX = 0f;
        float prevY = 0f;

        while (x < dxz && t < maxT)
        {
            float speedSq = vx * vx + vy * vy;
            float speedCur = Mathf.Sqrt(speedSq);
            if (speedCur < 0.05f) break;

            prevX = x;
            prevY = y;

            float dragMag = 0.5f * airResistanceSettings.airDensity * airResistanceSettings.dragCoefficient * airResistanceSettings.crossSectionArea * speedSq;
            float ax = -(vx / speedCur) * (dragMag / airResistanceSettings.mass);
            float ay = -(vy / speedCur) * (dragMag / airResistanceSettings.mass) - gravity;

            vx += ax * dt;
            vy += ay * dt;
            x += vx * dt;
            y += vy * dt;
            t += dt;
        }

        if (x < dxz) return -999f; // did not reach horizontal distance

        float frac = (dxz - prevX) / (x - prevX + 0.0001f);
        return prevY + frac * (y - prevY);
    }

    /// <summary>
    /// Calculates the optimal release point and time for a drone to drop a payload 
    /// so that it lands at the target position, taking into account optional air resistance.
    /// </summary>
    /// <param name="startPosition">Current position of the drone.</param>
    /// <param name="targetPosition">Desired landing position of the payload.</param>
    /// <param name="droneSpeed">Horizontal speed of the drone (m/s).</param>
    /// <param name="airSettings">Air resistance settings. If air drag is disabled, a simple analytical solution is used.</param>
    /// <returns>
    /// A ReleaseResult containing:
    /// - Success flag
    /// - Recommended release point (same height as drone)
    /// - Time (in seconds) for the drone to reach the release point from current position
    /// </returns>
    public static ReleaseResult CalculateDroneDrop(
        Vector3 startPosition,
        Vector3 targetPosition,
        float droneSpeed,
        AirResistanceSettings airSettings)
    {
        float height = startPosition.y - targetPosition.y;
        if (height <= 0.01f)
            return new ReleaseResult { success = false, releasePoint = startPosition, timeToRelease = 0f };

        Vector3 direction = targetPosition - startPosition;
        direction.y = 0f;          // Ignore height
        direction.Normalize();     // Normalize horizontal vector

        if (direction.sqrMagnitude < 0.01f)
            direction = Vector3.forward;

        Vector3 horizontalVel = direction * droneSpeed;

        Vector3 releasePoint;

        if (!airSettings.useAirDrag)
        {
            releasePoint = CalculateDropReleasePoint(startPosition, targetPosition, horizontalVel);
        }
        else
        {
            releasePoint = CalculateDropReleasePointWithDrag(startPosition, targetPosition, horizontalVel, airSettings);
        }

        Vector3 moveDir = horizontalVel.normalized;
        Vector3 toRelease = releasePoint - startPosition;
        toRelease.y = 0f;

        float forwardDistance = Vector3.Dot(toRelease, moveDir);

        if (forwardDistance <= 0f)
        {
            return new ReleaseResult { success = false, releasePoint = releasePoint, timeToRelease = 0f };  // if release point is behind the drone, target is out of reach
        }

        float timeToRelease = forwardDistance / droneSpeed;

        return new ReleaseResult { success = true, releasePoint = releasePoint, timeToRelease = timeToRelease };
    }

    /// <summary>
    /// Calculates the release point for dropping a payload using a simple analytical solution 
    /// (no air resistance / quadratic motion under constant gravity).
    /// </summary>
    /// <param name="dronePosition">Position of the drone at the moment of release.</param>
    /// <param name="targetPosition">Desired landing position of the payload.</param>
    /// <param name="droneHorizontalVelocity">Horizontal velocity vector of the drone.</param>
    /// <returns>
    /// The position where the drone should release the payload.
    /// </returns>
    private static Vector3 CalculateDropReleasePoint(
        Vector3 dronePosition,
        Vector3 targetPosition,
        Vector3 droneHorizontalVelocity)
    {
        float height = dronePosition.y - targetPosition.y;

        float fallTime = Mathf.Sqrt(2f * height / gravity);
        Vector3 leadOffset = droneHorizontalVelocity * fallTime;

        Vector3 releasePoint = targetPosition - leadOffset;
        releasePoint.y = dronePosition.y;

        return releasePoint;
    }

    /// <summary>
    /// Calculates the release point for dropping a payload taking quadratic air drag into account.
    /// Uses a single forward Euler simulation to compute the exact horizontal range the bomb will travel.
    /// No iterative solver (secant method) is needed because the initial velocity and direction are fixed.
    /// </summary>
    /// <param name="dronePosition">Current drone position (release height).</param>
    /// <param name="targetPosition">Desired impact position.</param>
    /// <param name="droneHorizontalVelocity">Drone's horizontal velocity vector at release.</param>
    /// <param name="airSettings">Air resistance settings (Cd, mass, area, density).</param>
    /// <returns>World-space release point at the same height as the drone.</returns>
    private static Vector3 CalculateDropReleasePointWithDrag(
        Vector3 dronePosition,
        Vector3 targetPosition,
        Vector3 droneHorizontalVelocity,
        AirResistanceSettings airSettings)
    {
        float height = dronePosition.y - targetPosition.y;
        if (height <= 0.01f)
            return dronePosition;

        // Horizontal direction and distance to target
        Vector3 horizDir = (targetPosition - dronePosition);
        horizDir.y = 0f;
        float targetRange = horizDir.magnitude;

        if (targetRange < 0.01f)
            return dronePosition;

        horizDir.Normalize();

        float droneSpeed = droneHorizontalVelocity.magnitude;
        if (droneSpeed < 0.1f)
            return dronePosition;

        // Simulate the exact horizontal distance the bomb will travel under drag
        float actualRange = SimulateBombHorizontalRange(droneSpeed, height, airSettings);

        // Release point is shifted backward by the distance the bomb will actually fly
        Vector3 releasePoint = targetPosition - horizDir * actualRange;
        releasePoint.y = dronePosition.y;

        return releasePoint;
    }

    /// <summary>
    /// Forward Euler integration that simulates the free-fall of a bomb with quadratic air drag.
    /// Starts with horizontal velocity only (vy = 0). Returns the total horizontal distance
    /// traveled until the bomb reaches y = 0.
    /// </summary>
    /// <param name="initialHorizontalSpeed">Horizontal speed the bomb receives from the drone.</param>
    /// <param name="height">Drop height (drone y - target y).</param>
    /// <param name="airSettings">Air resistance parameters.</param>
    /// <returns>Horizontal distance (meters) the bomb will travel before hitting ground.</returns>
    private static float SimulateBombHorizontalRange(
        float initialHorizontalSpeed,
        float height,
        AirResistanceSettings airSettings)
    {
        float vx = initialHorizontalSpeed;   // initial horizontal velocity
        float vy = 0f;                       // initial vertical velocity = 0
        float x = 0f;
        float y = height;

        float dt = 0.004f;                   // integration step (high precision)

        float prevX = 0f;
        float prevY = height;

        while (y > 0f)
        {
            prevX = x;
            prevY = y;

            float speedSq = vx * vx + vy * vy;
            if (speedSq < 0.0025f) break;    // velocity is almost zero

            float speed = Mathf.Sqrt(speedSq);

            // Quadratic drag force magnitude
            float dragMag = 0.5f * airSettings.airDensity *
                            airSettings.dragCoefficient *
                            airSettings.crossSectionArea * speedSq;

            // Acceleration components
            float ax = -(vx / speed) * (dragMag / airSettings.mass);
            float ay = -(vy / speed) * (dragMag / airSettings.mass) - gravity;

            // Semi-implicit Euler integration (velocity first, then position)
            vx += ax * dt;
            vy += ay * dt;

            x += vx * dt;
            y += vy * dt;
        }

        // Linear interpolation for exact y = 0 crossing (sub-step accuracy)
        if (y < 0f && prevY > 0f)
        {
            float frac = prevY / (prevY - y);
            x = prevX + frac * (x - prevX);
        }

        return x;
    }
}