using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelListMover : MonoBehaviour
{
    [SerializeField] float acceleration;
    [SerializeField] float maxSpeed;
    [SerializeField] float oscillationAcc;
    [SerializeField] float oscillationMinInitSpeed;
    [SerializeField] float oscillationMaxInitSpeed;
    [SerializeField] float nearThreshold;
    [SerializeField] Sprite sampleUnitSprite;

    private int heightInLevels;
    private float unitHeight;
    private float defaultY;

    private float speed = 0;
    private int selectedUnit = 0;
    private bool oscillating = false;
    private bool oscillationDone = true;
    private bool selectionChanged = false;
    private bool selectionBetweenLastAndCurrent = false;
    private float lastFrameY = 0f;

    private float selectedY => defaultY - unitHeight * selectedUnit;

    public void UpdateLevelCount(int count) => heightInLevels = count;

    public int GetSelectedLevel() => selectedUnit;

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
        else if (Mathf.Abs(initSpeed) > oscillationMaxInitSpeed)
            initSpeed = Mathf.Sign(initSpeed) * oscillationMaxInitSpeed;
        
        float timeStarted = Time.time;
        float oscillationDuration = Mathf.Abs(initSpeed) * 2f / oscillationAcc;
        float accdir = -Mathf.Sign(initSpeed);
        float initY = transform.position.y;

        while (!selectionChanged && Time.time - timeStarted < oscillationDuration && (transform.position.y - initY) * accdir <= 0)
        { // the last one is for too high speeds when it is not able stabilise its position
            float timePassed = Time.time - timeStarted;
            float displacement = initSpeed * timePassed + oscillationAcc * accdir * timePassed * timePassed / 2f;
            transform.position = new(transform.position.x, initY + displacement, transform.position.z);
            yield return new WaitForFixedUpdate();
        }

        if (!selectionChanged)
            transform.position = new(transform.position.x, initY, transform.position.z);

        oscillating = false;
    }

    //rewrite all of this to check with selectionBetween only
    //make it with future prediction
    private void FixedUpdate()
    {
        if (oscillating)
            return;
        
        bool inThreshold = Mathf.Abs(selectedY - transform.position.y) <= nearThreshold;

        if ((inThreshold || selectionBetweenLastAndCurrent) && !oscillationDone)
        {
            transform.position = new(transform.position.x, selectedY, transform.position.z);
            selectionChanged = false;

            if (speed != 0)
                StartCoroutine(OscillationRoutine(speed));

            oscillationDone = true;
            speed = 0;
        }
        else if (!inThreshold && !selectionBetweenLastAndCurrent)
        {
            int dir = (int)Mathf.Sign(selectedY - transform.position.y);
            speed += acceleration * dir * Time.fixedDeltaTime;
            speed = Mathf.Sign(speed) * Mathf.Clamp(Mathf.Abs(speed), 0f, maxSpeed);
            transform.position += Vector3.up * speed * Time.fixedDeltaTime;
        }

        selectionBetweenLastAndCurrent = (transform.position.y - selectedY) * (selectedY - lastFrameY) > 0;
        lastFrameY = transform.position.y;
    }

    void Start()
    {
        defaultY = transform.position.y;
        unitHeight = sampleUnitSprite.bounds.extents.y * 2;
        lastFrameY = transform.position.y;
    }
}
