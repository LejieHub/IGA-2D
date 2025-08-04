using DG.Tweening;
using UnityEngine;

public class WormMovement : MonoBehaviour 
{
    void Start()
    {
        // 获取物体的 Transform
        Transform spriteTransform = transform;

        // 创建一个序列来组合动画
        Sequence wriggleSequence = DOTween.Sequence();

        // 添加位置抖动（模拟蠕动）
        wriggleSequence.Append(spriteTransform.DOShakePosition(
            duration: 0.5f, // 每次抖动持续时间
            strength: new Vector3(0.1f, 0.1f, 0), // 抖动幅度
            vibrato: 10, // 抖动频率
            randomness: 90, // 随机性
            snapping: false,
            fadeOut: true
        ));

        // 添加缩放动画（模拟身体伸缩）
        wriggleSequence.Join(spriteTransform.DOScale(
            new Vector3(1.1f, 0.9f, 1f), // 轻微拉伸 X 轴，压缩 Y 轴
            0.25f
        ).SetEase(Ease.InOutSine));

        wriggleSequence.Join(spriteTransform.DOScale(
            new Vector3(0.9f, 1.1f, 1f), // 反向拉伸
            0.25f
        ).SetDelay(0.25f).SetEase(Ease.InOutSine));

        // 设置循环播放，模拟持续蠕动
        wriggleSequence.SetLoops(-1, LoopType.Yoyo);
    }
}