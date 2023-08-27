using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelListBehaviour : MonoBehaviour
{
    [SerializeField] int heightInLevels;
    [SerializeField] float slideAnimationDuration;
    [SerializeField] Sprite sampleUnitSprite;

    private float unitHeight;
    private float defaultY;

    private int movingDir = 0;
    private int currentUnit = 0;

    private Coroutine currentRoutine;

    public void MoveListUp ()
    {
        Debug.Log(currentUnit);
        if (movingDir == 0 && currentUnit > 0)
            currentRoutine = StartCoroutine(MoveListRoutine(1));
        else if (movingDir > 0 && currentUnit > 1)
        {
            currentUnit--;
            transform.position += Vector3.up * unitHeight;
        }
    }

    public void MoveListDown ()
    {
        if (movingDir == 0 && currentUnit < heightInLevels - 1)
            currentRoutine = StartCoroutine(MoveListRoutine(-1));
        else if (movingDir > 0 && currentUnit < heightInLevels - 2)
        {
            currentUnit++;
            transform.position += Vector3.down * unitHeight;
        }
    }

    public void SetLevel (int levelIndex)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);
        movingDir = 0;

        currentUnit = levelIndex;
        transform.position = new Vector3(transform.position.x, defaultY - currentUnit * unitHeight, transform.position.z);
    }

    private IEnumerator MoveListRoutine (int dir)
    {
        movingDir = dir;
        float startTime = Time.time;

        while (Time.time - startTime < slideAnimationDuration)
        {
            float heightInUnitsAdd = InterpFunc((Time.time - startTime) / slideAnimationDuration) * dir;
            transform.position = new Vector3(
                transform.position.x, 
                defaultY - (currentUnit - heightInUnitsAdd) * unitHeight, 
                transform.position.z);
            
            yield return new WaitForFixedUpdate();
        }

        currentUnit -= dir;
        transform.position = new Vector3(transform.position.x, defaultY - currentUnit * unitHeight, transform.position.z);

        movingDir = 0;
        currentRoutine = null;
    }

    private float InterpFunc(float t) => t;


    void Start()
    {
        defaultY = transform.position.y;
        unitHeight = sampleUnitSprite.bounds.extents.y * 2;
    }
}
