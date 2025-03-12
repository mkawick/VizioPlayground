using UnityEngine;

public class TrashBin : BaseCounter
{

    public override void Interact(Player player)
    {
        if(player.HasKitchenObject())
        {
            player.GetKitchenObject().DestroySelf();
        }
        //base.Interact(player);
    }
}
