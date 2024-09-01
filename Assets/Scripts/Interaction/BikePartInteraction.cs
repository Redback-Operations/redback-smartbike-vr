using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BikePartInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
public SkinnedMeshRenderer SkinnedMeshRenderer; 
    public int MaterialIndex; 
    public Color HighlightColor = Color.white; 
    private Color originalColor; 

    private Material _material;

    public GameObject colorPickerUI; 
    public FlexibleColorPicker colorPicker; 

    void Start()
    {
        if (SkinnedMeshRenderer == null)
        {
            Debug.LogError("SkinnedMeshRenderer is not assigned.");
            return;
        }

        if (MaterialIndex < SkinnedMeshRenderer.materials.Length)
        {
            _material = SkinnedMeshRenderer.materials[MaterialIndex];
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
            _material.color = HighlightColor;
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

    private void OnDestroy()
    {
        if (colorPicker != null)
        {
            colorPicker.onColorChange.RemoveListener(OnColorChanged); 
        }
    }
}
