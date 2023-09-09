using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathsBarManager : MonoBehaviour
{
    [SerializeField] GameObject DeathBar;
    [Space]
    [SerializeField] GameObject FruitIconPrefab;
    [SerializeField] int MaxFruitIcons;

    private List<Animator> fruitIcons;
    private int lastFruitCount;

    private void Awake() => Registry.ins.deathsBar = this;

    private void Start()
    {
        fruitIcons = new();
        Registry.ins.fruitManager.FruitUpdateEvent += UpdateBar;

        for (int i = 0; i < MaxFruitIcons; i++)
            CreateFruitIcon(false);
    }

    private GameObject CreateFruitIcon (bool startState)
    {
        var fruit = Instantiate(FruitIconPrefab);
        fruit.transform.SetParent(DeathBar.transform);
        fruit.transform.localScale = Vector3.one;
        fruitIcons.Add(fruit.GetComponent<Animator>());
        fruit.SetActive(startState);
        return fruit;
    }

    private void OnDestroy()
    {
        Registry.ins.fruitManager.FruitUpdateEvent -= UpdateBar;
    }

    private IEnumerator DisableFruit (int fruitIndex)
    {
        fruitIcons[fruitIndex].SetBool("Disappear", true);
        yield return new WaitWhile(() => !fruitIcons[fruitIndex].GetCurrentAnimatorStateInfo(0).IsName("End"));
        fruitIcons[fruitIndex].SetBool("Disappear", false);
        fruitIcons[fruitIndex].gameObject.SetActive(false);
    }

    public void UpdateBar ()
    {
        int fruitCount = Registry.ins.fruitManager.GetFruitsAmount();
        for (int i = 0; i < fruitIcons.Count; i++)
        {
            if (i < fruitCount)
                fruitIcons[i].gameObject.SetActive(true);
            else if (fruitCount >= i && i < lastFruitCount)
                StartCoroutine(DisableFruit(i));
            else
                fruitIcons[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < fruitCount - fruitIcons.Count; i++)
            CreateFruitIcon(true);

        lastFruitCount = fruitCount;
    }
}
