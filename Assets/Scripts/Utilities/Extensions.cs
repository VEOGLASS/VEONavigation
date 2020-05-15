using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public static class Extensions
{
    #region String

    public static string Clean(this string str)
    {
        string rstr = str.Replace('\t', ' ');
        while (rstr.Contains("  ")) rstr = rstr.Replace("  ", " ");
        return rstr.Trim();
    }

    #endregion

    public static void ForEachWithIndex<T>(this IEnumerable<T> enumerable, Action<T, int> handler)
    {
        int index = 0;
        foreach (T item in enumerable)
        {
            handler(item, index++);
        }
    }

    #region Vector2&Rect

    public static Vector2 Abs(this Vector2 vector)
    {
        for (int i = 0; i < 2; ++i) vector[i] = Mathf.Abs(vector[i]);
        return vector;
    }

    public static Vector2 DividedBy(this Vector2 vector, Vector2 divisor)
    {
        for (int i = 0; i < 2; ++i) vector[i] /= divisor[i];
        return vector;
    }

    public static Vector2 Max(this Rect rect)
    {
        return new Vector2(rect.xMax, rect.yMax);
    }

    public static Vector2 IntersectionWithRayFromCenter(this Rect rect, Vector2 pointOnRay)
    {
        var point = pointOnRay - rect.center;
        var edgeToRayRatios = (rect.Max() - rect.center).DividedBy(point.Abs());
        return (edgeToRayRatios.x < edgeToRayRatios.y) ?
            new Vector2(point.x > 0 ? rect.xMax : rect.xMin,
                point.y * edgeToRayRatios.x + rect.center.y) :
            new Vector2(point.x * edgeToRayRatios.y + rect.center.x,
                point.y > 0 ? rect.yMax : rect.yMin);
    }

    #endregion

    public static T Next<T>(this T src) where T : struct
    {
        if (!typeof(T).IsEnum) throw new ArgumentException($"Argument {typeof(T).FullName} is not an Enum");
        T[] arr = (T[])Enum.GetValues(src.GetType());
        int j = Array.IndexOf<T>(arr, src) + 1;
        return (arr.Length == j) ? arr[0] : arr[j];
    }

    public static void SetNext<T>(this T src) where T : struct
    {
        if (!typeof(T).IsEnum) throw new ArgumentException($"Argument {typeof(T).FullName} is not an Enum");
        T[] arr = (T[])Enum.GetValues(src.GetType());
        int j = Array.IndexOf<T>(arr, src) + 1;
        src = (arr.Length == j) ? arr[0] : arr[j];
    }
}
