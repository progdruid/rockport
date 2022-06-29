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
        GameManager gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        yield return gm.TransiteIn();
        SceneManager.LoadSceneAsync("End");
    }
}
