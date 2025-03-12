using UnityEngine;

public class KitchenObject : MonoBehaviour
{
    [SerializeField]
    private KitchenObjectSO kitchenObjectSO;
    IKitchenObjectParent kitchenObjectParent;

    public IKitchenObjectParent KitchenObjectParent {
        get { return kitchenObjectParent; }
        set 
        { 
            if(kitchenObjectParent != null)
            {
                kitchenObjectParent.ClearKitchenObject();
            }
            kitchenObjectParent = value;
            if (kitchenObjectParent != null)
            {
                if(kitchenObjectParent.HasKitchenObject())
                {
                    Debug.LogError("IKitchenObjectParent already has object on it");
                }
                kitchenObjectParent.SetKitchenObject(this);
                transform.parent = kitchenObjectParent.GetKitchenObjectFollowTransform();
                transform.localPosition = Vector3.zero;
            }
        }
    }

    public KitchenObjectSO GetKitchenObjectSO() { return kitchenObjectSO; }

    public void DestroySelf()
    {
        kitchenObjectParent?.ClearKitchenObject() ;
        Destroy(gameObject);
    }

    static public KitchenObject SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent parent)
    {
        Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.objectPrefab);
        var obj = kitchenObjectTransform.GetComponent<KitchenObject>();
        obj.KitchenObjectParent = parent;

        return obj;
    }

    public bool TryGetPlate(out PlateKitchenObject plate)
    {
        if (this is PlateKitchenObject)
        {
            plate = this as PlateKitchenObject;
            return true;
        }

        plate = null;
        return false;
    }
}
