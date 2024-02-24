using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class AssetLoaderAsync : MonoBehaviour
{
    [field: SerializeField]
    public LoaderModule loaderModule;

    private async void Start()
    {
        string selectedAssetName = EditorUtility.OpenFilePanel("Select obj model", "", "obj");
        await Load(selectedAssetName);
    }

    public async Task Load(string assetName)
    {
        GameObject loadedAsset = await loaderModule.LoadAssetAsync(assetName);
        loadedAsset.transform.SetParent(transform);
    }
}
