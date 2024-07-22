using UnityEngine;

public class Utils
{
    public static float MagnitudeInDirection(Vector3 v, Vector3 direction)
    {
        return Vector3.Dot(v, direction);
    }
}