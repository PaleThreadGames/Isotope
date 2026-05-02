using UnityEngine;

/// <summary>
/// Defines a tile-space region that can surface a ScuttleWart from the ground.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class ScuttleWartSpawnZone : MonoBehaviour
{
    [SerializeField]
    GameObject scuttleWartPrefab;

    [SerializeField]
    GameObject spawnVfxPrefab;

    [SerializeField]
    bool spawnOnStart = true;

    [SerializeField]
    bool randomizeWithinZone;

    [SerializeField]
    Vector2 spawnOffset;

    [SerializeField]
    bool disableAfterSpawn = true;

    BoxCollider2D _zone;
    bool _hasSpawned;

    void Awake()
    {
        _zone = GetComponent<BoxCollider2D>();
        _zone.isTrigger = true;
    }

    void Start()
    {
        if (spawnOnStart)
            Spawn();
    }

    public GameObject Spawn()
    {
        if (_hasSpawned || scuttleWartPrefab == null)
            return null;

        _hasSpawned = true;
        Vector3 position = GetSpawnPosition();
        
        if (spawnVfxPrefab != null)
        {
            Instantiate(spawnVfxPrefab, position, Quaternion.identity);
        }

        GameObject spawned = Instantiate(scuttleWartPrefab, position, Quaternion.identity);
        spawned.name = scuttleWartPrefab.name;

        if (disableAfterSpawn)
            _zone.enabled = false;

        return spawned;
    }

    Vector3 GetSpawnPosition()
    {
        Bounds bounds = _zone.bounds;
        Vector3 position = bounds.center;

        if (randomizeWithinZone)
        {
            position.x = Random.Range(bounds.min.x, bounds.max.x);
            position.y = Random.Range(bounds.min.y, bounds.max.y);
        }

        position += (Vector3)spawnOffset;
        position.z = transform.position.z;
        return position;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box == null)
            return;

        Gizmos.color = new Color(0.3f, 0.9f, 0.45f, 0.25f);
        Gizmos.DrawCube(box.bounds.center, box.bounds.size);
        Gizmos.color = new Color(0.3f, 0.9f, 0.45f, 0.9f);
        Gizmos.DrawWireCube(box.bounds.center, box.bounds.size);
    }
#endif
}
