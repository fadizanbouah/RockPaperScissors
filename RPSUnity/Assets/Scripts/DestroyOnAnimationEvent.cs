using UnityEngine;

public class DestroyOnAnimationEvent : MonoBehaviour
{
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
