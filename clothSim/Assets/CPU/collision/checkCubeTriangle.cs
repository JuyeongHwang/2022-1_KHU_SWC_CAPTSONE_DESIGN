using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkCubeTriangle : MonoBehaviour
{
    public GameObject Cube;
    Vector3 p = new Vector3(0.0f, -5.5f,0.5f);

    public Vector3 A = new Vector3(1, 1, 0);
    public Vector3 B = new Vector3(-1, -1, 0);
    public Vector3 C = new Vector3(3, -1, 0);
    public List<Vector3> fabricVertices = new List<Vector3>();
    Vector3 norm;
    public Vector3 planePos = Vector3.zero;

    private void Start()
    {
        makeCube();

    }
    public Vector3[] cVertices = new Vector3[] { };
    Vector3[] cNormals = new Vector3[] { };
    int[] cTriangles = new int[] { };
    List<int[]> cVerticesTriangle = new List<int[]>() { };
    public List<int> cEdges = new List<int>();
    void makeCube()
    {
        Mesh cubemesh = new Mesh();
        //Cube.transform.localScale *= 1f;
        cubemesh = Cube.transform.GetComponent<MeshFilter>().sharedMesh;

        cVertices = cubemesh.vertices;
        for (int i = 0; i < cVertices.Length; i++)
        {
            cVertices[i] += Cube.transform.position;
        }
        cTriangles = cubemesh.triangles;
        cNormals = cubemesh.normals;
    }


    float h = 0.1f;
    bool collision = false;
    public bool inner = false;

    private void Update()
    {
        for (int j = 0; j < cTriangles.Length; j += 3)
        {
            A = cVertices[cTriangles[0 + j]];
            B = cVertices[cTriangles[1 + j]];
            C = cVertices[cTriangles[2 + j]];

            //1. distance, norm 구하기
            norm = Vector3.Cross(A - B, C - B);
            norm /= norm.magnitude;
            float distance = Mathf.Abs(Vector3.Dot(p - C, norm));

            // 삼각형과 가장 가까운 지점 구하기
            float a = Vector3.Dot(B - C, B - C);
            float b = Vector3.Dot(B - C, A - C);
            float c = Vector3.Dot(B - C, A - C);
            float d = Vector3.Dot(A - C, A - C);

            float e = Vector3.Dot(B - C, p - C);
            float f = Vector3.Dot(A - C, p - C);

            float w1 = (d * e - b * f) / (a * d - b * c);
            float w2 = (a * f - c * e) / (a * d - b * c);
            float w3 = 1 - w2 - w1;
            planePos = w1 * B + w2 * A + w3 * C;

            //가장 가까운 planePos가 해당 큐브 삼각형 위에 있는지 확인
            Vector3 inner1, inner2, inner3;
            inner1 = Vector3.Cross(planePos - A, B - A);
            inner2 = Vector3.Cross(planePos - B, C - B);
            inner3 = Vector3.Cross(planePos - C, A - C);
            if (distance < 0.01f)
            {
                if (inner1.x <= 0 && inner1.y <= 0 && inner1.z <= 0)
                {
                    if (inner2.x <= 0 && inner2.y <= 0 && inner2.z <= 0)
                    {
                        if (inner3.x <= 0 && inner3.y <= 0 && inner3.z <= 0)
                        {
                            Vector3 pp = p - planePos;
                            float triangleLen = 5.0f;
                            if (pp.x >= -h / triangleLen && pp.x <= h / triangleLen)
                            {
                                if (pp.y >= -h / triangleLen && pp.y <= h / triangleLen)
                                {
                                    if (pp.z >= -h / triangleLen && pp.z <= h / triangleLen)
                                    {
                                        collision = true;

                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                if (inner1.x >= 0 && inner1.y >= 0 && inner1.z >= 0)
                {
                    if (inner2.x >= 0 && inner2.y >= 0 && inner2.z >= 0)
                    {
                        if (inner3.x >= 0 && inner3.y >= 0 && inner3.z >= 0)
                        {
                            
                            Vector3 pp = p - planePos;
                            float triangleLen = 5.0f;
                            if (pp.x >= -h / triangleLen && pp.x <= h / triangleLen)
                            {
                                if (pp.y >= -h / triangleLen && pp.y <= h / triangleLen)
                                {
                                    if (pp.z >= -h / triangleLen && pp.z <= h / triangleLen)
                                    {
                                        collision = true;

                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (collision == false)
            {
                p += Vector3.up * 0.1f * Time.deltaTime;
            }

        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(B, 0.051f);//B
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(A, 0.051f);//A
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(C, 0.051f);//C

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(p, 0.021f);


        Gizmos.color = Color.white;
        Gizmos.DrawSphere(planePos, 0.021f);

    }
}
