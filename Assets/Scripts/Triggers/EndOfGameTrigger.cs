using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndOfGameTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag != "Player")
            return;

        StartCoroutine(LoadEndScene());
    }

    private IEnumerator LoadEndScene ()
    {
        TransitionController tk = GameObject.FindGameObjectWithTag("GameController").GetComponent<TransitionController>();
        yield return tk.TransiteIn();
        SceneManager.LoadSceneAsync("End");
    }
}
