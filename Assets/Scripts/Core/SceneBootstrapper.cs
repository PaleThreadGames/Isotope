using UnityEngine;
using System.Linq;

public class SceneBootstrapper : MonoBehaviour
{
    [SerializeField] TransitionDataSO _transitionData;
    [SerializeField] PlayerReferenceSO _playerReference;

    void Start()
    {
        if (_transitionData == null || string.IsNullOrEmpty(_transitionData.targetEntranceID))
        {
            return;
        }

        // Find the entrance
        var entrances = Object.FindObjectsByType<SceneEntrance>(FindObjectsInactive.Include);
var target = entrances.FirstOrDefault(e => e.EntranceID == _transitionData.targetEntranceID);

        if (target != null)
        {
            // Find player
            GameObject player = null;
            if (_playerReference != null && _playerReference.Target != null)
            {
                player = _playerReference.Target.gameObject;
            }
            else
            {
                player = GameObject.FindWithTag("Player");
            }

            if (player != null)
            {
                player.transform.position = target.transform.position;
                
                // Clear velocity if it has a Rigidbody2D
                var rb = player.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                }
            }
            }
        
            // Clear target entrance ID so we don't spawn there again if we reload the scene normally
            _transitionData.targetEntranceID = null;
            }
            }
