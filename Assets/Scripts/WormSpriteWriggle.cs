using DG.Tweening;
using UnityEngine;

public class WormSpriteWriggle : MonoBehaviour
{
    [SerializeField] float scaleAmount = 0.2f;
    [SerializeField] float halfCycle = 0.3f; // 半个周期

    Deadline mover;

    void Start()
    {
        mover = GetComponentInParent<Deadline>();

        var seq = DOTween.Sequence();

        // 伸：横向拉长、纵向压缩
        seq.Append(transform.DOScale(new Vector3(1f + scaleAmount, 1f - scaleAmount, 1f), halfCycle)
                     .SetEase(Ease.InOutSine));

        // 缩：横向收缩、纵向回弹 —— 在回弹末尾触发一步推进
        seq.Append(transform.DOScale(new Vector3(1f, 1f, 1f), halfCycle)
                     .SetEase(Ease.InOutSine)
                     .OnComplete(() => mover?.RequestStep()));

        seq.SetLoops(-1);
    }
}
