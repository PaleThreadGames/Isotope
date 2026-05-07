using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
public class SceneTransition : MonoBehaviour
{
    [SerializeField] string _targetSceneName;
    [SerializeField] string _targetEntranceID;
    [SerializeField] TransitionDataSO _transitionData;

    bool _canTransition = true;

    void Awake()
    {
        var col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
    }

    void Start()
    {
        // If we just arrived at this door, don't allow transitioning back until we step away
        var entrance = GetComponent<SceneEntrance>();
        if (entrance != null && _transitionData != null && _transitionData.targetEntranceID == entrance.EntranceID)
        {
            _canTransition = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!_canTransition) return;

        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            if (_transitionData != null)
            {
                _transitionData.targetEntranceID = _targetEntranceID;
            }
            SceneManager.LoadScene(_targetSceneName);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            _canTransition = true;
        }
    }
}
