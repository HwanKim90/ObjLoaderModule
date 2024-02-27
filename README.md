# ObjLoaderModuler 
- 유니티 버전: 2022.3.0f1
- 문제별로 Scene을 만들어 놓았습니다. ex) 문제1 -> Question1 Scene 
 

## 1. LoaderModule class 리뷰

``` C#
public void LoadAsset(string path)
{
    Mesh mesh = new Mesh();

    try
    {
        ReadObjFile(path);
        AssignToMesh(mesh, vertices, uvs, normals, triangles);

        GameObject meshObject = CreateMeshGameObject(mesh);
        OnLoadCompleted?.Invoke(meshObject);

    }
    catch (Exception e)
    {
        Debug.LogException(e);
    }
}
```
obj파일 데이터로 메시를 생성하여 게임 오브젝트를 만드는 메소드
파일을 읽는 과정에서 발생할 수 있는 예외(예: 파일이 존재하지 않음, 읽을 권한 없음 등)에 대한 처리를 try-catch문으로 처리

``` C#
private void ReadObjFile(string filePath)
{
    using (StreamReader reader = new StreamReader(filePath))
    {
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            if (line.StartsWith("v "))
            {
                vertices.Add(ParseVector3(line.Substring(2)));
            }
            else if (line.StartsWith("vt "))
            {
                uvs.Add(ParseVector2(line.Substring(3)));
            }
            else if (line.StartsWith("vn "))
            {
                normals.Add(ParseVector3(line.Substring(3)));
            }
            else if (line.StartsWith("f "))
            {
                ProcessFaceLine(line, triangles);
            }
        }
    }
}

private void ProcessFaceLine(string line, List<int> triangles)
{
    string[] parts = line.Substring(2).Split(' ', StringSplitOptions.RemoveEmptyEntries);

    for (int i = 1; i < parts.Length - 1; i++)
    {
        int v1 = int.Parse(parts[0].Split('/')[0]) - 1;
        int v2 = int.Parse(parts[i].Split('/')[0]) - 1;
        int v3 = int.Parse(parts[i + 1].Split('/')[0]) - 1;

        triangles.Add(v1);
        triangles.Add(v2);
        triangles.Add(v3);
    }
}

private Vector3 ParseVector3(string line)
{
    string[] values = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    float x = float.Parse(values[0]);
    float y = float.Parse(values[1]);
    float z = float.Parse(values[2]);

    return new Vector3(x, y, z);
}

private Vector2 ParseVector2(string line)
{
    string[] values = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    float u = float.Parse(values[0]);
    float v = float.Parse(values[1]);

    return new Vector2(u, v);
}
```
- ReadObjFile함수: obj 파일을 한 줄씩 읽으면서 시작하는 문자에 따라 버텍스, UV, 노멀, 페이스을 판별, 해당 데이터를 파싱하여 리스트에 추가
- ProcessFaceLine함수: 페이스 정보를 파싱하여 트라이앵글 리스트에 추가합니다. 페이스는 3개 이상의 버텍스로 구성될 수 있으며, 이 함수는 각 페이스를 구성하는 버텍스의 인덱스를 추출하여 트라이앵글로 변환
- obj 파일의 인덱스는 1부터 시작하기때문에, 배열 인덱스에서 1을 빼주면서 조정

``` C#
private void AssignToMesh(Mesh mesh, List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, List<int> triangles)
{
    Vector2[] uv = new Vector2[vertices.Count];
    Vector3[] normal = new Vector3[vertices.Count];

    for (int i = 0; i < vertices.Count; i++)
    {
        uv[i] = (i < uvs.Count) ? uvs[i] : Vector2.zero;
        normal[i] = (i < normals.Count) ? normals[i] : new Vector3(0, 0, 1);
    }

    mesh.SetVertices(vertices);
    mesh.SetUVs(0, uv);
    mesh.SetNormals(normal);
    mesh.SetTriangles(triangles, 0);

    mesh.RecalculateBounds();
    mesh.RecalculateNormals();

    vertices.Clear();
    uvs.Clear();
    normals.Clear();
    triangles.Clear();
}
```
- 각 버텍스에 대응하는 UV와 노멀을 설정합니다. 만약 UVs 또는 노멀의 수가 버텍스의 수보다 적은 경우, 기본값 (Vector2.zero 또는 new Vector3(0, 0, 1))을 할당
- 모든 버텍스에 UVs나 노멀이 정의되어 있지 않을 때 기본값을 사용하는 것이 적합하지는 않지만, 이번 코드 문제에 한해서는 괜찮다고 판단하여 기본값으로 대체함.


## 2. LoaderModule 비동기 변경 구현 리뷰

