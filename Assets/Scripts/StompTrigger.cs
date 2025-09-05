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

                // ���ϴ����¼��� Player������
                Rigidbody2D playerRb = GetComponentInParent<Rigidbody2D>();
                if (playerRb != null)
                {
                    playerRb.velocity = new Vector2(playerRb.velocity.x, 8f); // ���������Զ���
                }
            }
        }
    }
}
