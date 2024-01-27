using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Custom_Debug
{
    public static void DrawRectangle(Vector2 bottom_left, Vector2 top_right)
    {
        Vector2 displacement_vector = top_right - bottom_left;
        Vector2 top_left = bottom_left + new Vector2(0, displacement_vector.y);
        Vector2 bottom_right = top_right + new Vector2(0, -displacement_vector.y);
        
        Gizmos.DrawLine(top_left, top_right);
        Gizmos.DrawLine(top_left, bottom_left);
        Gizmos.DrawLine(top_right, bottom_right);
        Gizmos.DrawLine(bottom_left, bottom_right);
    }
}
