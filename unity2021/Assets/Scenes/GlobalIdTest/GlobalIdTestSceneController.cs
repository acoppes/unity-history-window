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
        #if UNITY_EDITOR
        if (reference != null)
        {
            var globalId = UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(reference);
            referenceGlobalId = globalId.ToString();
        }
        else
        {
            referenceGlobalId = string.Empty;
        }
        #endif
    }
    
    [ContextMenu("ResetGlobalId")]
    public void ResetGlobalId()
    {
        referenceGlobalId = string.Empty;
    }

    [ContextMenu("Load")]
    public void LoadReferenceFromGlobalId()
    {
        #if UNITY_EDITOR
        if (UnityEditor.GlobalObjectId.TryParse(referenceGlobalId, out var globalId))
        {
            loadedReference = UnityEditor.GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalId);
        }
        else
        {
            loadedReference = null;
        }
        #endif
    }
    
    [ContextMenu("Reset")]
    public void ResetLoadedReference()
    {
        loadedReference = null;
    }
}
