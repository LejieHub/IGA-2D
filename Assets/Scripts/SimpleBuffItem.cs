using UnityEngine;

public class SimpleBuffItem : MonoBehaviour
{
    public enum BuffType { AddGPA, AddPressure }
    public BuffType type;
    public float value = 10f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                if (type == BuffType.AddGPA)
                    player.GainGPA(value);
                else if (type == BuffType.AddPressure)
                    player.GainPressure(value);
            }
            Destroy(gameObject); // ´¥ÅöºóÏûÊ§
        }
    }
}
