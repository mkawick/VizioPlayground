using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PlatesCounterVisual : MonoBehaviour
{
    [SerializeField]
    PlatesCounter platesCounter;
    [SerializeField]
    Transform counterTopPoint;
    [SerializeField]
    GameObject plateVisualPrefab;

    private List<GameObject> plates;

    void Awake()
    {
        plates = new List<GameObject>();
    }
   
    void Start() 
    {
        platesCounter.plateSpawnEvent += PlateSpawn;
        platesCounter.plateRemoveEvent += RemovePlate;
    }

    void PlateSpawn()
    {
        var plate = Instantiate(plateVisualPrefab, counterTopPoint);

        float plateYOffset = 0.08f * plates.Count;
        plate.transform.localPosition = new Vector3(0, plateYOffset, 0);
        plates.Add(plate);
    }

    void RemovePlate()
    {
        if(plates.Count == 0)
            return;
        var plate = plates[plates.Count-1];
        plates.Remove(plate);
        Destroy(plate);
    }
}
