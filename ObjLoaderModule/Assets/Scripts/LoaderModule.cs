using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class LoaderModule : MonoBehaviour
{
    public event Action<GameObject> OnLoadCompleted;

    List<Vector3> vertices = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<Vector3> normals = new List<Vector3>();
    List<int> triangles = new List<int>();

    public void LoadAsset(string path)
    {
        List<int> normalIndices = new List<int>();
        List<int> uvIndices = new List<int>();

        Mesh mesh = new Mesh();
        ReadObjFile(path);

        AssignToMesh(mesh, vertices, uvs, normals, triangles);

        GameObject meshObject = CreateMeshGameObject(mesh);
        OnLoadCompleted?.Invoke(meshObject);
    }

    public async Task<GameObject> LoadAssetAsync(string path)
    {
        Mesh mesh = new Mesh();
        //await ReadObjFileAsync(path);
        await Task.Run(() => ReadObjFile(path));

        AssignToMesh(mesh, vertices, uvs, normals, triangles);

        GameObject meshObject = CreateMeshGameObject(mesh);

        return meshObject;
    }

    public void ReadObjFile(string filePath)
    {
        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                ProcessLine(line, vertices, uvs, normals, triangles);
            }
        }
    }

    public async Task ReadObjFileAsync(string filePath)
    {
        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                ProcessLine(line, vertices, uvs, normals, triangles);
            }
        }
    }


    private void ProcessLine(string line, List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, List<int> triangles)
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

    }

    private GameObject CreateMeshGameObject(Mesh mesh)
    {
        GameObject meshObject = new GameObject("Model");
        MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();

        meshFilter.mesh = mesh;
        meshRenderer.material = new Material(Shader.Find("Standard"));

        vertices.Clear();
        uvs.Clear();
        normals.Clear();
        triangles.Clear();

        return meshObject;
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
}
