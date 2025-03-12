using System;
using System.Collections.Generic;
using UnityEngine;

public class Burger : MonoBehaviour
{
    [Serializable]
    public struct AcceptableBurgerComponents
    {
        public KitchenObjectSO kitchenObjectSO;
        public GameObject gameObjectPrefab;
    }
    [SerializeField]
    PlateKitchenObject plateKitchenObject;
    [SerializeField] 
    List<AcceptableBurgerComponents> burgerComponents;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        plateKitchenObject.OnIngredientAdded += IngredientAdded;
    }

    private void IngredientAdded(KitchenObjectSO kitchenObjectSO)
    {
        foreach(var comp in burgerComponents)
        {
            if(comp.kitchenObjectSO == kitchenObjectSO)
            {
                comp.gameObjectPrefab.SetActive(true);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
