using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BikePartInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [System.Serializable]
    public class BikePart
    {
        public MeshRenderer meshRenderer;
        public int materialIndex;
    }

    public List<BikePart> bikeParts;
    public Color highlightColor = Color.white;
    private Dictionary<BikePart, Color> originalColors = new Dictionary<BikePart, Color>();

    public GameObject colorPickerUI;
    public FlexibleColorPicker colorPicker;

    void Start()
    {
        if (bikeParts == null || bikeParts.Count == 0)
        {
            Debug.LogError("No bike parts assigned.");
            return;
        }

        foreach (var part in bikeParts)
        {
            if (part.meshRenderer != null && part.materialIndex < part.meshRenderer.materials.Length)
            {
                Material material = part.meshRenderer.materials[part.materialIndex];
                originalColors[part] = material.color;
            }
            else
            {
                Debug.LogError("MaterialIndex out of bounds or MeshRenderer missing for one or more parts.");
            }
        }

        if (colorPickerUI != null)
        {
            colorPickerUI.SetActive(false);
        }

        if (colorPicker != null)
        {
            colorPicker.onColorChange.AddListener(OnColorChanged);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        foreach (var part in bikeParts)
        {
            if (part.meshRenderer != null)
            {
                Material material = part.meshRenderer.materials[part.materialIndex];
                if (material != null)
                {
                    material.color = highlightColor;
                }
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        foreach (var part in bikeParts)
        {
            if (part.meshRenderer != null && originalColors.ContainsKey(part))
            {
                Material material = part.meshRenderer.materials[part.materialIndex];
                if (material != null)
                {
                    material.color = originalColors[part];
                }
            }
        }
    }

    public void OnColorChanged(Color newColor)
    {
        foreach (var part in bikeParts)
        {
            if (part.meshRenderer != null)
            {
                Material material = part.meshRenderer.materials[part.materialIndex];
                if (material != null)
                {
                    material.color = newColor;
                }
            }
        }
    }

    public void OnButtonClick()
    {
        if (colorPickerUI != null)
        {
            colorPickerUI.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        if (colorPicker != null)
        {
            colorPicker.onColorChange.RemoveListener(OnColorChanged);
        }
    }
}
