using System;
//using ExitGames.Client.Photon;
using UnityEngine;

public class ObjectProperties
{
    private Guid _guid;
    private string _type;

    public Vector3 Position;
    public Vector3 Rotation;

    public ObjectProperties(Guid guid, string type, Vector3 position, Vector3 rotation)
    {
        _guid = guid;
        _type = type;

        Position = position;
        Rotation = rotation;
    }
    /*
    public void Deserialize(Hashtable hashtable)
    {
        if (hashtable.ContainsKey($"{_guid}.Type"))
            _type = (string)hashtable[$"{_guid}.Type"];

        if (hashtable.ContainsKey($"{_guid}.Position"))
            Position = (Vector3)hashtable[$"{_guid}.Position"];

        if (hashtable.ContainsKey($"{_guid}.Rotation"))
            Rotation = (Vector3)hashtable[$"{_guid}.Rotation"];
    }
    public Hashtable Serialize()
    {
        var hashtable = new Hashtable
        {
            { $"{_guid}.Position", Position },
            { $"{_guid}.Rotation", Rotation },
            { $"{_guid}.Type", _type }
        };

        return hashtable;
    }*/
}
