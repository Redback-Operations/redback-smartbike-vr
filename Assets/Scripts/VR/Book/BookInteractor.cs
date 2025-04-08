using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BookInteractor : MonoBehaviour
{
    public bool DisableOnDrop;
    public bool CloseOnDrop;

    public SaveLoadBike Bikes;
    public Book Target;

    public int PageOffset;

    private IXRSelectInteractable _interactable;

    void Awake()
    {
        EnablePageTurning(false);
        _interactable = GetComponent<XRBaseInteractable>();

        if (Target != null)
            Target.SelectPage(PlayerPrefs.GetInt("SelectedBike") + 1);
    }

    void OnEnable()
    {
        _interactable.selectExited.AddListener(OnSelectExit);
        _interactable.selectEntered.AddListener(OnSelect);
    }

    void OnDisable()
    {
        _interactable.selectExited.RemoveListener(OnSelectExit);
        _interactable.selectEntered.RemoveListener(OnSelect);
    }

    void OnSelect(SelectEnterEventArgs args) => EnablePageTurning(true);
    void OnSelectExit(SelectExitEventArgs args) => EnablePageTurning(false);

    public void EnablePageTurning(bool state)
    {
        if (Target == null)
            return;

        Target.interactable = state || !DisableOnDrop;

        if (!state && CloseOnDrop)
            Target.CloseBook();
    }

    public void BookInteractor_SelectBike(int index)
    {
        var bike = index - PageOffset;
        Debug.Log($"Selecting Bike {bike}");

        if (bike < 0)
            return;

        Bikes.DisplayBike(bike);
        Bikes.SelectBike();
    }
}
