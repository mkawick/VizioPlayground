using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    public Action NewOrder;
    public Action OrderFulfilled;
    public static DeliveryManager Instance { get; private set; }

    List<BurgerRecipeSO> waitingRecipeList;
    [SerializeField] RecipeListSO recipeListSO;

    public List<BurgerRecipeSO> PendingRecipes { get { return waitingRecipeList; } }

    float spawnRecipeTimer;
    float spawnRecipeTimerMax = 4;
    int waitingRecipesMax = 4;

    private void Awake()
    {
        waitingRecipeList = new List<BurgerRecipeSO>();
        spawnRecipeTimer = spawnRecipeTimerMax;
        Instance = this;
    }

    private void Update()
    {
        spawnRecipeTimer -= Time.deltaTime;
        if(spawnRecipeTimer < 0 )
        {
            spawnRecipeTimer = spawnRecipeTimerMax;
            if (waitingRecipesMax <= waitingRecipeList.Count)
                return;

            var waitingRecipeSo = recipeListSO.recipeList[UnityEngine.Random.Range(0, recipeListSO.recipeList.Count)];
            //Debug.Log(waitingRecipeSo.recipeName);
            waitingRecipeList.Add( waitingRecipeSo );
            NewOrder.Invoke();
        }
    }

    public void DeliverRecipe(PlateKitchenObject plateKitchenObject)
    {
        for(int i=0; i< waitingRecipeList.Count;i++)
        {
            BurgerRecipeSO waitingRecipe = waitingRecipeList[i];
            if(waitingRecipe.kitchenObjectSOList.Count == plateKitchenObject.Ingredients.Count)// same num of ingredients
            {
                bool matches = true;
                foreach(KitchenObjectSO recipeObject in waitingRecipe.kitchenObjectSOList)
                {
                    if(plateKitchenObject.Ingredients.Contains(recipeObject) == false)
                    {
                        matches = false;
                        break;
                    }
                }
                if(matches == true)
                {
                    //Debug.Log("Player delivered the correct recipe");
                    waitingRecipeList.RemoveAt(i);
                    OrderFulfilled.Invoke();
                    return;
                }
            }
        }
        Debug.Log("Player didn't deliver a correct recipe");
    }
}
