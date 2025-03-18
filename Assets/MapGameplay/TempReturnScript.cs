using UnityEngine;
using UnityEngine.SceneManagement;

namespace MapGameplay
{

public class TempReturnScript : MonoBehaviour
{
    public void ReturnToPreviousScene()
    {
        if (!PlayerPrefs.HasKey("GameplayReturnScene"))
            return;
        
        var sceneName = PlayerPrefs.GetString("GameplayReturnScene");
        PlayerPrefs.DeleteKey("GameplayReturnScene");
        SceneManager.LoadScene(sceneName);
    }
}

}
