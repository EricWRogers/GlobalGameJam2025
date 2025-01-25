using UnityEngine;

public class PickUp : MonoBehaviour
{
    public ParticleSystem bubbles;
    public GameObject bottle;
    public float rotateSpeed = 45f;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
    }

    public void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.CompareTag("Player"))
        {
            bubbles.Play();
            bottle.SetActive(false);
            Destroy(gameObject, 4f);
        }
    }
}
