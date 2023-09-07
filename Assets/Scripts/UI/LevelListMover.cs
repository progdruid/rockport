using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelListMover : MonoBehaviour
{
    [SerializeField] float acceleration;
    [SerializeField] float maxSpeed;
    [SerializeField] float oscillationAcc;
    [SerializeField] float oscillationMinInitSpeed;
    [SerializeField] float oscillationMaxInitSpeed;
    [SerializeField] float nearThreshold;
    [SerializeField] LevelListFiller listFiller;

    private int heightInLevels;
    private float unitHeight;
    private float defaultY;

    private float speed = 0;
    private int selectedUnit = 0;
    private bool oscillating = false;
    private bool oscillationDone = true;
    private bool selectionChanged = false;

    private float selectedY => defaultY - unitHeight * selectedUnit;

    public void UpdateLevelCount(int count) => heightInLevels = count;

    public int GetSelectedLevel() => selectedUnit + 1;

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

    private void FixedUpdate()
    {
        if (oscillating)
            return;

        int dir = (int)Mathf.Sign(selectedY - transform.position.y);
        float nextSpeed = speed + acceleration * dir * Time.fixedDeltaTime;
        nextSpeed = Mathf.Sign(nextSpeed) * Mathf.Clamp(Mathf.Abs(nextSpeed), 0f, maxSpeed);
        float nextY = transform.position.y + nextSpeed * Time.fixedDeltaTime;
        bool selectionBetweenCurrentAndNext = (transform.position.y - selectedY) * (selectedY - nextY) > 0;
        bool inThreshold = Mathf.Abs(selectedY - transform.position.y) <= nearThreshold;

        //yes, both check are neseccary
        //first one is there to avoid trembling
        //second one is there to fix a bug with passing the threshold range in one frame that is left unhandled by the first check
        if ((inThreshold || selectionBetweenCurrentAndNext) && !oscillationDone)
        {
            transform.position = new(transform.position.x, selectedY, transform.position.z);
            selectionChanged = false;

            if (speed != 0)
                StartCoroutine(OscillationRoutine(speed));

            oscillationDone = true;
            speed = 0;
        }
        else if (!inThreshold && !selectionBetweenCurrentAndNext)
        {
            transform.position = new Vector3(transform.position.x, nextY, transform.position.z);
            speed = nextSpeed;
        }
    }

    void Start()
    {
        defaultY = transform.position.y;
        unitHeight = listFiller.GetElementHeight();
    }
}
