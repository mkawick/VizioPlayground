using UnityEngine;

public class ClearCounter : BaseCounter
{

    [SerializeField]
    protected KitchenObjectSO kitchenObjectSO;

    public override void Interact(Player player)
    {
        if(HasKitchenObject()) 
        {
            if (player.HasKitchenObject())
            {
                PlateKitchenObject plate;
                if (player.GetKitchenObject().TryGetPlate(out plate))
                {
                    if (plate.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        GetKitchenObject().DestroySelf();
                    }
                }
                else // not a plate, player carrying something else
                {
                    if(GetKitchenObject().TryGetPlate(out plate))
                    {
                        if (plate.TryAddIngredient(player.GetKitchenObject().GetKitchenObjectSO()))
                        {
                            player.GetKitchenObject().DestroySelf();
                        }
                    }
                }
            }
            else
            {
                GetKitchenObject().KitchenObjectParent = player;
            }
        }
        else
        {
            if(player.HasKitchenObject())
            {
                player.GetKitchenObject().KitchenObjectParent = this;
            }
        }
    }

    public void SetSelected(bool selected)
    {
        GetComponentInChildren<SelectedCounterVisual>()?.SetSelected(selected);
    }
}
