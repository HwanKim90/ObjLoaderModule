using System.IO;
using UnityEditor;
using UnityEngine;

public class AssetLoader : MonoBehaviour
{
    [SerializeField]
    public LoaderModule loaderModule;

    private void Start()
    {
        string currentDir = Directory.GetCurrentDirectory();
        string modelDirPath = Path.Combine(currentDir, "Assets", "Models");
        string selectedAssetName = EditorUtility.OpenFilePanel("Select obj model", modelDirPath, "obj");

        Load(selectedAssetName);
    }

    public void Load(string assetName)
    {
        loaderModule.OnLoadCompleted += OnLoadCompleted;
        loaderModule.LoadAsset(assetName);
    }

    private void OnLoadCompleted(GameObject loadedAsset)
    {
        loadedAsset.transform.SetParent(transform);
    }
}
