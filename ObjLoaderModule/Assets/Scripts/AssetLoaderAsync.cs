using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class AssetLoaderAsync : MonoBehaviour
{
    [SerializeField]
    public LoaderModule loaderModule;

    private async void Start()
    {
        string currentDir = Directory.GetCurrentDirectory();
        string modelDirPath = Path.Combine(currentDir, "Assets", "Models");

        string selectedAssetName = EditorUtility.OpenFilePanel("Select obj model", modelDirPath, "obj");

        await Load(selectedAssetName);

        //await MultiLoad();
    }

    public async Task Load(string assetName)
    {
        GameObject loadedAsset = await loaderModule.LoadAssetAsync(assetName);
        loadedAsset.transform.SetParent(transform);
    }

    //private async Task MultiLoad()
    //{
    //    string currentDir = Directory.GetCurrentDirectory();
    //    string modelDirPath = Path.Combine(currentDir, "Assets", "Models");
    //    string[] objFiles = Directory.GetFiles(modelDirPath, "*.obj");

    //    var loadTasks = objFiles.Select(objFile => Load(objFile)).ToArray();
    //    await Task.WhenAll(loadTasks);
    //}
}
