using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelListBehaviour : MonoBehaviour
{
    [SerializeField] int heightInLevels;
    [SerializeField] float acceleration;
    [SerializeField] float oscillationAcc;
    [SerializeField] float oscillationMinInitSpeed;
    [SerializeField] float nearThreshold;
    [SerializeField] Sprite sampleUnitSprite;

    private float unitHeight;
    private float defaultY;

    private float speed = 0;
    private int selectedUnit = 0;
    private bool oscillating = false;
    private bool oscillationDone = true;
    private bool selectionChanged = false;

    private float selectedY => defaultY - unitHeight * selectedUnit;

    public void MoveListUp ()
    {
        SetLevel(selectedUnit - 1);
    }

    public void MoveListDown ()
    {
        SetLevel(selectedUnit + 1);
    }

    public void SetLevel (int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < heightInLevels)
        {
            selectionChanged = selectedUnit != levelIndex || selectionChanged; //make it true if selectedUnit != levelIndex
            oscillationDone = selectedUnit == levelIndex && oscillationDone; //make it false if selectedUnit != levelIndex

            selectedUnit = levelIndex;
        }
    }

    private IEnumerator OscillationRoutine (float initSpeed)
    {
        oscillating = true;

        if (Mathf.Abs(initSpeed) < oscillationMinInitSpeed)
            initSpeed = Mathf.Sign(initSpeed) * oscillationMinInitSpeed;
        
        float timeStarted = Time.time;
        float oscillationDuration = Mathf.Abs(initSpeed) * 2f / oscillationAcc;
        float accdir = -Mathf.Sign(initSpeed);
        float initY = transform.position.y;

        while (!selectionChanged && Time.time - timeStarted < oscillationDuration)
        {
            float timePassed = Time.time - timeStarted;
            float displacement = initSpeed * timePassed + oscillationAcc * accdir * timePassed * timePassed / 2f;
            transform.position = new(transform.position.x, initY + displacement, transform.position.z);
            yield return new WaitForFixedUpdate();
        }

        if (!selectionChanged)
            transform.position = new(transform.position.x, initY, transform.position.z);

        oscillating = false;
    }

    private void FixedUpdate()
    {
        if (oscillating)
            return;

        if (Mathf.Abs(selectedY - transform.position.y) <= nearThreshold && !oscillationDone)
        {
            transform.position = new(transform.position.x, selectedY, transform.position.z);
            selectionChanged = false;
            
            if (speed != 0)
                StartCoroutine(OscillationRoutine(speed));

            oscillationDone = true;
            speed = 0;
        }
        else if (Mathf.Abs(selectedY - transform.position.y) > nearThreshold)
        {
            int dir = (int)Mathf.Sign(selectedY - transform.position.y);
            speed += acceleration * dir * Time.fixedDeltaTime;
            transform.position += Vector3.up * speed * Time.fixedDeltaTime;
        }
    }

    void Start()
    {
        defaultY = transform.position.y;
        unitHeight = sampleUnitSprite.bounds.extents.y * 2;
    }
}
