using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
public class ScuttleWartDeathPhysics : MonoBehaviour
{
    [Header("Physics Settings")]
    [SerializeField] float deathGravityScale = 1.5f;
    [SerializeField] float rollForce = 12f;
    [SerializeField] float initialPopForce = 3f;
    
    [Header("Settling Settings")]
    [SerializeField] float settleThreshold = 0.1f;
    [SerializeField] float settleTime = 3f;
    [SerializeField] string backgroundLayerName = "Ignore Raycast";

    [Header("Shrink Settings")]
    [SerializeField] float headRadius = 0.3f;
    [SerializeField] Vector2 headOffset = new Vector2(0f, 0.83f);
    [SerializeField] float shrinkDuration = 0.7f;

    [Header("Visual Effects")]
    [SerializeField] GameObject deathVFXPrefab;
    [SerializeField] Color vfxColor = Color.white;
    [SerializeField] float vfxLifetime = 1.5f;

    Rigidbody2D _rb;
    EnemyHealth _health;
    Collider2D _mainCollider;
    CircleCollider2D _rollCollider;
    bool _isDead;
    bool _hasBeenHit;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _health = GetComponent<EnemyHealth>();
        
        _mainCollider = GetComponent<Collider2D>();
        
        _rollCollider = gameObject.AddComponent<CircleCollider2D>();
        if (_mainCollider is BoxCollider2D box)
        {
            _rollCollider.radius = Mathf.Min(box.size.x, box.size.y) * 0.5f;
            _rollCollider.offset = box.offset;
        }
        else
        {
            _rollCollider.radius = 0.4f;
            _rollCollider.offset = Vector2.zero;
        }
        _rollCollider.enabled = false;
    }

    void OnEnable()
    {
        _health.OnDeath += HandleDeath;
    }

    void OnDisable()
    {
        _health.OnDeath -= HandleDeath;
    }

    void HandleDeath()
    {
        if (_isDead) return;
        _isDead = true;

        // Play VFX
        if (deathVFXPrefab != null)
        {
            // Center the VFX on the sprite/collider
            Vector3 spawnPos = transform.position;
            if (_mainCollider != null)
            {
                spawnPos = _mainCollider.bounds.center;
            }
            
            GameObject vfx = Instantiate(deathVFXPrefab, spawnPos, Quaternion.identity);
            
            // Apply configurable color to all particle systems
            var particleSystems = vfx.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                var main = ps.main;
                main.startColor = vfxColor;
            }

            // Ensure VFX is visible and reasonably sized
            vfx.transform.localScale = Vector3.one;
            
            var vfxRenderers = vfx.GetComponentsInChildren<Renderer>();
            var myRenderer = GetComponent<SpriteRenderer>();
            foreach (var r in vfxRenderers)
            {
                if (myRenderer != null)
                {
                    r.sortingLayerID = myRenderer.sortingLayerID;
                    r.sortingOrder = myRenderer.sortingOrder + 10;
                }
            }
            
            Destroy(vfx, vfxLifetime);
        }

        // Configure physics for ragdoll
        _rb.simulated = true;
        _rb.gravityScale = deathGravityScale;
        _rb.constraints = RigidbodyConstraints2D.None;
        _rb.angularDamping = 0.5f;
        _rb.linearDamping = 0.1f;

        // Swap to roll collider
        if (_mainCollider != null) _mainCollider.enabled = false;
        _rollCollider.enabled = true;

        // Start shrinking process
        StartCoroutine(ShrinkCollider());

        // Initial pop
        _rb.AddForce(new Vector2(Random.Range(-1f, 1f), initialPopForce), ForceMode2D.Impulse);
        _rb.AddTorque(Random.Range(-2f, 2f), ForceMode2D.Impulse);
    }

    IEnumerator ShrinkCollider()
    {
        float elapsed = 0f;
        float startRadius = _rollCollider.radius;
        Vector2 startOffset = _rollCollider.offset;

        while (elapsed < shrinkDuration)
        {
            float t = elapsed / shrinkDuration;
            // Use a curve-like feeling (sinusoidal or just linear)
            _rollCollider.radius = Mathf.Lerp(startRadius, headRadius, t);
            _rollCollider.offset = Vector2.Lerp(startOffset, headOffset, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        _rollCollider.radius = headRadius;
        _rollCollider.offset = headOffset;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!_isDead || _hasBeenHit) return;

        // If hit by player after death
        if (collision.gameObject.CompareTag("Player"))
        {
            _hasBeenHit = true;
            
            Vector2 pushDir = (transform.position - collision.transform.position).normalized;
            // Ensure some upward force so it rolls better
            pushDir += Vector2.up * 0.5f;
            pushDir.Normalize();

            _rb.AddForce(pushDir * rollForce, ForceMode2D.Impulse);
            _rb.AddTorque(-pushDir.x * rollForce * 2f, ForceMode2D.Impulse);

            StartCoroutine(BecomeBackgroundAfterSettle());
        }
    }

    IEnumerator BecomeBackgroundAfterSettle()
    {
        float timer = 0f;
        float stillTime = 0f;

        while (timer < settleTime)
        {
            if (_rb.linearVelocity.magnitude < settleThreshold && _rb.angularVelocity < 10f)
            {
                stillTime += Time.deltaTime;
                if (stillTime > 0.75f) break;
            }
            else
            {
                stillTime = 0f;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // Finalize as background object
        int layer = LayerMask.NameToLayer(backgroundLayerName);
        if (layer != -1)
        {
            gameObject.layer = layer;
        }
        
        _rb.bodyType = RigidbodyType2D.Static;
        // Keep the collider enabled if you want it to still be part of the ground, 
        // but since we changed layer to Ignore Raycast, it won't be hit by player.
        // If we want it to be "non-interactable" including physics, we could disable the collider too.
        // User said "background object that cant be interacted with".
        _rollCollider.enabled = false; 
    }
}