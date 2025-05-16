using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Collectable : MonoBehaviour
{
    public int Value;
    public string Tag;

    private UnityEvent<Collectable> _listener;

    public void Register(UnityAction<Collectable> call)
    {
        if (_listener == null)
            _listener = new UnityEvent<Collectable>();
        _listener.AddListener(call);
    }

    public void Deregister(UnityAction<Collectable> call)
    {
        _listener?.RemoveListener(call);
    }

    public int Collect()
    {
        _listener?.Invoke(this);
        gameObject.SetActive(false);

        return Value;
    }
    private void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Player") || other.CompareTag("NPC"))
    {
        Collect();
    }
}
}
