using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collision : MonoBehaviour
{

    public static collision instance;

    public GameObject Sphere;


    private void Awake()
    {
        collision.instance = this;
    }

    public bool useSphere;
    private void Start()
    {
        if (useSphere)
        {
            makeSphere();
        }

    }

    public Vector3 clothclothCollide2(int curi, Vector3 newPos, List<Vector3> Positions, float spacing)
    {
        for (int j = 0; j < Positions.Count; j++)
        {
            if (curi == j) { continue; }
            float distance = (newPos - Positions[j]).magnitude;
            if (distance < spacing / 1.5f)
            {
                Vector3 dir = Positions[j] - newPos;
                newPos -= dir.normalized * (1.0f - (spacing / 1.5f - distance)) * 0.000051f;
            }
        }
        return newPos;
    }


    public Vector3[] sVertices = new Vector3[] { };
    Vector3[] sNormals = new Vector3[] { };
    int[] sTriangles = new int[] { };
    List<int[]> sVerticesTriangle = new List<int[]>() { };
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


    public float sphereDetect = 0.18f;
    public Vector3 clothsphereCollide(int curi, Vector3 newPos)
    {

        for (int j = 0; j < sVertices.Length; j++)
        {
            float distance = (newPos - sVertices[j]).magnitude;
            if (distance < sphereDetect)
            {
                Vector3 dir = sNormals[j];// sVertices[j] - newPos;
                newPos += dir.normalized * (sphereDetect - distance) * 1f;
                
            }
        }

        return newPos;
    }


  
}
