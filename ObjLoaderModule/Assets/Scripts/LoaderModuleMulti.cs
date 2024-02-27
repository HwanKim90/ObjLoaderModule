using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class LoaderModuleMulti : MonoBehaviour
{
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