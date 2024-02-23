using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AssetLoader : MonoBehaviour
{
    [SerializeField]
    public LoaderModule loaderModule;

    private void Start()
    {
        string selectedAssetName = EditorUtility.OpenFilePanel("Select obj model", "", "obj");
        Load(selectedAssetName);
        //loaderModule.ReadObjFile(selectedAssetName);
    }

    public void Load(string assetName)
    {
        loaderModule.OnLoadCompleted += OnLoadCompleted;
        loaderModule.LoadAsset(assetName);
    }

    private void OnLoadCompleted(GameObject loadedAsset)
    {
        loadedAsset.transform.SetParent(transform);
        // To do
    }
}