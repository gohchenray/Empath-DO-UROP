using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class RsProcessingProfile_edit : ScriptableObject, IEnumerable<RsProcessingBlock>
{
    // [HideInInspector]
    [SerializeField]
    public List<RsProcessingBlock> _processingBlocks;

    public IEnumerator<RsProcessingBlock> GetEnumerator()
    {
        return _processingBlocks.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _processingBlocks.GetEnumerator();
    }

public void AddDepthCutoffBlock()
    {
        DepthCutoff depthCutoffBlock = ScriptableObject.CreateInstance<DepthCutoff>();
        depthCutoffBlock.Distance = 1000; // Example of setting a specific value
        _processingBlocks.Add(depthCutoffBlock); // Add the block to the list
    }

#if UNITY_EDITOR
    // Ensure the DepthCutoff block is added during reset
void Reset()
    {
        var obj = new UnityEditor.SerializedObject(this);
        obj.Update();

        if (_processingBlocks == null)
            _processingBlocks = new List<RsProcessingBlock>();

        // Add the DepthCutoff block to the list
        AddDepthCutoffBlock();
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
