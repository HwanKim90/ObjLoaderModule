using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class AssetLoaderAsync : MonoBehaviour
{
    [SerializeField]
    public LoaderModule loaderModule;

    private async void Start()
    {
        string selectedAssetName = EditorUtility.OpenFilePanel("Select obj model", "", "obj");

        await Load(selectedAssetName);

        //for (int i = 0; i < 20; i++)
        //{
        //    await Load(selectedAssetName);
        //}
    }

    public async Task Load(string assetName)
    {
        GameObject loadedAsset = await loaderModule.LoadAssetAsync(assetName);
        loadedAsset.transform.SetParent(transform);
    }
}
