using UnityEngine;

public class FanLayout : MonoBehaviour
{
    [SerializeField] private float radius = 200f;
    [SerializeField] private float angleRange = 30f;
    [SerializeField] private float spacing = 1f; // New: spread multiplier (1 = normal, <1 = tighter, >1 = wider)

    private void Start()
    {
        ApplyFanLayout();
    }

    public void ApplyFanLayout()
    {
        int childCount = transform.childCount;
        if (childCount == 0) return;

        float angleStep = angleRange / (childCount - 1);
        float startAngle = -angleRange / 2f;

        for (int i = 0; i < childCount; i++)
        {
            Transform card = transform.GetChild(i);
            float angle = startAngle + angleStep * i;
            float rad = angle * Mathf.Deg2Rad;

            Vector2 offset = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)) * radius * spacing;

            card.localPosition = offset;
            card.localRotation = Quaternion.Euler(0f, 0f, -angle);
        }
    }
}