``` C#
public async Task<GameObject> LoadAssetAsync(string path)
{
    Mesh mesh = new Mesh();
    
    try
    {
        await Task.Run(() => ReadObjFile(path));

        AssignToMesh(mesh, vertices, uvs, normals, triangles);

        GameObject meshObject = CreateMeshGameObject(mesh);
        return meshObject;
    }
    catch (Exception e)
    {
        Debug.LogException(e);
        return null;
    }
}
```
- await Task.Run(() => ReadObjFile(path))를 사용하여 파일 읽기 작업을 백그라운드 스레드에서 실행합니다. 메인 스레드를 차단하지 않고, 파일 로딩 작업을 수행하므로 애플리케이션의 반응성 향상과 효율적인 리소스 사용을 하여 성능 최적화에 이점을 줌.
- 가독성 측면에서도 async와 await 키워드를 사용할때, 비동기 코드의 가독성이 크게 향상됩니다. 코드의 흐름도 직관적으로 이해할 수 있음.


## 3. 동시성 제어 구현 리뷰

``` C#
public async Task Load(string assetName)
{
    GameObject loadedAsset = await loaderModule.LoadAssetAsync(assetName);
    loadedAsset.transform.SetParent(transform);
}

private async Task MultiLoad()
{
    string currentDir = Directory.GetCurrentDirectory();
    string modelDirPath = Path.Combine(currentDir, "Assets", "Models");
    string[] objFiles = Directory.GetFiles(modelDirPath, "*.obj");

    Task[] loadTasks = objFiles.Select(objFile => Load(objFile)).ToArray();
    await Task.WhenAll(loadTasks);
}
```
- obj 파일 20개를 로드할때 Failed setting triangles. Some indices are referencing out of bounds vertices에러, IndexOutOfRangeException: Index was outside the bounds of the array 에러 등등 발생. 
- LoadAssetAsync 메서드에서 여러 OBJ 파일을 동시에 비동기적으로 로딩할 때, 각 파일의 데이터(vertices, uvs, normals, triangles)를 별도로 처리해야 함에도 불구하고, 이 데이터가 공유되거나 올바르게 분리되지 않으면 동시성 문제가 발생함. 
- 여러 파일을 동시에 처리하려고 할 때 각각의 파일 처리 작업에서 서로 다른 파일의 데이터에 접근하거나 잘못된 데이터를 참조하게됨.

``` C#
public async Task LoadAssetsAsync(string[] paths)
{
    List<Task<GameObject>> loadTasks = new List<Task<GameObject>>();

    foreach (var path in paths)
    {
        loadTasks.Add(LoadAssetAsync(path));
    }

    await Task.WhenAll(loadTasks);
}

private async Task<GameObject> LoadAssetAsync(string path)
{
    Mesh mesh = new Mesh();
    List<Vector3> vertices = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<Vector3> normals = new List<Vector3>();
    List<int> triangles = new List<int>();

    try
    {
        await ReadObjFileAsync(path, vertices, uvs, normals, triangles);

        AssignToMesh(mesh, vertices, uvs, normals, triangles);

        GameObject meshObject = CreateMeshGameObject(mesh);
        return meshObject;
    }
    catch (Exception e)
    {
        Debug.LogException(e);
        return null;
    }
}

private async Task ReadObjFileAsync(string filePath, List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, List<int> triangles)
{
    using (StreamReader reader = new StreamReader(filePath))
    {
        string line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (line.StartsWith("v "))
            {
                vertices.Add(ParseVector3(line.Substring(2)));
            }
            else if (line.StartsWith("vt "))
            {
                uvs.Add(ParseVector2(line.Substring(3)));
            }
            else if (line.StartsWith("vn "))
            {
                normals.Add(ParseVector3(line.Substring(3)));
            }
            else if (line.StartsWith("f "))
            {
                ProcessFaceLine(line, triangles);
            }
        }
    }
}
```
- 각각의 LoadAssetAsync함수가 자신만의 로컬 상태(즉, 각 파일의 vertices, uvs, normals, triangles 리스트)를 가지고 있도록 함으로써, 동시에 여러 파일을 로딩하더라도 각 작업이 서로에게 영향을 주지 않도록 함. 이는 각각의 비동기 로딩 작업이 독립적으로 파일을 처리할 수 있게 하여, 동시성 문제를 해결할 수 있음
- TaskRun함수에서 StreamReader의 ReadLineAsync함수 사용으로 변경, 비동기 I/O 작업의 효율성과 스레드 관리 측면에서 대규모 파일 로딩에서애플리케이션의 응답성을 유지하고, 시스템 자원을 더 효율적으로 사용할 수 있기때문


