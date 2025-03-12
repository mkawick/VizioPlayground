using System;
using UnityEngine;

public class PlateIconsUI : MonoBehaviour
{
    [SerializeField]
    PlateKitchenObject plate;
    [SerializeField] Transform burgerIcons;

    private void Awake()
    {
        burgerIcons.gameObject.SetActive(false);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        plate.OnIngredientAdded += IngredientAdded;
    }

    void IngredientAdded(KitchenObjectSO kitchenObjectSO)
    {
        foreach (Transform child in transform) // restart
        {
            if (child == burgerIcons || child == plate)
                continue;
            Destroy(child.gameObject);
        }
        foreach(KitchenObjectSO objectSo in plate.Ingredients)
        {
            Transform iconTransform = Instantiate(burgerIcons, transform);
            iconTransform.gameObject.SetActive(true);
            iconTransform.GetComponent<PlateIconSingleObjectUI>().SetKitchenObjectSO(objectSo);
        }
    }

    // Update is called once per frame
    void Update()
    {
       /* Component GetComponent(IntPtr _unity_self, Type type)
        {
            var components = _unity_self.GetComponents();
            for(Component component in components)
            {
                if (component.GetType() == type)
                {
                    return component;
                }
            }
            return null;
        }
        Component GetComponentInChildren(IntPtr _unity_self, Type type)
        {
            var children = _unity_self.Children();
            for(Child child in children)
            {
                var component = GetComponent(child, type);
                if (component != null)
                {
                    return component;
                }
            }
            return null;
        }*/
    }
}
