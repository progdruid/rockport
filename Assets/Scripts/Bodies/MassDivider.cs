using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MassDivider : MonoBehaviour
{
    public bool active;
    [SerializeField] StayChecker stayChecker;

    public float additionalMass { get; private set; }
    public float normalMass { get; private set; }
    public float massMult => (normalMass + additionalMass) / normalMass;

    private MassDivider otherDivider;

    private void Start()
    {
        var rb = GetComponent<Rigidbody2D>();
        normalMass = rb.mass;

        stayChecker.EnterEvent += HandleBodyEnter;
        stayChecker.ExitEvent += HandleBodyExit;
    }

    private void OnDestroy()
    {
        stayChecker.EnterEvent -= HandleBodyEnter;
        stayChecker.ExitEvent -= HandleBodyExit;
    }

    private void HandleBodyEnter(Collider2D other)
    {
        if (!active || otherDivider != null)
            return;

        bool found = other.TryGetComponent(out otherDivider);
        if (found)
            otherDivider.ChangeAdditionalMass(normalMass + additionalMass);

    }

    private void HandleBodyExit(Collider2D other)
    {
        if (otherDivider != null)
            otherDivider.ChangeAdditionalMass(-(normalMass + additionalMass));
        otherDivider = null;
    }

    public void ChangeAdditionalMass(float changeOfMass)
    {
        additionalMass += changeOfMass;
        if (otherDivider != null)
            otherDivider.ChangeAdditionalMass(changeOfMass);
    }
}
