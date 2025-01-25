using UnityEngine;

public class Bumper : MonoBehaviour
{
    public Animator anim;
    public float bounceFactor = 5f;

    public void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.CompareTag("Player"))
        {
            anim.SetBool("didHit", true);
            Vector3 bounce = (col.gameObject.transform.position - transform.position).normalized;
            col.GetComponent<Rigidbody>().AddForce(bounce * bounceFactor, ForceMode.Impulse);
        }
    }

    public void OnTriggerExit(Collider col)
    {
        if(col.gameObject.CompareTag("Player"))
        {
            anim.SetBool("didHit", false);
        }
    }
}
