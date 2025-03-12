using System;
using UnityEngine;

public class PlatesCounter : BaseCounter
{
    [SerializeField]
    KitchenObjectSO plateSo;
    [SerializeField]
    float deltaTimeBetweenSpawns = 4;
    float spawnPlateTimer;

    [SerializeField]
    float maxNumSpawnedPlates = 4;
    float numSpawnedPlates;

    public Action plateSpawnEvent;
    public Action plateRemoveEvent;
    void Start()
    {
        numSpawnedPlates = 0;
    }

    // Update is called once per frame
    void Update()
    {
        spawnPlateTimer += Time.deltaTime;
        if (spawnPlateTimer > deltaTimeBetweenSpawns)
        {
            spawnPlateTimer = 0;
            if (numSpawnedPlates < maxNumSpawnedPlates)
            {
                numSpawnedPlates++;
                //var plate = KitchenObject.SpawnKitchenObject(plateSo, this);
                plateSpawnEvent.Invoke();
            }
        }
    }

    public override void Interact(Player player)
    {
        if (player.HasKitchenObject() == false)
        {
            if(numSpawnedPlates > 0)
            {
                numSpawnedPlates--;
                plateRemoveEvent.Invoke();

                var plate = KitchenObject.SpawnKitchenObject(plateSo, player);
            }
        }
    }
}
