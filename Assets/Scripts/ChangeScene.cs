using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public void ChangeToNextScene()
    {
        SceneManager.LoadScene("Level1");
    }
}
