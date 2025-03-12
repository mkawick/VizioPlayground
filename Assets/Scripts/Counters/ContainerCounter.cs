using System;
using UnityEngine;

public class ContainerCounter : BaseCounter
{
    public event EventHandler OnPlayerGrabObject;

    [SerializeField]
    protected KitchenObjectSO kitchenObjectSO;

    public override void Interact(Player player)
    {
        if (player.HasKitchenObject()) { return; }

        KitchenObject.SpawnKitchenObject(kitchenObjectSO, player);
        OnPlayerGrabObject?.Invoke(this, EventArgs.Empty);
    }
}
