using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [SerializeField] [Range(0f, 1f)] float parallaxCoeficient;
    
    private LevelLoader levelLoader;
    private Vector2 equatorialLinePoint;
    private Transform targetTransform;

    private void Awake()
    {
        levelLoader = GameObject.FindGameObjectWithTag("GameController").GetComponent<LevelLoader>();
        levelLoader.levelInstantiationEvent += Init;
    }

    private void OnDestroy()
    {
        if (levelLoader != null)
            levelLoader.GetComponent<LevelLoader>().levelInstantiationEvent -= Init;
    }

    void Start()
    {
        Init();
    }

    private void Init()
    {
        targetTransform = Camera.main.gameObject.transform;

        GameObject foundObject = levelLoader.TryFindObjectWithTag("EquatorialLineObject");
        if (foundObject != null)
            equatorialLinePoint = (Vector2)foundObject.transform.position;
        else
            equatorialLinePoint = Vector2.zero;

    }

    void FixedUpdate() 
    {
        transform.position = new Vector3(
            equatorialLinePoint.x * (1f - parallaxCoeficient) + targetTransform.position.x * parallaxCoeficient,
            equatorialLinePoint.y * (1f - parallaxCoeficient) + targetTransform.position.y * parallaxCoeficient,
            transform.position.z);
    }
}
