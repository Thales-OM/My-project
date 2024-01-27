using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class Custom_Utility
{
    public static Vector2 DirectionInputToVector(float input)
    {
        return new Vector2(input, 0);
    }
    public static T iff<T>(bool condition, T true_return, T false_return)
    {
        if(condition){
            return true_return;
        }
        return false_return;
    }
    public static void iff(bool condition, Action true_void, Action false_void)
    {
        if(condition){
            true_void.Invoke();
            return;
        }
        false_void.Invoke();
        return;
    }
}
