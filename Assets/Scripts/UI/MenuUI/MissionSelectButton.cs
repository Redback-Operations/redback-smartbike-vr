using System;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class MissionSelectButton : MonoBehaviour, IUIButton
{
    private MissionSelectButtonManager _manager;
    public ButtonAction Action;

    public Renderer Renderer;
    public Color Selected = Color.white;
    public Color Unselected = Color.gray;

    private Material _material;
    private bool _selected;

    private XRSimpleInteractable _interactable;
    private Button _button;

    [Serializable]
    public enum ButtonAction
    {
        None,
        Base,
        MissionSelect,
        Back,
        Start
    }


    void Awake()
    {
        _manager = GetComponentInParent<MissionSelectButtonManager>(true); //find manager in ancestor
        _interactable = GetComponent<XRSimpleInteractable>();
        _button = GetComponent<Button>();
    }

    void Start()
    {
        _selected = false;

        if (Renderer == null)
            return;

        _material = Renderer.material;

        if (_material != null)
            _material.color = Unselected;
    }

    void OnEnable()
    {
        if (_interactable != null)
        {
            _interactable.hoverEntered.AddListener(ButtonHoverEnter);
            _interactable.hoverExited.AddListener(ButtonHoverExit);
            _interactable.selectEntered.AddListener(OnButtonInteract);
        }

        if (_button != null)
        {
            _button.onClick.AddListener(OnButtonInteract);
        }
    }

    void OnDisable()
    {
        if (_interactable != null)
        {
            _interactable.hoverEntered.RemoveListener(ButtonHoverEnter);
            _interactable.hoverExited.RemoveListener(ButtonHoverExit);
            _interactable.selectEntered.RemoveListener(OnButtonInteract);
        }

        if (_button != null)
        {
            _button.onClick.RemoveListener(OnButtonInteract);
        }
    }

    void LateUpdate()
    {
        // not needed, will use the events instead
        if (!_selected || _button != null)
            return;

        if (Input.GetMouseButtonDown(0))
            OnButtonInteract();
    }

    void OnMouseEnter()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        _selected = true;
        ButtonHoverEnter(null);
    }

    void OnMouseExit()
    {
        _selected = false;
        ButtonHoverExit(null);
    }

    public void ButtonHoverEnter(HoverEnterEventArgs args)
    {
        if (_material == null)
            return;

        _material.color = Selected;
    }

    public void ButtonHoverExit(HoverExitEventArgs args)
    {
        if (_material == null)
            return;

        _material.color = Unselected;
    }

    public void OnButtonInteract(SelectEnterEventArgs args)
    {
        OnButtonInteract();
    }

    public void OnButtonInteract()
    {
        Debug.Log($"Sending button hit: {name}");
        _manager.ButtonInteract(this);
        return;
    }
}
