using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetCollider : MonoBehaviour
{
    public GameObject Sphere;

    public Vector3[] sVertices = new Vector3[] { };
    Vector3[] sNormals = new Vector3[] { };
    int[] sTriangles = new int[] { };
    List<int[]> sVerticesTriangle = new List<int[]>() { };

    private void Start()
    {
        makeSphere();
    }
    void makeSphere()
    {
        Mesh spheremesh = new Mesh();
        Sphere.transform.localScale *= 0.5f;
        spheremesh = Sphere.transform.GetComponent<MeshFilter>().mesh;

        sVertices = spheremesh.vertices;
        for (int i = 0; i < sVertices.Length; i++)
        {
            sVertices[i] = Sphere.transform.TransformPoint(spheremesh.vertices[i]);
            //sVertices[i] += Sphere.transform.position;
        }
        sTriangles = spheremesh.triangles;
        sNormals = spheremesh.normals;
        for (int j = 0; j < sVertices.Length; j++)
        {
            List<int> ind = new List<int>();

            for (int k = 0; k < sTriangles.Length; k += 3)
            {
                if (j == sTriangles[k])
                {
                    ind.Add(sTriangles[k + 1]);
                    ind.Add(sTriangles[k + 2]);

                }
                if (j == sTriangles[k + 1])
                {
                    ind.Add(sTriangles[k]);
                    ind.Add(sTriangles[k + 2]);
                }
                if (j == sTriangles[k + 2])
                {
                    ind.Add(sTriangles[k + 1]);
                    ind.Add(sTriangles[k]);
                }
            }

            sVerticesTriangle.Add(ind.ToArray());
        }
    }

}
