
using UnityEngine;

public static class ArrayExtensions
{
    public static T At<T>(this T[,] array, Vector2Int pos) => array[pos.x, pos.y];
    public static void Set<T>(this T[,] array, Vector2Int pos, T value) => array[pos.x, pos.y] = value;
}