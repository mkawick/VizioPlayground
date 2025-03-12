using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using Sirenix.OdinInspector;
using static UnityEditor.FilePathAttribute;

public class KeyDoesNotExistException : SystemException
{
    public KeyDoesNotExistException() { }
    public KeyDoesNotExistException(string message) { }
    public KeyDoesNotExistException(string message, Exception inner) { }
    protected KeyDoesNotExistException(SerializationInfo info, StreamingContext context) { }
}

public class TinyWizHideableManager : MonoBehaviour
{
    Dictionary<int, IHideableObject> allHidableObjects = new Dictionary<int, IHideableObject>();
    bool hasFinishedInit = false;
    internal int incrementingHidableId = 1;

    void Start()
    {
        hasFinishedInit = true;
    }

    [Button]
    void SpawnPlayer()
    {
        /* Visio visio = GameObject.FindAnyObjectByType<Visio>();
         var zones = visio.ZoneList;*/
        PositionMover pm = GameObject.FindAnyObjectByType< PositionMover>();
        if(pm == null)
        {
            Debug.Log("PositionMover DNE");
            return;
        }
       // Locations
    }

    public Dictionary<int, IHideableObject> AllObjects => allHidableObjects;
    public Dictionary<int, IHideableObject> AttentiveObjects => allHidableObjects.Where(kv => kv.Value.Observant == true).ToDictionary(kv => kv.Key, kv => kv.Value);
    public bool HasFinishedInit => hasFinishedInit;

    internal IHideableObject GetObjectById(int id)
    {
        if (allHidableObjects.ContainsKey(id))
        {
            return allHidableObjects[id];
        }
        return null;
    }
    public void Register(IHideableObject obj)
    {
        if(obj.HideableId == 0)
        {
            obj.Hide();
            obj.HideableId = incrementingHidableId++;
            allHidableObjects.Add(obj.HideableId, obj);
        }
    }
    public void Remove(IHideableObject obj)
    {
        if (obj.HideableId != 0)
        {
            if (allHidableObjects.ContainsKey(obj.HideableId))
            {
                allHidableObjects.Remove(obj.HideableId);
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    /// TODO - test
    void SpawnObj()
    {

    }

    /// TODO - test
    void DeleteSpawnedObject()
    {

    }

    internal void GetAllPlayersInRoom(int zoneId,
        HashSet<int> objectsInZone)
    {
        foreach(var obj in allHidableObjects)
        {
            if(obj.Value.GetZone() == zoneId)
            {
                objectsInZone.Add(obj.Key);
            }
        }
    }

    internal void InformObjectThatIAmVisible(int hidableId, HashSet<int> audience)
    {
        if (allHidableObjects.ContainsKey(hidableId) == false)
            throw new KeyDoesNotExistException();

        var hider = allHidableObjects[hidableId];
        
        foreach(var objId in audience)
        {
            if (allHidableObjects.ContainsKey(objId) == false)
                continue;
            if (allHidableObjects[objId].Observant)
            {
                allHidableObjects[objId].ObjectBecameVisible(hidableId);
            }
            // send notification
        }
    }

    internal void ShowLocalObjects(HashSet<int> objectsToShow)
    {
        foreach(var objId in objectsToShow)
        {
            if (allHidableObjects.ContainsKey(objId) == false)
                continue;
            allHidableObjects[objId].Show();
        }
    }
}
