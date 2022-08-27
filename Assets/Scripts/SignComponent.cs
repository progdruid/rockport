using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SignComponent : MonoBehaviour
{
    #region static part
    private static Dictionary<string, List<SignComponent>> signMap;

    public static string[] GetAvailableSigns ()
    {
        return signMap.Keys.ToArray();
    }

    public static SignComponent FindEntity (string sign)
    {
        var list = FindEntities(sign);
        if (list == null)
            return null;
        return list[0];
    }

    public static List<SignComponent> FindEntities (string sign)
    {
        if (!signMap.ContainsKey(sign))
            return null;

        signMap.TryGetValue(sign, out List<SignComponent> list);
        return list;
    }

    #endregion

    [SerializeField] string[] signs;

    public string[] GetSigns() => signs;
    public bool HasSign(string sign) => signs.Contains(sign);

    void Awake()
    {
        if (signMap == null)
        {
            signMap = new Dictionary<string, List<SignComponent>>();
        }

        for (int i = 0; i < signs.Length; i++)
        {
            List<SignComponent> list;
            bool exists = signMap.TryGetValue(signs[i], out list);
            if (!exists)
                signMap.Add(signs[i], list = new List<SignComponent>());
            list.Add(this);
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < signs.Length; i++)
        {
            signMap.TryGetValue(signs[i], out List<SignComponent> list);
            list.Remove(this);
            if (list.Count == 0)
                signMap.Remove(signs[i]);
        }
    }
}
