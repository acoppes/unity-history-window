using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

[ExecuteInEditMode]
public class GlobalIdTestSceneController : MonoBehaviour
{
    public Object reference;
    public string referenceGlobalId;

    public Object loadedReference;

    [ContextMenu("ToGlobalId")]
    public void SaveReferenceToGlobalId()
    {
        if (reference != null)
        {
            var globalId = UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(reference);
            referenceGlobalId = globalId.ToString();
        }
        else
        {
            referenceGlobalId = string.Empty;
        }
    }
    
    [ContextMenu("ResetGlobalId")]
    public void ResetGlobalId()
    {
        referenceGlobalId = string.Empty;
    }

    [ContextMenu("Load")]
    public void LoadReferenceFromGlobalId()
    {
        if (UnityEditor.GlobalObjectId.TryParse(referenceGlobalId, out var globalId))
        {
            loadedReference = UnityEditor.GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalId);
        }
        else
        {
            loadedReference = null;
        }
    }
    
    [ContextMenu("Reset")]
    public void ResetLoadedReference()
    {
        loadedReference = null;
    }
}
