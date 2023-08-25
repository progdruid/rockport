using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [SerializeField] [Range(0f, 1f)] float parallaxCoeficient;
    [SerializeField] Sprite sampleSprite;
    [SerializeField] Vector2 cycleMovementSpeed;

    private LevelLoader levelLoader;
    private Vector2 equatorialLinePoint;
    private Transform targetTransform;
    private float halfWidth;

    private void Awake()
    {
        GameObject gc = GameObject.FindGameObjectWithTag("GameController");
        if (gc != null)
            levelLoader = gc.GetComponent<LevelLoader>();
        if (levelLoader != null)
            levelLoader.levelInstantiationEvent += Init;

        halfWidth = sampleSprite.bounds.extents.x;
    }

    private void Start()
    {
        if (levelLoader == null)
            Init();
    }

    private void OnDestroy()
    {
        if (levelLoader != null)
            levelLoader.GetComponent<LevelLoader>().levelInstantiationEvent -= Init;
    }

    private void Init()
    {
        targetTransform = Camera.main.gameObject.transform;

        GameObject foundObject;
        if (levelLoader != null)
            foundObject = levelLoader.TryFindObjectWithTag("EquatorialLineObject");
        else
            foundObject = GameObject.FindGameObjectWithTag("EquatorialLineObject");

        if (foundObject != null)
            equatorialLinePoint = (Vector2)foundObject.transform.position;
        else
            equatorialLinePoint = Vector2.zero;

    }

    void FixedUpdate() 
    {
        float newx = equatorialLinePoint.x * (1f - parallaxCoeficient) + targetTransform.position.x * parallaxCoeficient;
        float newy = equatorialLinePoint.y * (1f - parallaxCoeficient) + targetTransform.position.y * parallaxCoeficient;
        newx += Time.time * cycleMovementSpeed.x * (1f - parallaxCoeficient);
        newy += Time.time * cycleMovementSpeed.y * (1f - parallaxCoeficient);
        
        newx = Mod(newx - Camera.main.transform.position.x + halfWidth, halfWidth * 2) + Camera.main.transform.position.x - halfWidth;

        transform.position = new Vector3(newx, newy, transform.position.z);
    }

    private float Mod(float a, float b) => (a % b + b) % b;
}
