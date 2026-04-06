using UnityEngine;

/// <summary>
/// On the player: add to a child with a trigger collider. Overlap with stealth enemies kills the player and loads game over.
/// This collider is excluded from <see cref="EnemyMovement"/> chase collision-ignore so contact still registers while chasing.
/// </summary>
[DisallowMultipleComponent]
public sealed class EnemyPlayerTouch : MonoBehaviour
{
    [SerializeField] LayerMask enemyLayers = ~0;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null)
            return;
        if (((1 << other.gameObject.layer) & enemyLayers) == 0)
            return;
        if (other.GetComponentInParent<EnemyAI>() == null
            && other.GetComponentInParent<EnemyPatrol>() == null)
            return;

        PlayerHealth health = PlayerHealth.FindForGameplayTransform(transform);
        health?.DieFromEnemy();
    }
}
