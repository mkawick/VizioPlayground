using System;
using Unity.VisualScripting;
using UnityEngine;

public class CuttingCounter : BaseCounter, IProgressBarUI
{
    [SerializeField]
    protected CuttingRecipeSO[] cuttingRecipes;
    public event EventHandler<IProgressBarUI.OnProgressChangedEventArgs> OnProgressChanged;

    public event EventHandler OnCut;

    int cutsSoFar;
    const int numCutsToSucceed = 4;

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
            if (player.HasKitchenObject())
            {
                var kitchenObject = player.GetKitchenObject();
                KitchenObjectSO so = GetOutput(kitchenObject.GetKitchenObjectSO());
                if (so != null)
                {
                    // player drops object
                    kitchenObject.KitchenObjectParent = this;
                    //cutsSoFar = 0;
                    SetNumCuts(0);
                }
            }
        }
    }

    public override void Slice(Player player)
    {
        if(HasKitchenObject())
        {
            var kitchenObject = GetKitchenObject();
            var recipe = GetRecipe(kitchenObject.GetKitchenObjectSO());
            //++cutsSoFar >= recipe.cutsToSucceed;
            if (recipe != null)
            {
                SetNumCuts(++cutsSoFar, recipe.cutsToSucceed);
                OnCut?.Invoke(this, EventArgs.Empty);
                if (cutsSoFar >= recipe.cutsToSucceed)
                {
                    var cutkitchenObjectSO = GetOutput(kitchenObject.GetKitchenObjectSO());
                    if (cutkitchenObjectSO != null)
                    {
                        kitchenObject.DestroySelf();
                        KitchenObject.SpawnKitchenObject(cutkitchenObjectSO, this);
                    }
                }
            }
        }
    }

    void SetNumCuts(int num, int max = 1)
    {
        if (num == 0 || max == 0)
        {
            OnProgressChanged?.Invoke(this, new IProgressBarUI.OnProgressChangedEventArgs { percentage = 0 });
        }
        else
        {
            OnProgressChanged?.Invoke(this, new IProgressBarUI.OnProgressChangedEventArgs { percentage = (float)num / (float)max }); ;
        }
        cutsSoFar = num;
    }

    KitchenObjectSO GetOutput(KitchenObjectSO kitchenObjectInput)
    {
        var recipe = GetRecipe(kitchenObjectInput);
        if(recipe != null)
            return recipe.output;

        return null;
    }

    CuttingRecipeSO GetRecipe(KitchenObjectSO kitchenObjectInput)
    {
        foreach (var originalItem in cuttingRecipes)
        {
            if (originalItem.input == kitchenObjectInput)
            {
                return originalItem;
            }
        }
        return null;
    }
}
