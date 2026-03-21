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

    public static BallisticResult SolveWithAirDrag(
        Vector3 start,
        Vector3 target,
        float speed,
        float gravity,
        float dragCoefficient,
        float crossSectionArea,
        float airDensity,
        float mass)
    {
        BallisticResult result = new BallisticResult { success = false, yaw = 0f, pitch = 0f };

        Vector3 diff = target - start;
        result.yaw = Mathf.Atan2(diff.x, diff.z) * Mathf.Rad2Deg;

        float dxz = new Vector2(diff.x, diff.z).magnitude;
        float dy = diff.y;

        // Бінарний пошук правильного кута θ
        float low = 0f;
        float high = 89f;
        float bestPitch = 45f;
        float bestError = float.MaxValue;

        for (int i = 0; i < 30; i++) // 30 ітерацій достатньо для точності
        {
            float mid = (low + high) / 2f;
            float testPitch = mid * Mathf.Deg2Rad;

            // Симулюємо політ з опором
            Vector3 vel = new Vector3(
                speed * Mathf.Cos(testPitch) * Mathf.Cos(result.yaw * Mathf.Deg2Rad),
                speed * Mathf.Sin(testPitch),
                speed * Mathf.Cos(testPitch) * Mathf.Sin(result.yaw * Mathf.Deg2Rad)
            );

            Vector3 pos = start;
            bool hit = false;

            for (int step = 0; step < 500; step++) // максимум 500 кроків симуляції
            {
                float speedMag = vel.magnitude;
                if (speedMag < 0.1f) break;

                // Опір
                float dragMag = 0.5f * airDensity * dragCoefficient * crossSectionArea * speedMag * speedMag;
                Vector3 dragForce = -vel.normalized * dragMag;

                Vector3 gravityForce = new Vector3(0, -gravity * mass, 0);
                Vector3 totalForce = dragForce + gravityForce;

                Vector3 acceleration = totalForce / mass;

                vel += acceleration * 0.02f; // маленький крок часу
                pos += vel * 0.02f;

                if (pos.y <= target.y + 0.5f)
                {
                    float horizontalError = Vector2.Distance(
                        new Vector2(pos.x, pos.z),
                        new Vector2(target.x, target.z)
                    );

                    if (horizontalError < bestError)
                    {
                        bestError = horizontalError;
                        bestPitch = mid;
                    }

                    if (horizontalError < 1.5f) // точність 1.5 метра
                    {
                        hit = true;
                        break;
                    }
                    break;
                }
            }

            if (hit)
                high = mid;
            else
                low = mid;
        }

        result.pitch = bestPitch;
        result.success = bestError < 3f; // вважаємо успіхом, якщо похибка < 3м

        return result;
    }
}
