using UnityEngine;

public class Platform : MonoBehaviour
{
    public float fadeSpeed = 0.5f;
    public float fadeThreshold = 0.3f;
    public Collider platformCol; 
    private float timer;
    private float timerDuration = 10f;
    private Material platformMat;
    private bool isFadingOut = false;

    void Awake()
    {
        platformMat = GetComponent<Renderer>().material;
        timer = timerDuration;
    }

    void Update()
    {
        if (isFadingOut)
        {
            Color color = platformMat.color;
            if (color.a > fadeThreshold)
            {
                color.a -= fadeSpeed * Time.deltaTime;
                platformMat.color = color;
            }
            else
            {
                platformCol.enabled = false;
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    isFadingOut = false;
                    timer = timerDuration;
                }
            }
        }
        else
        {
            platformCol.enabled = true;
            Color color = platformMat.color;
            if (color.a < 1f)
            {
                color.a += fadeSpeed * Time.deltaTime;
                platformMat.color = color;
            }
        }
    }

    public void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            isFadingOut = true;
        }
    }
}
