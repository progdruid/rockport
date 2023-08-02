using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrictionJoiner : MonoBehaviour
{
    [SerializeField] BodySideTrigger bottomTrigger;

    private FrictionJoint2D joint;

    private void Start()
    {
        joint = GetComponent<FrictionJoint2D>();
        joint.enabled = false;

        bottomTrigger.EnterEvent += HandleBodyUpdate;
        bottomTrigger.ExitEvent += HandleBodyUpdate;
    }

    private void OnDestroy()
    {
        bottomTrigger.EnterEvent -= HandleBodyUpdate;
        bottomTrigger.ExitEvent -= HandleBodyUpdate;
    }

    private void HandleBodyUpdate (Collider2D other, TriggeredType type)
    {
        if (bottomTrigger.body != null)
        {
            joint.enabled = true;
            joint.connectedBody = bottomTrigger.body;
        }
        else
        {
            joint.connectedBody = null;
            joint.enabled = false;
        }
    }
}
