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
    [SerializeField, Min(0f)] float moveSpeed = 2f;      // ����ģʽ��ˮƽ�ٶ�
    [SerializeField] bool faceToPlayer = true;           // �Ƿ�����ҷ�ת

    [Header("Step Move")]
    [SerializeField, Min(0f)] float stepDistance = 0.6f; // ÿ�Ρ��䶯��ǰ���ľ���
    [SerializeField, Min(0.05f)] float stepDuration = 0.2f; // ������ʱ���룩

    [Header("Grounding")]
    [SerializeField] bool lockHorizontalOnly = true;     // ���޸�ˮƽ�ٶȣ�������ֱ�ٶȣ���������/��أ�

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
            // ֻ��ˮƽ�ٶȣ���ֱ���򽻸���������ײ
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

                // �� MovePosition ƽ���ƽ������� rb ����ֱ�ٶȣ�������
                Vector2 cur = Vector2.Lerp(stepStartPos, stepEndPos, t);
                rb.MovePosition(new Vector2(cur.x, rb.position.y));

                if (t >= 1f) isStepping = false;
            }
            else
            {
                // ��������ʱ����ˮƽ�ٶ�Ϊ0����������/����Ħ����
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
        // ��ˮƽ��ת����Ӱ��������Ķ���
        var s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (dirX >= 0 ? 1f : -1f);
        transform.localScale = s;
    }

    /// <summary>
    /// ����ʽ���������嶯���ڹؼ�ʱ�̵��ã���������ҷ���ǰ��һ��
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

    // �����ⲿ���������/�л�Ŀ�꣩����׷�ٶ���
    public void SetTarget(Transform t) => player = t;
}
