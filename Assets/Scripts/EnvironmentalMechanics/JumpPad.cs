using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public Animator anim;
    public float springForce = 10f;
    
    public void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.CompareTag("Player"))
        {
            anim.SetBool("didHit", true);
            col.GetComponent<Rigidbody>().AddForce(transform.forward * (col.GetComponent<Rigidbody>().linearVelocity.magnitude + springForce), ForceMode.Impulse);
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
