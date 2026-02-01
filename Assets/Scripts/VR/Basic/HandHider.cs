using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HandHider : MonoBehaviour
{
    public GameObject handObject = null;

    private HandPhysics handPhysics = null;
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor = null;

    private void Awake()
    {
        handPhysics = handObject.GetComponent<HandPhysics>();
        interactor = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>();
    }

    private void OnEnable()
    {
        interactor.selectEntered.AddListener(Hide);
        interactor.selectExited.AddListener(Show);
    }

    private void OnDisable()
    {
        interactor.selectEntered.RemoveListener(Hide);
        interactor.selectExited.RemoveListener(Show);
    }

    private void Show(SelectExitEventArgs arg0)
    {
        handPhysics.TeleportToTarget();
        handObject.SetActive(true);
    }

    private void Hide(SelectEnterEventArgs arg0)
    {
        handObject.SetActive(false);
    }
}
