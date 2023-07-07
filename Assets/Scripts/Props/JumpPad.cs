using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public float Impulse;
    public float TimeOffset;
    public float CooldownForSameBody;

    private Animator animator;

    //parallel lists lol
    private int count;
    private List<Collider2D> collidersInside;
    private List<Rigidbody2D> rbsInside;
    private List<float> timesInside;

    private void Start()
    {
        animator = GetComponent<Animator>();

        collidersInside = new List<Collider2D>();
        rbsInside = new List<Rigidbody2D>();
        timesInside = new List<float>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        bool found = other.gameObject.TryGetComponent(out SignComponent sign);
        if (!found || !sign.HasSign("Body"))
            return;

        Rigidbody2D rb = other.gameObject.GetComponent<Rigidbody2D>();
        collidersInside.Add(other);
        rbsInside.Add(rb);
        timesInside.Add(Time.time);
        count++;

        StartCoroutine(Push(rb));
    }

    private IEnumerator Push (Rigidbody2D pressingBody)
    {
        //disables jump if it's a trigPlayer //lil hack to avoid the if statement
        bool isPlayer = pressingBody.TryGetComponent(out Player player);
        Registry.ins.inputSet.CanJump = !isPlayer;

        animator.SetTrigger("Pressed");

        yield return new WaitForSeconds(TimeOffset);

        pressingBody.velocity += new Vector2(0f, Impulse);
        if (isPlayer)
            Registry.ins.playerManager.ResetJumpCooldown();
    }

    private void OnTriggerExit2D (Collider2D other)
    {
        int id = collidersInside.IndexOf(other);
        collidersInside.RemoveAt(id);
        rbsInside.RemoveAt(id);
        timesInside.RemoveAt(id);
        count--;

        bool found = other.TryGetComponent(out Player player);

        if (found)
        {
            Registry.ins.inputSet.CanJump = true;
            player.pushedByPad = true;
        }
    }

    private void Update ()
    {
        for (int i = 0; i < count; i++)
            if (Time.time - timesInside[i] >= CooldownForSameBody)
            {
                timesInside[i] = Time.time;
                StartCoroutine(Push(rbsInside[i]));
            }
    }
}
