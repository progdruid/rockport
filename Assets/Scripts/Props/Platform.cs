using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public int platformWidth;
    public float colliderHeight;
    public float triggerHeight;
    public Sprite sprite;

    private GameObject[] spriteObjects;
    private BoxCollider2D platformCollider;
    private BoxCollider2D platformTrigger;

    private void Start()
    {
        Registry.ins.corpseManager.NewCorpseEvent += IgnoreNewCorpse;
        Setup();
    }

    private void OnDestroy()
    {
        Registry.ins.corpseManager.NewCorpseEvent -= IgnoreNewCorpse;
    }

    private void Setup ()
    {
        spriteObjects = new GameObject[platformWidth];
        for (int i = 0; i < platformWidth; i++)
        {
            spriteObjects[i] = new GameObject("Platform_" + i);
            spriteObjects[i].transform.position = transform.position;
            spriteObjects[i].transform.parent = transform;
            spriteObjects[i].transform.position += new Vector3(i, 0, 0);
            spriteObjects[i].AddComponent<SpriteRenderer>().sprite = sprite;
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
        
        //sorry for this, unity is really great(no)
        //ignoring collision for player
        Physics2D.IgnoreCollision(platformCollider, Registry.ins.playerManager.player.GetComponent<Collider2D>(), true);
    }
    GameObject corpse;
    void IgnoreNewCorpse(GameObject corpse)
    {
        this.corpse = corpse;
        Physics2D.IgnoreCollision(platformCollider, corpse.GetComponent<Collider2D>(), true);
    }
}
