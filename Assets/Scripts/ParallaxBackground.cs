using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [SerializeField] [Range(0f, 1f)] float parallaxCoeficient;

    private Transform targetTransform;

    void Start()
    {
        targetTransform = Camera.main.gameObject.transform;
    }

    void FixedUpdate()
    {
        transform.position = new Vector3(targetTransform.position.x * parallaxCoeficient, targetTransform.position.y * parallaxCoeficient, transform.position.z);
    }
}
