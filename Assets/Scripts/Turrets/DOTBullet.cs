using UnityEngine;

public class DOTBullet : Bullet
{
    private float DOTInterval;

    public void setDOTInterval(float interval)
    {
        DOTInterval = interval;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        TDEnemyProperties objectHitScript = other.gameObject.GetComponent<TDEnemyProperties>();
        objectHitScript.SufferDOT(bulletDamage, DOTInterval);
        Destroy(gameObject);
    }
}
