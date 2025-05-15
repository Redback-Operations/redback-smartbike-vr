using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NetworkObjectLocal : MonoBehaviour
{
    public Guid Guid;
    public string Type;

    private Rigidbody _rigidbody;
    private bool _hasMoved;

    private ObjectProperties _properties;

    void Start()
    {
        if (Guid == Guid.Empty)
            Guid = Guid.NewGuid();

        _rigidbody = GetComponent<Rigidbody>();
        _hasMoved = false;

        _properties = new ObjectProperties(Guid, Type, transform.position, transform.eulerAngles);
    }

    void Update()
    {
        if (Mathf.Approximately(_rigidbody.velocity.sqrMagnitude, 0))
            return;

        _hasMoved = true;

        _properties.Position = transform.position;
        _properties.Rotation = transform.eulerAngles;
    }

    public ObjectProperties Properties()
    {
        return _properties;
    }

    public bool HasMoved()
    {
        if (!_hasMoved)
            return false;
        _hasMoved = false;
        return true;
    }
}
