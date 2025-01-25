using UnityEngine;

public class RotatingBaton : MonoBehaviour
{
    public GameObject pole;

    public float rotationSpeed = 50f;

    // Update is called once per frame
    void Update()
    {
        pole.transform.Rotate(0f,0f,rotationSpeed * Time.deltaTime);
    }
}
