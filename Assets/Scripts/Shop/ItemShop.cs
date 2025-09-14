using UnityEngine;

public struct ItemAddedEvent : IEvent
{
    public string itemName;
    public int quantity;

    public ItemAddedEvent(string itemName, int quantity)
    {
        this.itemName = itemName;
        this.quantity = quantity;
    }
}

public struct ItemRemovedEvent : IEvent
{
    public string itemName;
    public int quantity;
}
public class ItemShop : MonoBehaviour, IInteractable
{
    [SerializeField] private string itemName;
    [SerializeField] private string resourceName;
    [SerializeField] private int requiredAmount = 1;

    public IInteractionCondition InteractionCondition =>
        new CompositeCondition(
            new KeyPressCondition(KeyCode.T),
            new PlayerHasResourceCondition(resourceName, requiredAmount));

    public void OnInteractedBy(IInteractor interactor)
    {
        if (InteractionCondition.CanInteract(interactor))
        {
            EventBus<ItemAddedEvent>.Raise(new ItemAddedEvent(itemName,1));
        }
    }
}