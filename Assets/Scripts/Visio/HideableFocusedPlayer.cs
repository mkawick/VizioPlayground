using System;
using UnityEngine;
using UnityEngine.Serialization;

//[assembly: BindTypeNameToType("HideablePlayerController", typeof(HideablePlayerController))]
public class HideableFocusedPlayer : HideableCharacterController
{
    //public override void Hide() { }
    public override bool IsLocalPlayer() { return true; }

    public override void ObjectBecameVisible(int objId)
    {
        if (objId == HideableId)
        {
            return; // don't add self
        }
        var obj = _tinyWizHideableManager.GetObjectById(objId);
        if(obj != null)
        {
            _objectsISee.Add(objId); // only add if valid
            obj.Show();
        }
    }

    public override void ObjectBecameInvisible(int objId)
    {
        _objectsISee.Remove(objId); // always remove regardless
        AddHidableToHistory(objId, true);
    }

    public override void ClearHistory(int timeStamp)
    {
        // this is a queue and is ordered by time.. fall out early 
        while (objectsIUsedToSee.Count > 0)
        {
            if (objectsIUsedToSee.Peek()._timestamp < timeStamp)
            {
                var history = objectsIUsedToSee.Dequeue();
                foreach (var objId in history._objectsISee)
                {
                    var obj = _tinyWizHideableManager.GetObjectById(objId);
                    if (obj != null)
                    {
                        obj.Hide();
                    }
                    _objectsISee.Remove(objId);
                }
            }
            else
            {
                break;
            }
        }
    }
}
