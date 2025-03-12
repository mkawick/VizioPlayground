using UnityEngine;

public class BaseCounter : MonoBehaviour, IKitchenObjectParent
{
    [SerializeField]
    protected Transform counterTopPoint;

    protected KitchenObject objectOnCounter;
    public KitchenObject KitchenObjectOnCounter { get { return objectOnCounter; } set { objectOnCounter = value; } }
    public virtual void Interact(Player player) { Debug.LogError("base class invoked"); }
    public virtual void Slice(Player player) 
    { 
        //Debug.LogError("base class slice invoked"); 
    }


    public Transform GetKitchenObjectFollowTransform()
    {
        return counterTopPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        KitchenObjectOnCounter = kitchenObject;
    }

    public KitchenObject GetKitchenObject()
    {
        return KitchenObjectOnCounter;
    }

    public void ClearKitchenObject()
    {
        KitchenObjectOnCounter = null;
    }

    public bool HasKitchenObject() => KitchenObjectOnCounter != null;
}
