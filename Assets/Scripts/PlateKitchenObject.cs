using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlateKitchenObject : KitchenObject
{
    List<KitchenObjectSO> kitchenObjectSOList;
    [SerializeField] List<KitchenObjectSO> validKitchenObjectSOList;

    public System.Action<KitchenObjectSO> OnIngredientAdded;

    public List<KitchenObjectSO> Ingredients { get { return kitchenObjectSOList; } }

    private void Awake()
    {
        kitchenObjectSOList = new List<KitchenObjectSO>();
        //GetComponent<Rigidbody>();
    }

    public bool TryAddIngredient(KitchenObjectSO kitchenObjectSO) 
    {
        if (validKitchenObjectSOList.Contains(kitchenObjectSO) == false)
            return false;

        if (kitchenObjectSOList.Contains(kitchenObjectSO)) return false;
        
        kitchenObjectSOList.Add(kitchenObjectSO);
        OnIngredientAdded.Invoke(kitchenObjectSO);

        return true;

    }
}
