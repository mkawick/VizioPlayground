using UnityEngine;

public class DeliveryCounter : BaseCounter 
{
   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Interact(Player player)
    {
        if (player == null) return;
        PlateKitchenObject plate;
        if(player.HasKitchenObject() && player.GetKitchenObject().TryGetPlate(out plate) == true)
        {
            DeliveryManager.Instance.DeliverRecipe(plate);
            player.GetKitchenObject().DestroySelf();
        }
    }
}
