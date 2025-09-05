using UnityEngine;

public enum HandheldState { Static, FlyingAway, FlyingBack, Floating, BeingTossed }

public class ThrowableHandheld : MonoBehaviour
{
    [Header("Injected at runtime")]
    [SerializeField] Transform player;         // 玩家 Transform（由 PlayerInventory 注入）
    [SerializeField] Transform originalPoint;  // 手持锚点 HandAnchor（由 PlayerInventory 注入）

    [Header("Mode")]
    [SerializeField] bool isUpgraded = true;   // 升级模式（回旋镖式）/ 非升级（抛掷+漂浮）

    [Header("State (readonly)")]
    public HandheldState state = HandheldState.Static;

    // —— 与“Baby”保持一致的参数（不变） ——
    float tickCounter = 0f;
    float curDropSpeed = 0f;  // BeingTossed 时的“下落速度累积”（伪重力）

    // ====== API：供外部注入（PlayerInventory.Equip时调用） ======
    public void Setup(Transform playerTransform, Transform handAnchor, bool upgraded)
    {
        player = playerTransform;
        originalPoint = handAnchor;
        isUpgraded = upgraded;
        state = HandheldState.Static;
        curDropSpeed = 0f;

        // 初始就贴到手
        transform.SetParent(handAnchor, worldPositionStays: false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    // 可选：外部切换升级状态（比如某个BUFF）
    public void SetUpgraded(bool upgraded) => isUpgraded = upgraded;

    // ================== 输入（按你需求：左键） ==================
    void Update()
    {
        // 按 R 切换
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
                    state = HandheldState.BeingTossed;
                else if (state == HandheldState.Floating)
                    state = HandheldState.FlyingBack;
            }
        }
    }

    // ================== 运动/状态机 ==================
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
        if (originalPoint == null) return;
        transform.SetParent(originalPoint, worldPositionStays: false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = originalPoint.localRotation;
        curDropSpeed = 0f;
    }

    void SwingForward()
    {
        float flySpeed = 1f;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 dir = (mousePos - player.position);
        dir.z = 0;
        dir = dir.normalized;

        transform.position += dir * flySpeed;
        if (Vector3.Distance(player.position, transform.position) > 2f)
            state = HandheldState.FlyingBack;

        SelfRotate();
    }

    void SwingBackToPlayer()
    {
        float flySpeed = 0.2f;
        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += dir * flySpeed;

        if (Vector3.Distance(player.position, transform.position) < 0.2f)
            state = HandheldState.Static;

        SelfRotate();
    }

    void FlyAndDrop()
    {
        float flySpeed = 0.13f;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 dir = (mousePos - player.position);
        dir.z = 0;
        dir = dir.normalized;

        // 向鼠标方向飞
        transform.position += dir * flySpeed;

        // 伪重力：逐帧增加下落速度
        float dropAcceleration = 0.005f;
        curDropSpeed += dropAcceleration;
        transform.position += Vector3.down * curDropSpeed;
    }

    void Vibrate()
    {
        float timeCycle = 0.8f;
        float flySpeed = 0.002f;

        tickCounter += Time.deltaTime;
        if (tickCounter > timeCycle) tickCounter = -timeCycle;

        if (tickCounter > 0f)
            transform.position += transform.up * flySpeed;
        else
            transform.position -= transform.up * flySpeed;
    }

    void SelfRotate()
    {
        transform.eulerAngles += new Vector3(0, 0, 20);
    }

    // ================== 碰撞处理 ==================
    void OnCollisionEnter2D(Collision2D collision)
    {
        curDropSpeed = 0f; // 落地/命中重置下落速度

        // 1) 敌人：只判断 Tag 或 Layer，不做伤害
        if (collision.gameObject.CompareTag("Enemy"))
        {
            state = isUpgraded ? HandheldState.FlyingBack : HandheldState.Floating;
            return;
        }

        // 撞到玩家：若在漂浮则回收
        if (collision.gameObject.TryGetComponent(out PlayerController _))
        {
            if (state == HandheldState.Floating)
                state = HandheldState.FlyingBack;
            return;
        }

        // 其他：升级回收，非升级漂浮
        state = isUpgraded ? HandheldState.FlyingBack : HandheldState.Floating;
    }

    void ToggleMode()
    {
        isUpgraded = !isUpgraded;
        Debug.Log($"Handheld mode: {(isUpgraded ? "Upgraded (回旋镖)" : "Normal (投掷)")}");
    }
}
