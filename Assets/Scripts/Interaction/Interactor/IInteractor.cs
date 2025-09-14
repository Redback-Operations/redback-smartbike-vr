using System;
using System.Linq;
using UnityEngine;

public interface IInteractor
{
    /// <summary>
    /// Called when the interactor begins interacting with an object.
    /// </summary>
    /// <param name="interactable">The object being interacted with.</param>
    void InteractWith(IInteractable interactable);
}

public interface IInteractable
{
    IInteractionCondition InteractionCondition { get; }

    /// <summary>
    /// Called when this object is being interacted with.
    /// </summary>
    /// <param name="interactor">The actor initiating the interaction.</param>
    void OnInteractedBy(IInteractor interactor);
}

public interface IInteractionCondition
{
    bool CanInteract(IInteractor interactor);
    string GetFailureMessage();
}

[Serializable]
public class KeyPressCondition : IInteractionCondition
{
    [SerializeField] private KeyCode requiredKey;
    public string interactionName;
    public KeyPressCondition(KeyCode requiredKey)
    {
        new KeyPressCondition(requiredKey, "interact");
    }
    public KeyPressCondition(KeyCode requiredKey, string interactionName)
    {
        this.requiredKey = requiredKey;
        this.interactionName = interactionName;
    }

    public bool CanInteract(IInteractor interactor)
    {
        return Input.GetKeyDown(requiredKey);
    }

    public string GetFailureMessage()
    {
        return $"Press {requiredKey} to {interactionName}";
    }
}

public class CompositeCondition : IInteractionCondition
{
    private IInteractionCondition[] _conditions;
    private string failMessage = "";

    public CompositeCondition(params IInteractionCondition[] conditions)
    {
        _conditions = conditions;
    }

    public bool CanInteract(IInteractor interactor)
    {
        failMessage = "";
        foreach (var condition in _conditions)
        {
            if (!condition.CanInteract(interactor))
            {
                failMessage += condition.GetFailureMessage() + "\n";
                return false;
            }
        }
        return true;
    }

    public string GetFailureMessage()
    {
        return failMessage;
    }
}

public class PlayerHasResourceCondition : IInteractionCondition
{
    private readonly string _resourceName;
    private readonly int _amount;

    public PlayerHasResourceCondition(string resourceName, int amount)
    {
        _resourceName = resourceName;
        _amount = amount;
    }

    public bool CanInteract(IInteractor interactor)
    {
        if (interactor is MonoBehaviour behaviour)
        {
            return behaviour.GetComponent<PlayerController>()
                .inventory
                .GetItemCount(_resourceName) >= _amount;
        }
        return false;
    }
    public string GetFailureMessage()
    {
        return $"{_amount} {_resourceName}s required";
    }
}