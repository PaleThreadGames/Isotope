using UnityEngine;

// Notice we call it EnemyBase, and it inherits from MonoBehaviour
public class EnemyBase : MonoBehaviour 
{
    // All shared variables go here (Health, Speed, Vision, Rigidbody, etc.)
    [Header("Base Stats")]
    public float moveSpeed = 2f;
    public int maxHealth = 10;
    
    protected Rigidbody2D rb;
    protected Animator anim;
    protected Transform player;

    protected virtual void Start() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    protected virtual void FixedUpdate() {

    }

    protected virtual void Patrol() {
    }

 
    protected virtual void Attack() {
        Debug.Log("Base enemy attack!");
    }

    protected void Flip() {

    }
}