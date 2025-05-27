using UnityEngine;

public static class HandControllerFinder
{
    public static HandController Player =>
        GameObject.FindWithTag("Player")?.GetComponent<HandController>();
}
