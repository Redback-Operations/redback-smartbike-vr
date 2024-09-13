using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BikePartInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public MeshRenderer meshRenderer; 
    public int materialIndex;
    public Color highlightColor = Color.white; 
    private Color originalColor;

    private Material _material;

    public GameObject colorPickerUI;
    public FlexibleColorPicker colorPicker;

    void Start()
    {
        if (meshRenderer  == null)
        {
            Debug.LogError("MeshRenderer  is not assigned.");
            return;
        }

        if (materialIndex < meshRenderer.materials.Length)
        {
             _material = meshRenderer.materials[materialIndex];
            originalColor = _material.color;
        }
        else
        {
            Debug.LogError("MaterialIndex out of bounds.");
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
        if (_material != null)
        {
            _material.color = highlightColor;
        }
    }

     public void OnPointerExit(PointerEventData eventData)
    {
        if (_material != null)
        {
            _material.color = originalColor;
        }
    }



     public void OnColorChanged(Color newColor)
    {
        if (_material != null)
        {
            _material.color = newColor;
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
