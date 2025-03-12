using System;
using UnityEngine;

public class Player : MonoBehaviour, IKitchenObjectParent
{
    public event EventHandler<OnSelectCounterChangeEventArgs> OnSelectCounterChange;
    public class OnSelectCounterChangeEventArgs : EventArgs
    {
        public BaseCounter selectedCounter;
    }
    float playerRadius = 0.7f;
    float playerHeight = 2.0f;

    [SerializeField, Range(0.2f, 20.0f)]
    float movementSpeed = 1;
    [SerializeField, Range(2f, 40.0f)]
    float rotateSpeed = 10;
    [SerializeField]
    GameInput gameInput;
    [SerializeField]
    LayerMask counterLayerMask;

    [SerializeField]
    private Transform kitchenObjectHoldingPoint;
    private KitchenObject kitchenObjectHeld;
    public KitchenObject KitchenObjectHeld{ get { return kitchenObjectHeld; } set { kitchenObjectHeld = value; } }

    private Vector3 lastMoveInteractDirection;
    private BaseCounter selectedCounter;

    public static Player Instance
    {
        get; private set;
    }

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogError("Player singleton trying to create second instance");
            Debug.Break();
        }
        Instance = this;
    }

    void Start()
    {
        gameInput.OnInterract += GameInput_OnInterract;
        gameInput.OnSlice += GameInput_OnSlice; 
    }

    private void GameInput_OnSlice(object sender, EventArgs e)
    {
        if (selectedCounter != null)
        {
            selectedCounter.Slice(this);
        }
    }

    private void GameInput_OnInterract(object sender, EventArgs e)
    {
        if(selectedCounter != null)
        {
            selectedCounter.Interact(this);
        }
    }

    Vector3 GetDirAfterCollision(float playerRadius, float playerHeight, Vector3 movementDir, float movementDist)
    {
        bool isBlocked = Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, movementDir, movementDist);

        if(isBlocked)
        {
            Vector3 moveDirX = new Vector3(movementDir.x, 0, 0).normalized;
            isBlocked = Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirX, movementDist);

            if(isBlocked)
            {
                Vector3 moveDirZ = new Vector3(0, 0, movementDir.z).normalized;
                isBlocked = Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirZ, movementDist);
                if(isBlocked)
                {
                    return Vector3.zero;
                }
                else
                {
                    return moveDirZ;
                }
            }
            else 
            {
                return moveDirX; 
            }
        }

        return movementDir;
    }

    void HandleMovement()
    {
        var inputVector = gameInput.GetMovementVectorNormalized();
        var movementDir = new Vector3(inputVector.x, 0, inputVector.y);

        if(movementDir != Vector3.zero) { IsWalking = true; }
        else { IsWalking = false; }

        var elapsedTime = Time.deltaTime;
        var movementDist = elapsedTime * movementSpeed;
        movementDir = GetDirAfterCollision(playerRadius, playerHeight, movementDir, movementDist);

        if (movementDir != Vector3.zero)
        {
            transform.position = transform.position + movementDir * movementDist;
        }
        transform.forward = Vector3.Slerp(transform.forward, movementDir, elapsedTime * rotateSpeed);        
    }

    private void SetSelectedCounter(BaseCounter clearCounter)
    {
        selectedCounter = clearCounter;
        OnSelectCounterChange?.Invoke(this, new OnSelectCounterChangeEventArgs
        {
            selectedCounter = selectedCounter
        });
    }

    void SetupTrackingForInteractions()
    {
        var inputVector = gameInput.GetMovementVectorNormalized();

        var movementDir = new Vector3(inputVector.x, 0, inputVector.y);

        if (movementDir != Vector3.zero)
        {
            lastMoveInteractDirection = movementDir;
        }

        float interactionDist = 2.0f;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, lastMoveInteractDirection, out hit, interactionDist, counterLayerMask))
        {
            if (hit.transform.TryGetComponent(out BaseCounter clearCounter))
            {
                if (selectedCounter != clearCounter)
                {
                    SetSelectedCounter(clearCounter);
                }
            }
            else
            {
                SetSelectedCounter(null);
            }
        }
        else
        {
            SetSelectedCounter(null);
        }
    }

    private void Update()
    {
        HandleMovement();
        SetupTrackingForInteractions();
    }

    public Transform GetKitchenObjectFollowTransform()
    {
        return kitchenObjectHoldingPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        kitchenObjectHeld = kitchenObject;
    }

    public KitchenObject GetKitchenObject()
    {
        return kitchenObjectHeld;
    }

    public void ClearKitchenObject()
    {
        kitchenObjectHeld = null;
    }

    public bool HasKitchenObject() => kitchenObjectHeld != null;

    public bool IsWalking
    {
        get; private set;
    }

}
