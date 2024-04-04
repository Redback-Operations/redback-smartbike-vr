using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class HoverButton : MonoBehaviour
{
    [Serializable]
    public enum HoverButtonAction
    {
        None,
        LoadScene,
        ObjectsToggle,
        ObjectsOn,
        ObjectsOff
    }

    public HoverButtonAction Action;

    public Transform[] ToggleTargets;

    public string SceneTarget;

    public Renderer Renderer;
    public Color Selected = Color.white;
    public Color Unselected = Color.gray;

    private Material _material;
    private bool _selected;

    void Start()
    {
        _selected = false;

        if (Renderer == null)
            return;

        _material = Renderer.material;

        if (_material != null)
            _material.color = Unselected;
    }

    void LateUpdate()
    {
        // not needed, will use the XR events instead
        if (XRSettings.enabled || !_selected)
            return;

        if (Input.GetMouseButtonDown(0))
            ButtonInteract();
    }

    void OnMouseEnter()
    {
        _selected = true;
        ButtonHoverEnter();
    }

    void OnMouseExit()
    {
        _selected = false;
        ButtonHoverExit();
    }

    public void ButtonHoverEnter()
    {
        if (_material == null)
            return;

        _material.color = Selected;
    }

    public void ButtonHoverExit()
    {
        if (_material == null)
            return;

        _material.color = Unselected;
    }

    public void ButtonInteract()
    {
        if (Action == HoverButtonAction.None)
            return;

        switch (Action)
        {
            case HoverButtonAction.LoadScene:
            {
                if (string.IsNullOrWhiteSpace(SceneTarget))
                    return;

                SceneManager.LoadScene(SceneTarget);
            } break;

            default:
            {
                if (ToggleTargets == null || ToggleTargets.Length == 0)
                    return;

                foreach (var target in ToggleTargets)
                {
                    if (Action == HoverButtonAction.ObjectsToggle)
                        target.gameObject.SetActive(!target.gameObject.activeInHierarchy);
                    else
                        target.gameObject.SetActive(Action == HoverButtonAction.ObjectsOn);
                }
            } break;
        }
    }
}
