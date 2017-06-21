using UnityEngine;

public class ItemController : MonoBehaviour
{
    [System.NonSerialized]
    public int ItemId;

    void OnTriggerEnter(Collider other)
    {
        Debug.LogError("OnTriggerEnter");
    }
}
