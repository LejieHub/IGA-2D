using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 1.5f;

    [Header("Idle Settings")]
    public float idleDurationMin = 1f;
    public float idleDurationMax = 3f;

    [Header("Environment Check")]
    public Transform groundCheck;
    public Transform wallCheck;
    public float checkDistance = 0.2f;
    public LayerMask groundLayer;


    private float currentIdleDuration;

    private Rigidbody2D rb;
    private Animator anim;

    private bool movingRight = true;
    private bool isIdle = false;
    private float idleTimer = 0f;

    private bool isStomped = false;

    public float popUpForce = 4f;  // 弹起力度
    public float fallDelay = 0.5f; // 弹起后多久开始下落
    public float stompGravityScale = 3f;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        Flip();
    }

    void Update()
    {
        if (isStomped) return;

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

        // 如果检测到墙壁或前方无地面，则进入 idle
        bool hitWall = Physics2D.Raycast(wallCheck.position, movingRight ? Vector2.right : Vector2.left, checkDistance, groundLayer);
        bool noGround = !Physics2D.Raycast(groundCheck.position, Vector2.down, checkDistance, groundLayer);

        if (hitWall || noGround)
        {
            EnterIdle();
            return;
        }

        Patrol();
    }

    void Patrol()
    {
        float direction = movingRight ? 1f : -1f;
        rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);
        SetAnimation(true);
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


    public void OnStomped()
    {
        if (isStomped) return;
        isStomped = true;

        // 停止一切行为
        rb.velocity = Vector2.zero;
        anim.SetBool("isWalking", false);
        anim.SetTrigger("Stomped"); // 可选动画

        // 禁用碰撞体（防止再次被踩或干扰）
        Collider2D col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        // 弹起
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(0, popUpForce);
        StartCoroutine(DoFall());
    }

    private System.Collections.IEnumerator DoFall()
    {
        yield return new WaitForSeconds(fallDelay);
        rb.gravityScale = stompGravityScale;
        yield return new WaitForSeconds(2f); // 等待掉出视野
        Destroy(gameObject); // 销毁
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * checkDistance);

        if (wallCheck != null)
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + (movingRight ? Vector3.right : Vector3.left) * checkDistance);
    }
}
