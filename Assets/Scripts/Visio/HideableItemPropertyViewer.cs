using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SearchService;
using UnityEngine;
using static IHideableObject;

public class HideableItemPropertyViewer : MonoBehaviour
{
    IHideableObject hideableObject;
    [SerializeField] int myZone;
    [SerializeField] int myLastZone;
    [SerializeField] float timestamp;
    [SerializeField] bool autoHideOldVisibles;
    [SerializeField] bool showVisibleConnections;
    [SerializeField] List<string> objectsISee;
    [SerializeField] List<string> objectsISaw; //Queue<HistoricalListOfObjectsISaw> viewHistory;

    static TinyWizHideableManager _tinyWizPlayerManager;
    private void Start()
    {
        hideableObject = GetComponent< IHideableObject>();
        if (_tinyWizPlayerManager == null)
        {
            _tinyWizPlayerManager = GameObject.FindAnyObjectByType<TinyWizHideableManager>();
        }
       // hideableObject == localPlayer;
    }
    [Button]
    public void HideObjectsFromPrevRoom()
    {
        var viewHistory = hideableObject.objectsIUsedToSee;
        if (viewHistory != null && viewHistory.Count != 0)
        {
            hideableObject.ClearHistory(Time.frameCount);
            /*var history = viewHistory.Dequeue();
            foreach(var objId in history._objectsISee)
            {
                var obj = _tinyWizPlayerManager.GetObjectById(objId);
                if (obj)
                {
                    obj.MakeMeshVisible(false);
                    Debug.Log("don't fool yourself, this is not done.. this object needs to hide you too, deque may be enough");
                    //objectsISee.Add(obj.name);
                }
            }*/
                

            Debug.Log("Items removed");
        }
        else
        {
            Debug.Log("Invalid");
        }
        
    }

    void Update()
    {
        var tempZone = hideableObject.GetZone();
        if(myZone != tempZone)
        {
            myLastZone = myZone;
            myZone = tempZone;
        }
        UpdateListOfVisibleObjects();

        ShowConnections();

        UpdateHistoricalView();
    }

    private void UpdateListOfVisibleObjects()
    {
        var visibleObjects = hideableObject.ObjectsISee;
        var replacementObjectsISee = new List<string>();
        int matchingCount = 0;
        foreach (var objId in visibleObjects)
        {
            var obj = _tinyWizPlayerManager.GetObjectById(objId);
            if (obj)
            {
                if (objectsISee.Contains(obj.name))
                    matchingCount++;
                replacementObjectsISee.Add(obj.name);
            }
        }

        if (replacementObjectsISee.Count != matchingCount 
            || objectsISee.Count != matchingCount)
        {
            objectsISee = replacementObjectsISee;
        }
    }

    private void ShowConnections()
    {
        if (showVisibleConnections)
        {
            var visibleObjects = hideableObject.ObjectsISee;
            Vector3 myPosition = hideableObject.transform.position;
            foreach (var objId in visibleObjects)
            {
                Vector3 endPoint = _tinyWizPlayerManager.GetObjectById(objId).transform.position;
                Debug.DrawLine(myPosition, endPoint, Color.green, 0.5f);
            }
        }
    }
    private void UpdateHistoricalView()
    {
        bool areTheSame = true;
        var replacementObjectsISee = new List<string>();
        var viewHistory = hideableObject.objectsIUsedToSee;
        if (viewHistory != null && viewHistory.Count != 0)
        {
            var top = viewHistory.Peek();
            timestamp = top._timestamp;
            foreach (var objId in top._objectsISee)
            {
                var obj = _tinyWizPlayerManager.GetObjectById(objId);
                if (obj)
                {
                    replacementObjectsISee.Add(obj.name);
                }
            }
        }

        if(areTheSame)
        {
            objectsISaw = replacementObjectsISee;
        }
        if (myLastZone != myZone && autoHideOldVisibles)
        {
            HideObjectsFromPrevRoom();
        }
    }
}
