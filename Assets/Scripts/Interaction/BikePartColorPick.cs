using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BikePartColorPick : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    public Bike bike;
    public string partName;
    public Color highlightColor = Color.white;
    private Color _prevColor;
    public void OnColorChanged(Color newColor)
    {
        bike.SetPartColor(partName,newColor,true);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        _prevColor = bike.GetPartColor(partName).GetValueOrDefault();
        bike.SetPartColor(partName,highlightColor,false);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        bike.SetPartColor(partName,_prevColor,false);
    }
}
