using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public int platformWidth;

    public Sprite sprite;
    public float colliderHeight;

    private GameObject[] spriteObjects;
    private BoxCollider2D platformCollider;

    void Start()
    {

        Assemble();
    }

    private void Update()
    {
        Collider2D playerCollider = Registry.ins.player.GetComponent<Collider2D>();
        Vector2 playerPos = Registry.ins.player.transform.position;
        bool isValid = CheckValid(playerPos);
        
        Physics2D.IgnoreCollision(playerCollider, platformCollider, !isValid);
    }

    private bool CheckValid (Vector2 objPos)
    {
        bool isInColumn = transform.position.x < objPos.x + 1f && objPos.x < transform.position.x + platformWidth;
        bool isAbove = transform.position.y + 0.5f - colliderHeight < objPos.y - 0.4f;
        return isInColumn && isAbove;
    }

    private void Assemble ()
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
    }
}
