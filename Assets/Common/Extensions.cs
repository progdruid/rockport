
using UnityEngine;

public static class Extensions
{
    public static T At<T>(this T[,] array, Vector2Int pos) => array[pos.x, pos.y];
    public static void Set<T>(this T[,] array, Vector2Int pos, T value) => array[pos.x, pos.y] = value;
 
    public static T At<T>(this T[,] array, (int x, int y) pos) => array[pos.x, pos.y];
    public static void Set<T>(this T[,] array, (int x, int y) pos, T value) => array[pos.x, pos.y] = value;

    public static void SetWorldXY(this Transform transform, Vector2 pos) =>
        transform.position = new Vector3(pos.x, pos.y, transform.position.z);
    public static void SetLocalXY(this Transform transform, Vector2 pos) =>
        transform.localPosition = new Vector3(pos.x, pos.y, transform.localPosition.z);
    
    public static void SetWorldZ(this Transform transform, float z) =>
        transform.position = new Vector3(transform.position.x, transform.position.y, z);
    public static void SetLocalXY(this Transform transform, float z) =>
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, z);
    
    
    public static void SetWorld(this Transform transform, Vector2 pos, float z) =>
        transform.position = new Vector3(pos.x, pos.y, z);
    public static void SetLocal(this Transform transform, Vector2 pos, float z) =>
        transform.localPosition = new Vector3(pos.x, pos.y, z);
    
}
