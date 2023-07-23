using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    [SerializeField] int platformWidth;
    [SerializeField] float colliderHeight;
    [SerializeField] float triggerHeight;
    [SerializeField] Sprite sprite;
    [SerializeField] Sprite[] sprites;

    private GameObject[] spriteObjects;
    private BoxCollider2D platformCollider;
    private BoxCollider2D platformTrigger;

    private void Awake()
    {
        Setup();
    }

    private void Setup ()
    {
        System.Random rand = new System.Random();

        spriteObjects = new GameObject[platformWidth * 2];
        for (int i = 0; i < platformWidth * 2; i++)
        {
            spriteObjects[i] = new GameObject("Platform_" + System.Convert.ToString(i/2f));
            spriteObjects[i].transform.position = transform.position;
            spriteObjects[i].transform.parent = transform;
            spriteObjects[i].transform.position += new Vector3(i / 2f - 0.25f, 0.25f, 0);

            if (i >= 1 && i < platformWidth * 2 - 1)
            {
                int spriteIndex = rand.Next(0, 3);
                spriteObjects[i].AddComponent<SpriteRenderer>().sprite = sprites[spriteIndex];
                rand = new System.Random(spriteIndex);
            }
            else if (i == 0)
                spriteObjects[i].AddComponent<SpriteRenderer>().sprite = sprites[0];
            else if (i == platformWidth * 2 - 1)
                spriteObjects[i].AddComponent<SpriteRenderer>().sprite = sprites[3];
        }

        platformCollider = gameObject.AddComponent<BoxCollider2D>();
        platformCollider.size = new Vector2(platformWidth, colliderHeight);
        platformCollider.offset = new Vector2((platformWidth - 1f) / 2f, (1f - colliderHeight) / 2f);

        GameObject triggerObject = new GameObject("PlatformTrigger");
        triggerObject.transform.parent = transform;
        triggerObject.transform.localPosition = Vector3.zero;
        triggerObject.layer = 7;
        platformTrigger = triggerObject.AddComponent<BoxCollider2D>();
        platformTrigger.isTrigger = true;
        platformTrigger.size = new Vector2(platformWidth - 0.5f, triggerHeight);
        platformTrigger.offset = new Vector2((platformWidth - 1f) / 2f, 1f + triggerHeight / 2f);
        triggerObject.AddComponent<PlatformTrigger>().platformCollider = platformCollider;
    }

}
