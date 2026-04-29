using UnityEngine;

/// <summary>
/// Registers the player transform with a shared ScriptableObject so enemies do not call Find per instance.
/// </summary>
public class PlayerTargetBinder : MonoBehaviour
{
    [SerializeField]
    PlayerReferenceSO playerReference;

    void OnEnable()
    {
        if (playerReference != null)
            playerReference.Set(transform);
    }

    void OnDisable()
    {
        if (playerReference != null)
            playerReference.Clear(transform);
    }
}
