using UnityEngine;

public class SceneEntrance : MonoBehaviour
{
    [SerializeField] string _entranceID;
    public string EntranceID => _entranceID;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawIcon(transform.position + Vector3.up * 0.5f, "MoveTool On", true);
    }
}
