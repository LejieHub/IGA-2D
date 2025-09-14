using UnityEngine;

public enum HandheldState { Static, FlyingAway, FlyingBack, Floating, BeingTossed }

public class ThrowableHandheld : MonoBehaviour
{
    [Header("Injected at runtime")]
    [SerializeField] Transform player;          // 由外部注入
    [SerializeField] Transform originalPoint;   // 由外部注入

    Rigidbody2D rb;

    [Header("Mode")]
    [SerializeField] bool isUpgraded = false;    // 回旋镖/普通

    [Header("State (readonly)")]
    public HandheldState state = HandheldState.Static;

    // -------- 可调参数（Inspector）--------
    [Header("Shared")]
    [Tooltip("自转角速度（度/帧）")]
    [SerializeField, Range(0f, 50f)] float rotateDegPerTick = 20f;

    [Tooltip("玩家附近判定为已回收的距离")]
    [SerializeField, Range(0.01f, 1f)] float catchDistance = 1f;

    [Header("Boomerang (Upgraded)")]
    [Tooltip("向外飞行的速度（单位/秒）")]
    [SerializeField, Range(0.1f, 30f)] float boomerangFlySpeed = 30f;

    [Tooltip("回程速度（单位/秒）")]
    [SerializeField, Range(0.1f, 30f)] float boomerangReturnSpeed = 30f;

    [Tooltip("离开玩家的最大外延距离，超过后开始折返")]
    [SerializeField, Range(0.1f, 30f)] float boomerangMaxDistance = 8f;

    [Header("Normal Throw (Non-upgraded)")]
    [Tooltip("初始向鼠标方向的水平速度（单位/秒）")]
    [SerializeField, Range(0.1f, 50f)] float normalFlySpeed = 30f;

    [Tooltip("重力加速度（单位/秒²），模拟下坠")]
    [SerializeField, Range(0f, 50f)] float dropAcceleration = 30f;

    [Header("Floating (非升级命中后的漂浮)")]
    [Tooltip("上下抖动的周期（秒）")]
    [SerializeField, Range(0.1f, 5f)] float floatCycle = 0.8f;

    [Tooltip("沿自身up轴抖动的速度（单位/秒）")]
    [SerializeField, Range(0f, 1f)] float floatSpeed = 0.2f;

    // -------- 私有运行时变量 --------
    float tickCounter = 0f;
    float curDropSpeed = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = false;    // 默认拿在手里时不参与碰撞
        }
    }

    // ========== 外部注入 ==========
    public void Setup(Transform playerTransform, Transform handAnchor, bool upgraded)
    {
        player = playerTransform;
        originalPoint = handAnchor;
        isUpgraded = upgraded;
        state = HandheldState.Static;
        curDropSpeed = 0f;

        transform.SetParent(handAnchor, worldPositionStays: false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void SetUpgraded(bool upgraded) => isUpgraded = upgraded;

    // ========== 输入 ==========
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            ToggleMode();

        if (Input.GetMouseButtonDown(0))
        {
            if (isUpgraded)
            {
                if (state == HandheldState.Static)
                    state = HandheldState.FlyingAway;
            }
            else
            {
                if (state == HandheldState.Static)
                {
                    // >>> 新增：脱离 player <<<
                    transform.SetParent(null, true);

                    state = HandheldState.BeingTossed;
                    rb.simulated = true; 
                }
                else if (state == HandheldState.Floating)
                {
                    state = HandheldState.FlyingBack;
                }
            }
        }
    }

    // ========== 运动/状态机 ==========
    void FixedUpdate()
    {
        switch (state)
        {
            case HandheldState.Static:
                StickToHand();
                break;
            case HandheldState.FlyingAway:
                SwingForward();
                break;
            case HandheldState.FlyingBack:
                SwingBackToPlayer();
                break;
            case HandheldState.Floating:
                Vibrate();
                break;
            case HandheldState.BeingTossed:
                FlyAndDrop();
                break;
        }
    }

    void StickToHand()
    {
        if (!originalPoint) return;
        transform.SetParent(originalPoint, worldPositionStays: false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = originalPoint.localRotation;
        curDropSpeed = 0f;
    }

    void SwingForward()
    {
        // 方向：从玩家指向鼠标
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = player.position.z;
        Vector3 dir = (mousePos - player.position);
        dir.z = 0f;
        dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector3.right;

        transform.position += dir * boomerangFlySpeed * Time.fixedDeltaTime;

        // 触发回程的外延半径
        if (Vector3.Distance(player.position, transform.position) > boomerangMaxDistance)
            state = HandheldState.FlyingBack;

        SelfRotate();
    }

    void SwingBackToPlayer()
    {
        Vector3 dir = (player.position - transform.position);
        dir.z = 0f;
        float dist = dir.magnitude;
        if (dist > 0.0001f) dir /= dist;

        transform.position += dir * boomerangReturnSpeed * Time.fixedDeltaTime;

        if (dist < catchDistance)
            state = HandheldState.Static;

        SelfRotate();
    }

    void FlyAndDrop()
    {
        // 水平向鼠标方向飞
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = player.position.z;
        Vector3 dir = (mousePos - player.position);
        dir.z = 0f;
        dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector3.right;

        transform.position += dir * normalFlySpeed * Time.fixedDeltaTime;

        // 伪重力：v = v0 + a * dt
        curDropSpeed += dropAcceleration * Time.fixedDeltaTime;
        transform.position += Vector3.down * curDropSpeed * Time.fixedDeltaTime;
    }

    void Vibrate()
    {
        tickCounter += Time.fixedDeltaTime;
        if (tickCounter > floatCycle) tickCounter = -floatCycle;

        float step = floatSpeed * Time.fixedDeltaTime;
        if (tickCounter > 0f)
            transform.position += transform.up * step;
        else
            transform.position -= transform.up * step;
    }

    void SelfRotate()
    {
        transform.eulerAngles += new Vector3(0, 0, rotateDegPerTick);
    }

    // ========== 碰撞 ==========
    void OnCollisionEnter2D(Collision2D collision)
    {
        curDropSpeed = 0f; // 命中/落地 重置

        if (collision.gameObject.CompareTag("Enemy"))
        {
            state = isUpgraded ? HandheldState.FlyingBack : HandheldState.Floating;
            return;
        }

        if (collision.gameObject.TryGetComponent(out PlayerController _))
        {
            if (state == HandheldState.Floating)
                state = HandheldState.FlyingBack;
            return;
        }

        state = isUpgraded ? HandheldState.FlyingBack : HandheldState.Floating;
        if (!isUpgraded)
        {
            this.enabled = false;
        }
    }

    void ToggleMode()
    {
        isUpgraded = !isUpgraded;
        Debug.Log($"Handheld mode: {(isUpgraded ? "Upgraded (回旋镖)" : "Normal (投掷)")}");
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (player)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(player.position, boomerangMaxDistance);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(player.position, catchDistance);
        }
    }
#endif
}
