using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public float Impulse;
    public float TimeOffset;

    private Animator animator;
    private List<(Collider2D col, IUltraJumper jumper, bool isPlayer)> bodiesInside;

    private void Start()
    {
        animator = GetComponent<Animator>();

        bodiesInside = new List<(Collider2D col, IUltraJumper jumper, bool isPlayer)>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag != "Player" && other.tag != "Corpse")
            return;

        IUltraJumper jumper = (IUltraJumper)(other.gameObject.GetComponent<Player>());
        if (jumper == null)
            jumper = (IUltraJumper)(other.gameObject.GetComponent<CorpsePhysics>());
        
        var body = (other, jumper, other.tag == "Player");
        bodiesInside.Add(body);

        StartCoroutine(Push(body));
    }

    private IEnumerator Push ((Collider2D col, IUltraJumper jumper, bool isPlayer) pressingBody)
    {
        pressingBody.jumper.PresetUltraJumped(true);
        
        animator.SetTrigger("Pressed");
        yield return new WaitForSeconds(TimeOffset);
        
        pressingBody.jumper.MakeUltraJump(Impulse);
    }

    private void OnTriggerExit2D (Collider2D other)
    {
        bodiesInside.RemoveAll((x) => x.col == other);

        if (other.tag == "Player")
            Registry.ins.inputSet.CanJump = true;
    }
    
    private void Update ()
    {
        for (int i = 0; i < bodiesInside.Count; i++)
        {
            bodiesInside[i] = (bodiesInside[i].col, bodiesInside[i].jumper, bodiesInside[i].isPlayer);
            StartCoroutine(Push(bodiesInside[i]));
        }
    }
}
