using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public interface IUIButton
{
    void ButtonHoverEnter(HoverEnterEventArgs args);
    void ButtonHoverExit(HoverExitEventArgs args);
    void OnButtonInteract(SelectEnterEventArgs args);
    void OnButtonInteract();
}
