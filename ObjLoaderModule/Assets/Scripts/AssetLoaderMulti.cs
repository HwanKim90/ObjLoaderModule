using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class AssetLoaderMulti : MonoBehaviour
{
    [SerializeField]
    public LoaderModuleMulti loaderModule;

    private async void Start()
    {
        string currentDir = Directory.GetCurrentDirectory();
        string modelDirPath = Path.Combine(currentDir, "Assets", "Models");
        string[] objFiles = Directory.GetFiles(modelDirPath, "*.obj");

        await loaderModule.LoadAssetsAsync(objFiles);
    }
}
