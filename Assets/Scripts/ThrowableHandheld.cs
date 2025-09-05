using UnityEngine;

public enum HandheldState { Static, FlyingAway, FlyingBack, Floating, BeingTossed }

public class ThrowableHandheld : MonoBehaviour
{
    [Header("Injected at runtime")]
    [SerializeField] Transform player;         // ��� Transform���� PlayerInventory ע�룩
    [SerializeField] Transform originalPoint;  // �ֳ�ê�� HandAnchor���� PlayerInventory ע�룩

    [Header("Mode")]
    [SerializeField] bool isUpgraded = true;   // ����ģʽ��������ʽ��/ ������������+Ư����

    [Header("State (readonly)")]
    public HandheldState state = HandheldState.Static;

    // ���� �롰Baby������һ�µĲ��������䣩 ����
    float tickCounter = 0f;
    float curDropSpeed = 0f;  // BeingTossed ʱ�ġ������ٶ��ۻ�����α������

    // ====== API�����ⲿע�루PlayerInventory.Equipʱ���ã� ======
    public void Setup(Transform playerTransform, Transform handAnchor, bool upgraded)
    {
        player = playerTransform;
        originalPoint = handAnchor;
        isUpgraded = upgraded;
        state = HandheldState.Static;
        curDropSpeed = 0f;

        // ��ʼ��������
        transform.SetParent(handAnchor, worldPositionStays: false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    // ��ѡ���ⲿ�л�����״̬������ĳ��BUFF��
    public void SetUpgraded(bool upgraded) => isUpgraded = upgraded;

    // ================== ���루������������� ==================
    void Update()
    {
        // �� R �л�
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

    // ================== �˶�/״̬�� ==================
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

        // ����귽���
        transform.position += dir * flySpeed;

        // α��������֡���������ٶ�
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

    // ================== ��ײ���� ==================
    void OnCollisionEnter2D(Collision2D collision)
    {
        curDropSpeed = 0f; // ���/�������������ٶ�

        // 1) ���ˣ�ֻ�ж� Tag �� Layer�������˺�
        if (collision.gameObject.CompareTag("Enemy"))
        {
            state = isUpgraded ? HandheldState.FlyingBack : HandheldState.Floating;
            return;
        }

        // ײ����ң�����Ư�������
        if (collision.gameObject.TryGetComponent(out PlayerController _))
        {
            if (state == HandheldState.Floating)
                state = HandheldState.FlyingBack;
            return;
        }

        // �������������գ�������Ư��
        state = isUpgraded ? HandheldState.FlyingBack : HandheldState.Floating;
    }

    void ToggleMode()
    {
        isUpgraded = !isUpgraded;
        Debug.Log($"Handheld mode: {(isUpgraded ? "Upgraded (������)" : "Normal (Ͷ��)")}");
    }
}
