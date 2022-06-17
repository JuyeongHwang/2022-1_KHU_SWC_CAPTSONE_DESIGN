using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class meshGenerator : MonoBehaviour
{
    public static meshGenerator instance;

    [Header("Mesh Info")]
    [SerializeField] private Material m_material = null;
    public List<Vector3> vertices = new List<Vector3>() { };
    public List<int> triangles = new List<int>() { };
    public List<Vector3> fabricNormals = new List<Vector3>() { };
    List<Vector2> uvs = new List<Vector2>() { };
    List<int> VerticesIndex = new List<int>() { };
    public List<int[]> includeTri = new List<int[]>();
    private void Awake()
    {
        meshGenerator.instance = this;
    }

    Mesh mesh;
    public void meshGen(int width, int space, List<Vector3> Positions)
    {
        int col = 0;
        int row = 0;
        int maxLen = width * (space);
        int iter = 0;
        Vector3 normals = new Vector3(0, 0, -1);
        for (int v0 = 0; v0 < Positions.Count; v0++)
        {
            col = v0 % maxLen;
            row = v0 / maxLen;

            List<int> ind = new List<int>();
            if (row <= maxLen - 2 && col <= maxLen - 2)
            {
                //int j = nRow * (lastIndex + 1) + nCol;
                int v1 = (row) * maxLen + col + 1;
                int v2 = (row + 1) * maxLen + (col + 1);
                int v3 = (row + 1) * maxLen + (col);

                vertices.Add(Positions[v0]);
                vertices.Add(Positions[v1]);
                vertices.Add(Positions[v2]);
                vertices.Add(Positions[v3]);

                VerticesIndex.Add(v0);
                VerticesIndex.Add(v1);
                VerticesIndex.Add(v2);
                VerticesIndex.Add(v3);

                triangles.Add(0 + iter * 4);
                triangles.Add(1 + iter * 4);
                triangles.Add(2 + iter * 4);

                triangles.Add(0 + iter * 4);
                triangles.Add(2 + iter * 4);
                triangles.Add(3 + iter * 4);

                ind.Add(v1);
                ind.Add(v2);
                ind.Add(v2);
                ind.Add(v3);

                uvs.Add(new Vector2(0, 0.5f));
                uvs.Add(new Vector2(0.5f, 0.5f));
                uvs.Add(new Vector2(0.5f, 0));
                uvs.Add(new Vector2(0, 0));

                //fabricNormals.Add(normals);
                //fabricNormals.Add(normals);
                //fabricNormals.Add(normals);
                //fabricNormals.Add(normals);
                //v0
                Vector3 side1 = Positions[v1] - Positions[v0];
                Vector3 side2 = Positions[v2] - Positions[v0];

                normals = Vector3.Cross(side1, side2);
                side1 = Positions[v2] - Positions[v0];
                side2 = Positions[v3] - Positions[v0];
                normals += Vector3.Cross(side1, side2);
                normals /= 2.0f;

                fabricNormals.Add(normals);

                //v1
                side1 = Positions[v2] - Positions[v1];
                side2 = Positions[v0] - Positions[v1];
                normals = Vector3.Cross(side1, side2);
                fabricNormals.Add(normals);

                //v2
                side1 = Positions[v0] - Positions[v2];
                side2 = Positions[v1] - Positions[v2];
                normals = Vector3.Cross(side1, side2);
                side1 = Positions[v3] - Positions[v2];
                side2 = Positions[v0] - Positions[v2];
                normals += Vector3.Cross(side1, side2);
                normals /= 2.0f;
                fabricNormals.Add(normals);

                //v3
                side1 = Positions[v0] - Positions[v3];
                side2 = Positions[v2] - Positions[v3];
                normals = Vector3.Cross(side1, side2);
                fabricNormals.Add(normals);

                iter++;
            }
            includeTri.Add(ind.ToArray());
            ind.Clear();
        }

        mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = fabricNormals.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateBounds();
        //mesh.RecalculateNormals();
        //normals = mesh.normals;
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = m_material;

    }

    public void changeMesh(int width, int space, List<Vector3>Positions)
    {
        int maxLen = width * (space);
        Vector3 normals = Vector3.zero;

        for (int v0 = 0; v0 < VerticesIndex.Count; v0++)
        {
            vertices[v0] = Positions[VerticesIndex[v0]];
        }
        mesh.vertices = vertices.ToArray();
        //mesh.RecalculateNormals();
        //mesh.normals = fabricNormals.ToArray();
    }
}
