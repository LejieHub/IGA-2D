using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float inputHorizontal;
    private Rigidbody2D rb2d => GetComponent<Rigidbody2D>();
    private Animator anim => GetComponent<Animator>();

    [Header("Sprite Bars")]
    public Transform gpaBar;
    public Transform pressureBar;

    private Vector3 gpaBaseScale;
    private Vector3 pressureBaseScale;

    // ====== Player Static Attributes ======
    public enum Personality { Introvert, Extrovert }
    public enum AcademicLevel { Good, Medium, Poor }
    public enum FamilyBackground { Good, Bad }
    public enum Hobby { Game, Music, Sport }

    public enum IQ { Good, Medium, Poor }
    public IQ iq = IQ.Good;

    [Header("Character Attributes")]
    public Personality personality = Personality.Introvert;
    public AcademicLevel academicLevel = AcademicLevel.Medium;
    public FamilyBackground familyBackground = FamilyBackground.Good;
    public Hobby hobby = Hobby.Game;

    [Header("Hurt Settings")]
    [SerializeField] float hurtDuration = 1f;   // 硬直时间
    [SerializeField] float hurtUpForce = 15f;     // 向上弹起力度
    [SerializeField] float hurtBackForce = 8f;   // 横向击退力度
    bool isHurt = false;                           // 是否硬直中




    // ====== GPA & Pressure System ======
    [Header("Status")]
    [Range(0, 4f)] public float GPA = 2.5f;
    [Range(0, 100)] public float pressure = 0f;

    public float GPADecreaseRate = 0.01f;     // 每秒减少
    public float pressureIncreaseRate = 0.05f;

    public float maxGPA = 4.0f;
    public float minGPA = 0f;
    public float maxPressure = 100f;

    // ====== Movement ======
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

    void Start()
    {
        gpaBaseScale = gpaBar.localScale;
        pressureBaseScale = pressureBar.localScale;
    }
    void Update()
    {
        GetInput();
        CheckGround();
        TurnAround();

        // GPA & Pressure Over Time
        GPA -= GPADecreaseRate * Time.deltaTime;
        pressure += pressureIncreaseRate * Time.deltaTime;

        GPA = Mathf.Clamp(GPA, minGPA, maxGPA);
        pressure = Mathf.Clamp(pressure, 0f, maxPressure);
        
        // Failure check
        if (GPA <= 0f)
        {
            Debug.Log("GPA dropped to 0: Dropout ending");
            // TODO: Trigger dropout ending
        }

        if (pressure >= maxPressure)
        {
            Debug.Log("Pressure exploded: Breakdown ending");
            // TODO: Trigger stress breakdown ending
        }

        gpaBar.localScale = new Vector3(gpaBaseScale.x * (GPA / maxGPA), gpaBaseScale.y, gpaBaseScale.z);
        pressureBar.localScale = new Vector3(pressureBaseScale.x * (pressure / maxPressure), pressureBaseScale.y, pressureBaseScale.z);

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
        if (isHurt) return;
        rb2d.velocity = new Vector2(inputHorizontal * moveSpeed, rb2d.velocity.y);
    }

    void Jump()
    {
        if (isHurt) return;
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

    // ====== Public External Triggers ======

    // Called when stomping a Homework or Test
    public void GainGPA(float baseValue)
    {
        float multiplier = academicLevel switch
        {
            AcademicLevel.Good => 1.0f,
            AcademicLevel.Medium => 0.75f,
            AcademicLevel.Poor => 0.5f,
            _ => 1f
        };

        GPA += baseValue * multiplier;
        GPA = Mathf.Clamp(GPA, minGPA, maxGPA);
    }

    public void GainPressure(float amount)
    {
        pressure += amount;
        pressure = Mathf.Clamp(pressure, 0f, maxPressure);
    }

    public void LoseGPA(float amount)
    {
        GPA -= amount;
        GPA = Mathf.Clamp(GPA, minGPA, maxGPA);
    }

    public void TakeDamage(Transform source)
    {
        if (isHurt) return;

        isHurt = true;
        // 受击动画
        anim.SetTrigger("Hurt");

        float dir = Mathf.Sign(transform.position.x - source.position.x);

        rb2d.velocity = Vector2.zero;
        rb2d.velocity = new Vector2(dir * hurtBackForce, hurtUpForce);
        

        // 禁用输入/攻击（如果你有输入脚本，记得在这里关，hurt 结束后再开）
        StartCoroutine(HurtRoutine());
    }

    private System.Collections.IEnumerator HurtRoutine()
    {
        // 硬直期间可根据需要禁止 Move/Jump，这里仅做时间控制
        yield return new WaitForSeconds(hurtDuration);
        isHurt = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyDamage"))
        {
            TakeDamage(other.transform);
        }
    }

}
