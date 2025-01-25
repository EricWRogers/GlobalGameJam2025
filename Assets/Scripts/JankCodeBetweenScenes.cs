using Unity.VisualScripting;
using UnityEngine;

public class JankCodeBetweenScenes : MonoBehaviour
{
    public static JankCodeBetweenScenes Instance;
    public string joinCode;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);  
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  
        }
    }

   
}
