using UnityEngine;

public class StompTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyPatrol enemy = other.GetComponent<EnemyPatrol>();
            if (enemy != null)
            {
                enemy.OnStomped();

                // 向上传播事件给 Player：反弹
                Rigidbody2D playerRb = GetComponentInParent<Rigidbody2D>();
                if (playerRb != null)
                {
                    playerRb.velocity = new Vector2(playerRb.velocity.x, 8f); // 反弹力度自定义
                }
            }
        }
    }
}
