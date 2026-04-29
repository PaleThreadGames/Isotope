using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [Header("Settings")]
    public float lifetime = 3f; 

    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D coll;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();

        Invoke("TimeoutDestroy", lifetime);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.CompareTag("SmallEnemy")) return; 
 
        CancelInvoke("TimeoutDestroy");

        rb.linearVelocity = Vector2.zero;
        coll.enabled = false;
        anim.SetTrigger("Hit");

        IDamageable damageable = hitInfo.GetComponent<IDamageable>();
        if (damageable != null)
            damageable.TakeDamage(1f);
    }

    // Called by the Animation Event when the explosion finishes
    public void DestroyProjectile()
    {
        Destroy(gameObject);
    }

    // Called if the timer runs out before hitting anything
    private void TimeoutDestroy()
    {

        Destroy(gameObject);
    }
}