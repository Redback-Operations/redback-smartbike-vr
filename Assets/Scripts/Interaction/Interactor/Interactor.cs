using System.Collections.Generic;
using UnityEngine;
public class Interactor : MonoBehaviour, IInteractor
{
    private readonly List<IInteractable> interactablesInRange = new();
    public void InteractWith(IInteractable interactable)
    {
        if (interactable.InteractionCondition.CanInteract(this))
        {
            interactable.OnInteractedBy(this);
        }
    }

    void Update()
    {
        foreach (var interactable in interactablesInRange)
        {
            if (interactable.InteractionCondition == null) continue;
            if (interactable.InteractionCondition.CanInteract(this))
            {
                InteractWith(interactable);
                break; // Only interact with the first one
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var interactable = other.GetComponent<IInteractable>();
        if (interactable != null && !interactablesInRange.Contains(interactable))
        {
            interactablesInRange.Add(interactable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var interactable = other.GetComponent<IInteractable>();
        if (interactable != null && interactablesInRange.Contains(interactable))
        {
            interactablesInRange.Remove(interactable);
        }
    }
}