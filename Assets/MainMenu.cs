using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void ChangeScene(string _scene)
    {
        SceneManager.LoadScene(_scene);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
