using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class BikeData
{
    public CustomColor[] partCustomizations;


    [Serializable]
    public class CustomColor
    {
        public string Name;
        [FormerlySerializedAs("Material")] public int MaterialIndex;
        public Color Color;
    }
}