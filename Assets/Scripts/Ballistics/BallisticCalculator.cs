using UnityEngine;

public static class BallisticCalculator
{
    public static BallisticResult SolveBallisticArc(
    Vector3 start,
    Vector3 target,
    float speed,
    float gravity)
    {
        BallisticResult result = new BallisticResult { success = false, yaw = 0f, pitch = 0f };

        Vector3 diff = target - start;
        // Horizontal angle
        result.yaw = Mathf.Atan2(diff.x, diff.z) * Mathf.Rad2Deg;

        float dxz = new Vector2(diff.x, diff.z).magnitude;
        float dy = diff.y;

        float v2 = speed * speed;
        float g = gravity;

        float discriminant = v2 * v2 - g * (g * dxz * dxz + 2 * dy * v2);

        if (discriminant < 0)
        {
            return result;  // target is out of reach
        }

        // Low trajectory (more stable)
        float sqrt = Mathf.Sqrt(discriminant);
        result.pitch = Mathf.Atan((v2 - sqrt) / (g * dxz)) * Mathf.Rad2Deg;
        result.success = true;

        return result;
    }
}
