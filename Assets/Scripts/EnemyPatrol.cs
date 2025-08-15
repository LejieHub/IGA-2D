using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Movement Settings")]
    public float leftRange = 2f;
    public float rightRange = 4f;
    public float moveSpeed = 1.5f;

    [Header("Idle Settings")]
    public float idleDurationMin = 1f;
    public float idleDurationMax = 3f;

    private float currentIdleDuration;

    private Rigidbody2D rb;
    private Animator anim;

    private Vector3 startPoint;
    private Vector3 leftBound;
    private Vector3 rightBound;

    private bool movingRight = true;
    private bool isIdle = false;
    private float idleTimer = 0f;

    private bool isStomped = false;

    public float popUpForce = 4f;  // 弹起力度
    public float fallDelay = 0.5f; // 弹起后多久开始下落
    public float fallSpeed = 10f;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        startPoint = transform.position;
        leftBound = startPoint - Vector3.right * leftRange;
        rightBound = startPoint + Vector3.right * rightRange;

        Flip();
    }

    void Update()
    {
        if (isIdle)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= currentIdleDuration)
            {
                isIdle = false;
                idleTimer = 0f;
                movingRight = !movingRight;
                Flip();
            }
            else
            {
                SetAnimation(false);
                rb.velocity = new Vector2(0, rb.velocity.y);
                return;
            }
        }

        Patrol();
    }

    void Patrol()
    {
        float direction = movingRight ? 1f : -1f;
        rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);
        SetAnimation(true);

        if (movingRight && transform.position.x >= rightBound.x)
            EnterIdle();
        else if (!movingRight && transform.position.x <= leftBound.x)
            EnterIdle();
    }

    void EnterIdle()
    {
        isIdle = true;
        idleTimer = 0f;
        currentIdleDuration = Random.Range(idleDurationMin, idleDurationMax);
        rb.velocity = new Vector2(0, rb.velocity.y);
    }

    void SetAnimation(bool isWalking)
    {
        anim.SetBool("isWalking", isWalking);
    }

    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (movingRight ? -1 : 1);
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position - Vector3.right * leftRange, transform.position + Vector3.right * rightRange);
    }

    public void OnStomped()
    {
        if (isStomped) return;
        isStomped = true;

        // 停止一切行为
        rb.velocity = Vector2.zero;
        rb.gravityScale = 1f;
        anim.SetBool("isWalking", false);
        anim.SetTrigger("Stomped"); // 可选动画

        // 禁用碰撞体（防止再次被踩或干扰）
        Collider2D col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        // 弹起
        rb.velocity = new Vector2(0, popUpForce);
        StartCoroutine(FallAfterDelay());
    }

    private System.Collections.IEnumerator FallAfterDelay()
    {
        yield return new WaitForSeconds(fallDelay);
        rb.velocity = new Vector2(0, -fallSpeed);
        yield return new WaitForSeconds(2f); // 等待掉出视野
        Destroy(gameObject); // 销毁
    }
}
