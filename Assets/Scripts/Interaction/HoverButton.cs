using System;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class HoverButton : MonoBehaviour
{
    [Serializable]
    public enum HoverButtonAction
    {
        None,
        LoadScene,
        ObjectsToggle,
        ObjectsOn,
        ObjectsOff,
        SetInteger,
        SetString,
        SetIntegerThenLoad,
        SetStringThenLoad
    }

    public HoverButtonAction Action;

    public string TargetScene;
    public string TargetValue;
    public int IntValue;
    public string StringValue;

    public Transform[] ToggleTargets;

    [SerializeField] private UnityEvent onInteract;

    public Renderer Renderer;
    public Color Selected = Color.white;
    public Color Unselected = Color.gray;

    private Material _material;
    private bool _selected;

    private XRSimpleInteractable _interactable;
    private Button _button;

    void Awake()
    {
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
            _button.onClick.AddListener(ButtonInteract);
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
            _button.onClick.RemoveListener(ButtonInteract);
        }
    }

    void LateUpdate()
    {
        // not needed, will use the events instead
        if (!_selected || _button != null)
            return;

        if (Input.GetMouseButtonDown(0))
            ButtonInteract();
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

    private void OnButtonInteract(SelectEnterEventArgs args)
    {
        ButtonInteract();
    }

    public void ButtonInteract()
    {
        if (Action == HoverButtonAction.None)
            return;

        switch (Action)
        {
            case HoverButtonAction.LoadScene:
            {
                if (string.IsNullOrWhiteSpace(TargetScene))
                    return;

                MapLoader.LoadScene(TargetScene);
            } break;

            case HoverButtonAction.SetInteger:
            {
                PlayerPrefs.SetInt(TargetValue, IntValue);
            } break;

            case HoverButtonAction.SetIntegerThenLoad:
            {
                if (string.IsNullOrWhiteSpace(TargetScene) || string.IsNullOrWhiteSpace(TargetValue))
                    return;

                PlayerPrefs.SetInt(TargetValue, IntValue);
                MapLoader.LoadScene(TargetScene);
            } break;

            case HoverButtonAction.SetString:
            {
                PlayerPrefs.SetString(TargetValue, StringValue);
            } break;

            case HoverButtonAction.SetStringThenLoad:
            {
                if (string.IsNullOrWhiteSpace(TargetScene) || string.IsNullOrWhiteSpace(TargetValue))
                    return;

                PlayerPrefs.SetString(TargetValue, StringValue);
                MapLoader.LoadScene(TargetScene);
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
        
        onInteract?.Invoke();
    }
}
