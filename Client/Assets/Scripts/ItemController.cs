using System;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    [System.NonSerialized]
    public int ItemId;
    public event Action OnGot;

    void OnTriggerEnter(Collider other)
    {
        OnGot();
    }
}
