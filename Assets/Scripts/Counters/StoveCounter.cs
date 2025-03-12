using System;
using UnityEngine;
using static CuttingCounter;

public class StoveCounter : BaseCounter, IProgressBarUI
{
    [SerializeField]
    FryingRecipeSO[] fryingRecipes;

    FryingRecipeSO fryingRecipeSO;
    float fryTimer;

    public class StoveStateChanged : EventArgs
    {
        public bool burnerOn;
        public bool isCooking;
    }
    public event EventHandler<StoveStateChanged> OnStoveStateChangedEvent;
    public event EventHandler<IProgressBarUI.OnProgressChangedEventArgs> OnProgressChanged;

    private void Update()
    {
        if(HasKitchenObject())
        {
            fryTimer += Time.deltaTime;
            if (fryingRecipeSO != null)
            {
                OnProgressChanged?.Invoke(this, new IProgressBarUI.OnProgressChangedEventArgs { percentage = fryTimer / fryingRecipeSO.fryingTimerMax });
                if(fryingRecipeSO.fryingTimerMax < fryTimer)
                {
                    GetKitchenObject().DestroySelf();
                    fryTimer = 0;
                    KitchenObject.SpawnKitchenObject(fryingRecipeSO.output, this);
                    fryingRecipeSO = GetRecipe(fryingRecipeSO.output);
                    if (fryingRecipeSO)
                    {
                        OnStoveStateChangedEvent?.Invoke(this, new StoveStateChanged { burnerOn = true, isCooking = fryingRecipeSO.isFrying });

                    }
                    else
                    {
                        // keep making sparks
                        OnStoveStateChangedEvent?.Invoke(this, new StoveStateChanged { burnerOn = false, isCooking = true });
                    }
                }
            }
        }
    }

    public override void Interact(Player player)
    {
        if (HasKitchenObject())
        {
            if (player.HasKitchenObject())
            {
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plate))
                {
                    if (plate.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        GetKitchenObject().DestroySelf();
                        ResetStove();
                    }
                }
            }
            else
            { 
                GetKitchenObject().KitchenObjectParent = player;
                ResetStove();
            }
        }
        else
        {
            if (player.HasKitchenObject())
            {
                var kitchenObject = player.GetKitchenObject();
                var recipe = GetRecipe(kitchenObject.GetKitchenObjectSO());
                if (recipe != null) {
                    KitchenObjectSO so = recipe.output;
                    if (so != null)
                    {
                        // player drops object
                        kitchenObject.KitchenObjectParent = this;
                        SetProgress(0);
                        fryTimer = 0;
                        OnStoveStateChangedEvent?.Invoke(this, new StoveStateChanged { burnerOn = true, isCooking = false });
                        OnProgressChanged?.Invoke(this, new IProgressBarUI.OnProgressChangedEventArgs { percentage = 1 });

                        fryingRecipeSO = GetRecipe(GetKitchenObject().GetKitchenObjectSO());
                    }
                }
            }
        }
    }

    void ResetStove()
    {
        fryTimer = 0;
        OnStoveStateChangedEvent?.Invoke(this, new StoveStateChanged { burnerOn = false, isCooking = false });
        OnProgressChanged?.Invoke(this, new IProgressBarUI.OnProgressChangedEventArgs { percentage = 0 });
    }

    void SetProgress(int num, int max = 1)
    {
       /* if (num == 0 || max == 0)
        {
            OnProgressChanged?.Invoke(this, new OnProgressChangedEventArgs { percentage = 0 });
        }
        else
        {
            OnProgressChanged?.Invoke(this, new OnProgressChangedEventArgs { percentage = (float)num / (float)max }); ;
        }*/
    }

    FryingRecipeSO GetRecipe(KitchenObjectSO kitchenObjectInput)
    {
        foreach (var originalItem in fryingRecipes)
        {
            if (originalItem.input == kitchenObjectInput)
            {
                return originalItem;
            }
        }
        return null;
    }
}
