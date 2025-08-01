using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float inputHorizontal;
    private Rigidbody2D rb2d => GetComponent<Rigidbody2D>();
    private Animator anim => GetComponent<Animator>();

    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Jumping")]
    public float jumpForce = 10f;
    private bool canJump = false;
    private bool canJumpTwice = false;

    [Header("Ground Check")]
    public bool onGround;
    public Transform groundCheckPoint;
    public Vector2 checkBoxSize = new Vector2(0.4f, 0.1f);
    public LayerMask groundLayer;

    void Update()
    {
        GetInput();
        CheckGround();
        
        TurnAround();
    }

    private void FixedUpdate()
    {
        Move();
        Jump();
        UpdateAnimation();
    }

    void GetInput()
    {
        inputHorizontal = Input.GetAxis("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (onGround)
            {
                canJump = true;
            }
            else if (canJumpTwice)
            {
                canJump = true;
                canJumpTwice = false;
            }
        }
    }

    void Move()
    {
        rb2d.velocity = new Vector2(inputHorizontal * moveSpeed, rb2d.velocity.y);
    }

    void Jump()
    {
        if (canJump)
        {
            rb2d.velocity = Vector2.up * jumpForce;
            canJump = false;
        }
    }

    void CheckGround()
    {
        Collider2D collider = Physics2D.OverlapBox(groundCheckPoint.position, checkBoxSize, 0f, groundLayer);
        if (collider != null)
        {
            onGround = true;
            canJumpTwice = true;
        }
        else
        {
            onGround = false;
        }
    }

    void UpdateAnimation()
    {
        anim.SetBool("OnGround", onGround);
        anim.SetFloat("SpeedX", Mathf.Abs(rb2d.velocity.x));
        anim.SetFloat("SpeedY", rb2d.velocity.y);
    }

    void TurnAround()
    {
        if (rb2d.velocity.x < -0.01f)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (rb2d.velocity.x > 0.01f)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(groundCheckPoint.position, checkBoxSize);
        }
    }
}
