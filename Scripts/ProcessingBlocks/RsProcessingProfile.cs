using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class RsProcessingProfile : ScriptableObject, IEnumerable<RsProcessingBlock>
{
    // [HideInInspector]
    [SerializeField]
    public List<RsProcessingBlock> _processingBlocks = new List<RsProcessingBlock>();

    public IEnumerator<RsProcessingBlock> GetEnumerator()
    {
        return _processingBlocks.GetEnumerator() as IEnumerator<RsProcessingBlock>;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _processingBlocks.GetEnumerator();
    }


#if UNITY_EDITOR
void Reset()
    {
        // Initialize the serialized object and update it
        var obj = new UnityEditor.SerializedObject(this);
        obj.Update();

        // Get the property for the _processingBlocks array
        var blocks = obj.FindProperty("_processingBlocks");
        blocks.ClearArray(); // Clear existing references

        // Load all assets at the current path and add them to _processingBlocks
        var assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
        var assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(assetPath);
        
        foreach (var asset in assets)
        {
            if (asset == this) continue;

            int i = blocks.arraySize++;
            var element = blocks.GetArrayElementAtIndex(i);
            element.objectReferenceValue = asset;
        }

        // Apply the modified properties and save the asset
        obj.ApplyModifiedProperties();
        UnityEditor.AssetDatabase.SaveAssets();
    }
#endif

}
