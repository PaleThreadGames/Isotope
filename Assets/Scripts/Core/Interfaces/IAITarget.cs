using UnityEngine;

/// <summary>
/// Optional abstraction for AI focus targets (player, decoys, escort objectives).
/// </summary>
public interface IAITarget
{
    Transform Transform { get; }
}
