using System.Collections;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;

public static class Extensions
{
    //matrix access
    public static T At<T>(this T[,] array, Vector2Int pos) => array[pos.x, pos.y];
    public static void Set<T>(this T[,] array, Vector2Int pos, T value) => array[pos.x, pos.y] = value;
 
    public static T At<T>(this T[,] array, (int x, int y) pos) => array[pos.x, pos.y];
    public static void Set<T>(this T[,] array, (int x, int y) pos, T value) => array[pos.x, pos.y] = value;
    
    //coroutines
    public static Coroutine Start(this IEnumerator routine, MonoBehaviour behaviour) =>
        behaviour.StartCoroutine(routine);
    public static void Stop(this Coroutine routine, MonoBehaviour behaviour) =>
        behaviour.StopCoroutine(routine);
    
    //transforms
    public static void SetWorldX(this Transform transform, float x) => 
        transform.position = new Vector3(x, transform.position.y, transform.position.z);
    public static void SetWorldY(this Transform transform, float y) =>
        transform.position = new Vector3(transform.position.x, y, transform.position.z);
    public static void SetWorldZ(this Transform transform, float z) =>
        transform.position = new Vector3(transform.position.x, transform.position.y, z);
    public static void SetWorldXY(this Transform transform, float x, float y) =>
        transform.position = new Vector3(x, y, transform.position.z);
    public static void SetWorldXY(this Transform transform, Vector2 pos) =>
        transform.position = new Vector3(pos.x, pos.y, transform.position.z);
    public static void SetWorld(this Transform transform, Vector2 pos, float z) =>
        transform.position = new Vector3(pos.x, pos.y, z);
    public static void SetWorld(this Transform transform, float x, float y, float z) =>
        transform.position = new Vector3(x, y, z);
    
    
    
    public static void SetLocalX(this Transform transform, float x) =>
        transform.localPosition = new Vector3(x, transform.position.y, transform.position.z);
    public static void SetLocalY(this Transform transform, float y) =>
        transform.localPosition = new Vector3(transform.position.x, y, transform.position.z);
    public static void SetLocalZ(this Transform transform, float z) =>
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, z);
    public static void SetLocalXY(this Transform transform, float x, float y) =>
        transform.localPosition = new Vector3(x, y, transform.localPosition.z);
    public static void SetLocalXY(this Transform transform, Vector2 pos) =>
        transform.localPosition = new Vector3(pos.x, pos.y, transform.localPosition.z);
    public static void SetLocal(this Transform transform, Vector2 pos, float z) =>
        transform.localPosition = new Vector3(pos.x, pos.y, z);
    public static void SetLocal(this Transform transform, float x, float y, float z) =>
        transform.localPosition = new Vector3(x, y, z);
    
    
    
    //vectors
    public static bool IsApproximately(this Vector2 a, Vector2 b) =>
        Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y);
    public static bool IsApproximately(this Vector3 a, Vector3 b) =>
        Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y) && Mathf.Approximately(a.z, b.z);
    public static Vector2 SmoothDamp(this Vector2 value, Vector2 target, ref Vector2 currentVelocity, float smoothTime) =>
        Vector2.SmoothDamp(value, target, ref currentVelocity, smoothTime);
    public static Vector3 SmoothDamp(this Vector3 value, Vector3 target, ref Vector3 currentVelocity, float smoothTime) =>
        Vector3.SmoothDamp(value, target, ref currentVelocity, smoothTime);
    public static Vector3 To3(this Vector2 v) => v;
    public static Vector2 To2(this Vector3 v) => v;
    public static Vector2Int FloorToInt(this Vector2 v) => Vector2Int.FloorToInt(v);
    public static Vector3Int FloorToInt(this Vector3 v) => Vector3Int.FloorToInt(v);
    public static Vector2Int RoundToInt(this Vector2 v) => Vector2Int.RoundToInt(v);
    public static Vector3Int RoundToInt(this Vector3 v) => Vector3Int.RoundToInt(v);
    public static Vector2Int CeilToInt(this Vector2 v) => Vector2Int.CeilToInt(v);
    public static Vector3Int CeilToInt(this Vector3 v) => Vector3Int.CeilToInt(v);
    
    public static JSONNode ToJson(this Vector2Int v)
    {
        var array = new JSONArray();
        array.WriteVector2Int(v);
        return array;
    }
    public static JSONNode ToJson(this Vector3Int v)
    {
        var array = new JSONArray();
        array.WriteVector3Int(v);
        return array;
    }
    

    //floats
    public static bool IsApproximately(this float a, float b) => Mathf.Approximately(a, b);
    public static float Abs(this float value) => Mathf.Abs(value);
    public static float Mod(this float a, float b) => (a % b + b) % b;
    public static float Sign(this float value) => Mathf.Sign(value);
    public static float Lerp(this float t, float value1, float value2) => Mathf.Lerp(value1, value2, t);
    public static float ClampBottom(this float value, float bound) => Mathf.Max(value, bound);
    public static float ClampTop(this float value, float bound) => Mathf.Min(value, bound);
    public static float Round(this float value) => Mathf.Round(value);
    public static int RoundToInt(this float value) => Mathf.RoundToInt(value);
    public static float Ceil(this float value) => Mathf.Ceil(value);
    public static int CeilToInt(this float value) => Mathf.CeilToInt(value);
    public static float Floor(this float value) => Mathf.Floor(value);
    public static int FloorToInt(this float value) => Mathf.FloorToInt(value);
    public static void MoveToRef(this ref float value, float target, float maxDelta) =>
        value = Mathf.MoveTowards(value, target, maxDelta);
    public static float MoveTo(this float value, float target, float maxDelta) =>
        Mathf.MoveTowards(value, target, maxDelta);
    public static float SmoothDamp(this float value, float target, ref float currentVelocity, float smoothTime) =>
        Mathf.SmoothDamp(value, target, ref currentVelocity, smoothTime);
    
    //ints
    public static int Abs(this int value) => Mathf.Abs(value);
    public static int Sign(this int value) => (int)Mathf.Sign(value);
    public static int ClampBottom(this int value, int bound) => Mathf.Max(value, bound);
    public static int ClampTop(this int value, int bound) => Mathf.Min(value, bound);
}
