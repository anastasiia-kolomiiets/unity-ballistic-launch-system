using UnityEngine;

/// <summary>
/// Static class containing ballistic trajectory calculation methods.
/// Supports both vacuum (no drag) and quadratic air drag models.
/// </summary>
public static class BallisticCalculator
{
    /// <summary>
    /// Calculates launch angles (yaw and pitch) assuming no air resistance (vacuum / analytical solution).
    /// Uses the standard low-angle ballistic formula (minus root solution).
    /// </summary>
    /// <param name="start">Starting position of the projectile</param>
    /// <param name="target">Target position</param>
    /// <param name="speed">Initial projectile speed (m/s)</param>
    /// <param name="gravity">Gravitational acceleration magnitude (usually 9.81 m/s²)</param>
    /// <returns>BallisticResult with yaw, pitch and success flag</returns>
    public static BallisticResult SolveBallisticArc(
        Vector3 start,
        Vector3 target,
        float speed,
        float gravity)
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
        float g = gravity;

        // Discriminant of the quadratic equation for tan(theta)
        float discriminant = v2 * v2 - g * (g * dxz * dxz + 2 * dy * v2);

        if (discriminant < 0)
        {
            return result;  // target is out of reach
        }

        // Low trajectory solution (more stable, usually preferred)
        float sqrt = Mathf.Sqrt(discriminant);
        result.pitch = Mathf.Atan((v2 - sqrt) / (g * dxz)) * Mathf.Rad2Deg;
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
    /// <param name="gravity">Gravity magnitude (m/s²)</param>
    /// <param name="airResistanceSettings">All projectile physics parameters including air resistance, mass, and drag.</param>
    /// <param name="maxIterations">Maximum secant method iterations</param>
    /// <param name="tolerance">Acceptable vertical error at target distance (meters)</param>
    /// <returns>BallisticResult with computed yaw, pitch and success flag</returns>
    public static BallisticResult SolveBallisticArcWithDrag(
        Vector3 start,
        Vector3 target,
        float speed,
        float gravity,
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
            float g = gravity;
            float discriminant = v2 * v2 - g * (g * dxz * dxz + 2 * dy * v2);
            if (discriminant >= 0)
            {
                float sqrtD = Mathf.Sqrt(discriminant);
                noDragPitch = Mathf.Atan((v2 - sqrtD) / (g * dxz)) * Mathf.Rad2Deg;
            }
        }

        float p0 = noDragPitch;
        float p1 = noDragPitch + 8f;

        for (int i = 0; i < maxIterations; i++)
        {
            float y0 = SimulateHitHeight(dxz, speed, p0, gravity, airResistanceSettings);
            float y1 = SimulateHitHeight(dxz, speed, p1, gravity, airResistanceSettings);

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
        float finalY = SimulateHitHeight(dxz, speed, p1, gravity, airResistanceSettings);
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
    private static float SimulateHitHeight(float dxz, float speed, float pitchDeg, float g, AirResistanceSettings airResistanceSettings)
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
            float ay = -(vy / speedCur) * (dragMag / airResistanceSettings.mass) - g;

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
}