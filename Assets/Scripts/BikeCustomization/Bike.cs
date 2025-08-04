﻿using System;
using System.Linq;
using UnityEngine;

public class Bike : MonoBehaviour
{
    [SerializeField] private PartData[] parts;
    [SerializeField] private IkTarget[] ikTargets;
    [SerializeField] private bool generateSprayTargets = true;

    public WheelCollider frontWheelCollider;
    public WheelCollider rearWheelCollider;
    
    public Transform frontWheelTransform;
    public Transform rearWheelTransform;
    public Transform frontHandlePivot;
    public Transform pedalTransform;
    
    public Transform mountTf;
    public IkTarget[] IkTargets => ikTargets;
    public PartData[] PartDatas => parts;
    [HideInInspector] public BikeData bikeData;
    public Action<BikeData> OnBikeDataChange;

    public BikeData ToBikeData()
    {
        return new BikeData
        {
            partCustomizations = parts.Select(part => new BikeData.CustomColor
            {
                Name = part.name,
                Color = part.renderer.materials[part.materialIndex].color,
                MaterialIndex = part.materialIndex
            }).ToArray()
        };
    }

    public Color? GetPartColor(string partName)
    {
        foreach (var part in bikeData.partCustomizations)
        {
            if (part.Name == partName)
            {
                return part.Color;
            }
        }
        return null;
    }

    public void SetPartColor(string partName, Color color, bool save = false)
    {
        foreach (var part in parts)
        {
            if (part.name == partName)
            {
                SetPartColor(part.renderer, part.materialIndex, color, save);
            }
        }
    }

    public void SetPartColor(Renderer renderer, int materialIndex, Color color, bool save = false)
    {
        renderer.materials[materialIndex].color = color;
        if (save)
        {
            OnBikeDataChange?.Invoke(ToBikeData());
            bikeData = ToBikeData();
        }
    }

    private void Start()
    {
        if (generateSprayTargets)
            SetupSprayTargets();
    }

    public void SetupSprayTargets()
    {
        var sprayTargetParent = new GameObject("Spray Targets");
        sprayTargetParent.transform.parent = transform;
        foreach (var part in parts)
        {
            var partBounds = part.renderer.GetComponent<MeshFilter>().mesh.GetSubMesh(part.materialIndex).bounds;
            var sprayTarget = new GameObject($"SprayTarget: {part.name}").AddComponent<SprayTarget>();
            sprayTarget.transform.parent = sprayTargetParent.transform;
            sprayTarget.Target = part.renderer;
            sprayTarget.MaterialIndex = part.materialIndex;
            sprayTarget.bike = this;

            var collider = sprayTarget.gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.center = part.renderer.transform.TransformPoint(partBounds.center);
            collider.size = part.renderer.transform.TransformVector(partBounds.size);
        }
    }

    public void LoadBikeData(BikeData bikeData)
    {
        if (bikeData == null)
        {
            bikeData = ToBikeData();
        }

        this.bikeData = bikeData;
        foreach (var part in bikeData.partCustomizations)
        {
            SetPartColor(part.Name, part.Color);
        }
    }

    [Serializable]
    public class PartData
    {
        public string name;
        public Renderer renderer;
        public int materialIndex;
    }

    [Serializable]
    public class IkTarget
    {
        public AvatarIKGoal target;
        public Transform tf;

        [Range(0, 1)] public float positionWeight;
        [Range(0, 1)] public float rotationWeight;
    }
}