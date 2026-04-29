using UnityEngine;

[CreateAssetMenu(fileName = "PlayerReference", menuName = "Isotope/Player Reference")]
public class PlayerReferenceSO : ScriptableObject
{
    Transform _target;

    public Transform Target => _target;

    public void Set(Transform t)
    {
        _target = t;
    }

    public void Clear(Transform t)
    {
        if (_target == t)
            _target = null;
    }
}
