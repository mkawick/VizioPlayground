using UnityEngine;

public class DeliveryManagerUI : MonoBehaviour
{
    [SerializeField] Transform recipeContainer;
    [SerializeField] Transform recipeTemplate;

    void Awake()
    {
        recipeTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        DeliveryManager.Instance.NewOrder += UpdateVisual;
        DeliveryManager.Instance.OrderFulfilled += UpdateVisual;
        UpdateVisual();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateVisual() 
    {
        DestroyOldVisuals();
        var listOfPendingRecipes = DeliveryManager.Instance.PendingRecipes;
        foreach(var burgerRecipe in listOfPendingRecipes)
        {
            Transform recipeTransform = Instantiate(recipeTemplate, recipeContainer);
            recipeTransform.gameObject.SetActive(true);
        }
        //recipeContainer.gameObject.SetActive(true);
    }

    void DestroyOldVisuals()
    {
        foreach(Transform transform in recipeContainer.transform)
        {
            if (transform == recipeTemplate)
                continue;

            Destroy(transform.gameObject);
        }
    }
}
