using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deadline : MonoBehaviour
{
    public enum MoveMode { Continuous, StepTriggered }

    [Header("Target")]
    [SerializeField] Transform player;

    [Header("Mode")]
    [SerializeField] MoveMode mode = MoveMode.Continuous;

    [Header("Continuous Move")]
    [SerializeField, Min(0f)] float moveSpeed = 2f;      // 连续模式的水平速度
    [SerializeField] bool faceToPlayer = true;           // 是否朝向玩家翻转

    [Header("Step Move")]
    [SerializeField, Min(0f)] float stepDistance = 0.6f; // 每次“蠕动”前进的距离
    [SerializeField, Min(0.05f)] float stepDuration = 0.2f; // 单步用时（秒）

    [Header("Grounding")]
    [SerializeField] bool lockHorizontalOnly = true;     // 仅修改水平速度，不改竖直速度（保留重力/落地）

    Rigidbody2D rb;
    bool isStepping = false;
    Vector2 stepStartPos, stepEndPos;
    float stepTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (!player) return;

        if (mode == MoveMode.Continuous)
        {
            // 只改水平速度，竖直方向交给重力和碰撞
            float dirX = Mathf.Sign(player.position.x - transform.position.x);
            float vx = dirX * moveSpeed;

            if (lockHorizontalOnly)
                rb.velocity = new Vector2(vx, rb.velocity.y);
            else
                rb.velocity = new Vector2(vx, 0f);

            if (faceToPlayer && Mathf.Abs(vx) > 0.01f)
                Face(dirX);
        }
        else // StepTriggered
        {
            if (isStepping)
            {
                stepTimer += Time.fixedDeltaTime;
                float t = Mathf.Clamp01(stepTimer / stepDuration);

                // 用 MovePosition 平滑推进，保留 rb 的竖直速度（重力）
                Vector2 cur = Vector2.Lerp(stepStartPos, stepEndPos, t);
                rb.MovePosition(new Vector2(cur.x, rb.position.y));

                if (t >= 1f) isStepping = false;
            }
            else
            {
                // 步进闲置时保持水平速度为0（仅受重力/地面摩擦）
                rb.velocity = new Vector2(0f, rb.velocity.y);
            }

            if (faceToPlayer)
            {
                float dirX = Mathf.Sign(player.position.x - transform.position.x);
                if (Mathf.Abs(dirX) > 0.01f) Face(dirX);
            }
        }
    }

    void Face(float dirX)
    {
        // 仅水平翻转，不影响子物体的动画
        var s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (dirX >= 0 ? 1f : -1f);
        transform.localScale = s;
    }

    /// <summary>
    /// 步进式：由子物体动画在关键时刻调用，触发向玩家方向前挤一步
    /// </summary>
    public void RequestStep()
    {
        if (mode != MoveMode.StepTriggered || !player) return;

        float dirX = Mathf.Sign(player.position.x - transform.position.x);
        stepStartPos = rb.position;
        stepEndPos = stepStartPos + new Vector2(dirX * stepDistance, 0f);
        stepTimer = 0f;
        isStepping = true;
    }

    // 可在外部（例如出生/切换目标）设置追踪对象
    public void SetTarget(Transform t) => player = t;
}
