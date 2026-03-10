using UnityEngine;

public static class BallisticCalculator
{
    public static bool SolveBallisticArc(
        Vector3 start,
        Vector3 target,
        float speed,
        float gravity,
        out float yawDeg,
        out float pitchDeg)
    {
        Vector3 diff = target - start;

        // Horizontal angle
        yawDeg = Mathf.Atan2(diff.x, diff.z) * Mathf.Rad2Deg;

        float dxz = new Vector2(diff.x, diff.z).magnitude;
        float dy = diff.y;

        float v2 = speed * speed;
        float g = gravity;

        float discriminant = v2 * v2 - g * (g * dxz * dxz + 2 * dy * v2);

        if (discriminant < 0)
        {
            pitchDeg = 0;
            return false; // target is out of reach
        }

        // Low trajectory (more stable)
        float sqrt = Mathf.Sqrt(discriminant);
        pitchDeg = Mathf.Atan((v2 - sqrt) / (g * dxz)) * Mathf.Rad2Deg;

        return true;
    }
}
