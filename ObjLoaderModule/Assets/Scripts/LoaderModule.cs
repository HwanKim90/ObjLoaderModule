using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LoaderModule : MonoBehaviour
{
    public event Action<GameObject> OnLoadCompleted;

    private List<Vector3> vertices = new List<Vector3>();
    private List<Vector2> textures = new List<Vector2>();
    private List<Vector3> normals = new List<Vector3>();

    private List<int> triangles = new List<int>();

    public void ReadObjFile(string filePath)
    {
        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("v "))
                {
                    Vector3 vertex = ParseVector3(line.Substring(2));
                    vertices.Add(vertex);

                }
                else if (line.StartsWith("vt "))
                {
                    Vector2 uv = ParseVector2(line.Substring(3));
                    textures.Add(uv);
                }
                else if (line.StartsWith("vn "))
                {
                    Vector3 normal = ParseVector3(line.Substring(3));
                    normals.Add(normal);
                }
                else if (line.StartsWith("f "))
                {
                    string[] parts = line.Substring(2).Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 1; i < parts.Length - 1; i++)
                    {
                        // 첫 번째 정점 인덱스 (OBJ 인덱스는 1부터 시작하므로 1을 빼서 Unity 인덱스로 변환)
                        triangles.Add(int.Parse(parts[0].Split('/')[0]) - 1);
                        // 현재 정점 인덱스
                        triangles.Add(int.Parse(parts[i].Split('/')[0]) - 1);
                        // 다음 정점 인덱스
                        triangles.Add(int.Parse(parts[i + 1].Split('/')[0]) - 1);
                    }
                }
            }
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

    public void LoadAsset(string path)
    {
        ReadObjFile(path);

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray(); 

        Vector2[] uv = new Vector2[vertices.Count];
        for (int i = 0; i < uv.Length; i++)
        {
            if (i < textures.Count)
            {
                uv[i] = textures[i];
            }
            else
            {
                uv[i] = Vector2.zero;
            }
        }
        mesh.uv = uv; 

        Vector3[] normal = new Vector3[vertices.Count];
        for (int i = 0; i < normal.Length; i++)
        {
            if (i < normals.Count)
            {
                normal[i] = normals[i];
            }
            else
            {
                normal[i] = Vector3.zero;
            }
        }
        mesh.normals = normal;

        mesh.triangles = triangles.ToArray(); 
       
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        GameObject meshObject = new GameObject("Model");
        MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter.mesh = mesh;
        meshRenderer.material = new Material(Shader.Find("Standard")); // 기본 재질 할당

        OnLoadCompleted?.Invoke(meshObject);
    }


}
