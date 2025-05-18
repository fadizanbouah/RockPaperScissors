using UnityEngine;

public class FanLayout : MonoBehaviour
{
    [SerializeField] private float angleRange = 30f;
    [SerializeField] private float spacing = 30f; // Horizontal distance between cards

    private void Start()
    {
        ApplyFanLayout();
    }

    public void ApplyFanLayout()
    {
        int childCount = transform.childCount;
        if (childCount == 0) return;

        if (childCount == 1)
        {
            Transform card = transform.GetChild(0);
            card.localPosition = Vector2.zero;
            card.localRotation = Quaternion.identity;
            return;
        }

        float angleStep = angleRange / (childCount - 1);
        float startAngle = -angleRange / 2f;
        float middleIndex = (childCount - 1) / 2f;

        for (int i = 0; i < childCount; i++)
        {
            Transform card = transform.GetChild(i);

            float angle = startAngle + angleStep * i;
            float xOffset = spacing * (i - middleIndex);

            card.localPosition = new Vector2(xOffset, 0);
            card.localRotation = Quaternion.Euler(0f, 0f, -angle);
        }
    }
}
