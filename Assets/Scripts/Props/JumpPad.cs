using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class JumpPad : MonoBehaviour
{
    public float Impulse;
    public float TimeOffset;

    [SerializeField] float Cooldown;
    [SerializeField] UnityEvent OnJump;

    private Animator animator;
    private List<(Collider2D col, IUltraJumper jumper, bool isPlayer, float time)> bodiesInside;

    private void Start()
    {
        animator = GetComponent<Animator>();

        bodiesInside = new ();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag != "Player" && other.tag != "Corpse")
            return;

        IUltraJumper jumper = (IUltraJumper)(other.gameObject.GetComponent<Player>());
        if (jumper == null)
            jumper = (IUltraJumper)(other.gameObject.GetComponent<CorpsePhysics>());
        
        var body = (other, jumper, other.tag == "Player", Time.time);
        bodiesInside.Add(body);

        StartCoroutine(Push(body));
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
            if (bodiesInside[i].time - Time.time >= Cooldown)
            {
                bodiesInside[i] = (bodiesInside[i].col, bodiesInside[i].jumper, bodiesInside[i].isPlayer, Time.time);
                StartCoroutine(Push(bodiesInside[i]));
            }
    }

    private IEnumerator Push ((Collider2D col, IUltraJumper jumper, bool isPlayer, float time) pressingBody)
    {
        pressingBody.jumper.PresetUltraJumped(true);

        OnJump.Invoke();
        animator.SetBool("Pressed", true);
        yield return new WaitForSeconds(TimeOffset);
        animator.SetBool("Pressed", false);
	
        pressingBody.jumper.MakeUltraJump(Impulse);
    }
}
