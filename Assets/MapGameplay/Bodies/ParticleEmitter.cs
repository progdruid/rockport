using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEmitter : MonoBehaviour
{
    private ParticleSystem _system;

    void Start()
    {
        _system = GetComponent<ParticleSystem>();
        EmitOneParticle();
        StartCoroutine(DeleteSystemAfterLifetimeRoutine());
    }

    private void EmitOneParticle ()
    {
        _system.Emit(1);
    }

    private IEnumerator DeleteSystemAfterLifetimeRoutine ()
    {
        yield return new WaitForSeconds(_system.main.startLifetime.constant);
        Destroy(gameObject);
    }
}
