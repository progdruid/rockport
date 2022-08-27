using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndOfGameTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == 7)
            return;

        if (!other.GetComponent<SignComponent>().HasSign("Player"))
            return;

        StartCoroutine(LoadEndScene());
    }

    private IEnumerator LoadEndScene ()
    {
        TransitionController tk = SignComponent.FindEntity("LevelManager").GetComponent<TransitionController>();
        yield return tk.TransiteIn();
        SceneManager.LoadSceneAsync("End");
    }
}
