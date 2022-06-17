using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    public GameObject Cube;
    Vector3 p = new Vector3(1.5f, -0.5f, 6);
    Vector3 p2 = new Vector3(2.0f, -0.2f, 6);
    Vector3 p3 = new Vector3(1.4f, 0.2f, 6);

    public Vector3 A = new Vector3(1, 1, 0);
    public Vector3 B = new Vector3(-1, -1, 0);
    public Vector3 C = new Vector3(3, -1, 0);
    public List<Vector3> fabricVertices = new List<Vector3>();
    Vector3 norm;
    public Vector3 planePos = Vector3.zero;
    Vector3 edgePos = Vector3.zero;
    Vector3 edgePos2 = Vector3.zero;
    Vector3 edgePos3 = Vector3.zero;
    Vector3 edgePos4 = Vector3.zero;


    public Vector3[] cVertices = new Vector3[] { };
    Vector3[] cNormals = new Vector3[] { };
    int[] cTriangles = new int[] { };
    public List<int> edges = new List<int>();
    void Start()
    {
        makeCube();
        
        fabricVertices.Add(p);
        fabricVertices.Add(p2);
        fabricVertices.Add(p3);

        edges.Add(0);
        edges.Add(1);
        edges.Add(0);
        edges.Add(2);
        edges.Add(0);
        edges.Add(6);

        edges.Add(3);
        edges.Add(1);
        edges.Add(3);
        edges.Add(2);
        edges.Add(3);
        edges.Add(5);

        edges.Add(4);
        edges.Add(2);
        edges.Add(4);
        edges.Add(6);
        edges.Add(4);
        edges.Add(5);

        edges.Add(7);
        edges.Add(5);
        edges.Add(7);
        edges.Add(6);
        edges.Add(7);
        edges.Add(1);

    }

    void makeCube()
    {
        Mesh cubemesh = new Mesh();
        //Cube.transform.localScale *= 1f;
        cubemesh = Cube.transform.GetComponent<MeshFilter>().sharedMesh;

        cVertices = cubemesh.vertices;
        for (int i = 0; i < cVertices.Length; i++)
        {
            cVertices[i] += Cube.transform.position;
            //cVertices[i] = Cube.transform.TransformPoint(cubemesh.vertices[i]);
        }
        cTriangles = cubemesh.triangles;
        cNormals = cubemesh.normals;
    }

    float h = 0.1f;
    bool collision = false;
    public bool inner = false;
    // Update is called once per frame
    void Update()
    {
        int collisionIndex1=0, collisionIndex2=0, collisionIndex3 = 0;
        for(int j = 0; j<cTriangles.Length; j += 3)
        {
            A = cVertices[cTriangles[1 + j]];
            B = cVertices[cTriangles[0 + j]];
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
            if (distance < 0.5f)
            {
                if (inner1.x >= 0 && inner1.y >= 0 && inner1.z >= 0)
                {
                    if (inner2.x >= 0 && inner2.y >= 0 && inner2.z >= 0)
                    {
                        if (inner3.x >= 0 && inner3.y >= 0 && inner3.z >= 0)
                        {
                            inner = true;
                        }
                    }
                }
            }

            Vector3 pp = p - planePos;
            if (inner)
            {
                float triangleLen = 5.0f;
                if (pp.x >= -h / triangleLen && pp.x <= h / triangleLen)
                {
                    if (pp.y >= -h / triangleLen && pp.y <= h / triangleLen)
                    {
                        if (pp.z >= -h / triangleLen && pp.z <= h / triangleLen)
                        {
                            collision = true;
                            collisionIndex1 = cTriangles[0 + j];
                            collisionIndex2 = cTriangles[1 + j];
                            collisionIndex3 = cTriangles[2 + j];
                            break;
                        }
                    }
                }
            }

            if (collision == false)
            {
                p -= Vector3.forward * 0.1f * Time.deltaTime;
                p2 -= Vector3.forward * 0.1f * Time.deltaTime;
                p3 -= Vector3.forward * 0.1f * Time.deltaTime;
            }

        }

        //1.
        //x1,x2 -> cube A,B,C
        //x3,x4 -> fabric p, p2
        
        for(int i = 0; i<edges.Count; i += 2)
        {
            //충돌한 삼각형의 edge가 큐브 모서리인지 확인.
            if (edges[i] == collisionIndex1 || edges[i] == collisionIndex2)
            {
                if (edges[i + 1] == collisionIndex1 || edges[i + 1] == collisionIndex2)
                {

                    //edge 위의 점 구하기
                    int index1 = edges[i + 1];
                    int index2 = edges[i];
                    float aa = Vector3.Dot(cVertices[index1] - cVertices[index2], cVertices[index1] - cVertices[index2]);
                    float bb = Vector3.Dot(-(cVertices[index1] - cVertices[index2]), p2 - p);
                    float cc = Vector3.Dot(-(cVertices[index1] - cVertices[index2]), p2 - p);
                    float dd = Vector3.Dot(p2 - p, p2 - p);
                    float ee = Vector3.Dot(cVertices[index1] - cVertices[index2], p - cVertices[index2]);
                    float ff = Vector3.Dot(-(p2 - p), p - cVertices[index2]);

                    float Aa = (dd * ee - bb * ff) / (aa * dd - bb * cc);
                    float Bb = (aa * ff - cc * ee) / (aa * dd - bb * cc);

                    edgePos = cVertices[index2] + Aa * (cVertices[index1] - cVertices[index2]);
                    edgePos2 = p + Bb * (p2 - p);

                    //fabric 삼각형 위에 edge가 있는지.
                    Vector3 cinner1, cinner2, cinner3;
                    cinner1 = Vector3.Cross(edgePos2 - p2, p - p2);
                    cinner2 = Vector3.Cross(edgePos2 - p, p3 - p);
                    cinner3 = Vector3.Cross(edgePos2 - p3, p2 - p3);
                    int count = 0;
                    if (cinner1.x >= 0 && cinner1.y >= 0 && cinner1.z >= 0)
                    {
                        count++;
                        if (cinner2.x >= 0 && cinner2.y >= 0 && cinner2.z >= 0)
                        {
                            count++;
                            if (cinner3.x >= 0 && cinner3.y >= 0 && cinner3.z >= 0)
                            {
                                count++;
                            }
                        }
                    }

                    if (Mathf.Abs(edgePos.x - edgePos2.x) < 0.05f && Mathf.Abs(edgePos.y - edgePos2.y) < 0.05f
                        && Mathf.Abs(edgePos.z - edgePos2.z) < 0.05f)
                    {
                        //삼각형 ABC 변 위에 있다면
                        if (count >= 3)
                        {
                            collision = true;
                            //1. edgePos로 옮기기

                            //p = edgePos;

                            //2. 나누기 
                        }

                    }
                    
                }
            }

            if (edges[i] == collisionIndex1 || edges[i] == collisionIndex2)
            {
                if (edges[i + 1] == collisionIndex1 || edges[i + 1] == collisionIndex2)
                {

                    //edge 위의 점 구하기
                    int index1 = edges[i + 1];
                    int index2 = edges[i];
                    float aa = Vector3.Dot(cVertices[index1] - cVertices[index2], cVertices[index1] - cVertices[index2]);
                    float bb = Vector3.Dot(-(cVertices[index1] - cVertices[index2]), p3 - p2);
                    float cc = Vector3.Dot(-(cVertices[index1] - cVertices[index2]), p3 - p2);
                    float dd = Vector3.Dot(p3 - p2, p3 - p2);
                    float ee = Vector3.Dot(cVertices[index1] - cVertices[index2], p2 - cVertices[index2]);
                    float ff = Vector3.Dot(-(p3 - p2), p2 - cVertices[index2]);

                    float Aa = (dd * ee - bb * ff) / (aa * dd - bb * cc);
                    float Bb = (aa * ff - cc * ee) / (aa * dd - bb * cc);

                    edgePos3 = cVertices[index2] + Aa * (cVertices[index1] - cVertices[index2]);
                    edgePos4 = p2 + Bb * (p3 - p2);

                    //fabric 삼각형 위에 edge가 있는지.
                    Vector3 cinner1, cinner2, cinner3;
                    cinner1 = Vector3.Cross(edgePos4 - p3, p2 - p3);
                    cinner2 = Vector3.Cross(edgePos4 - p2, p3 - p);
                    cinner3 = Vector3.Cross(edgePos4 - p3, p3 - p3);
                    int count = 0;
                    if (cinner1.x >= 0 && cinner1.y >= 0 && cinner1.z >= 0)
                    {
                        count++;
                        if (cinner2.x >= 0 && cinner2.y >= 0 && cinner2.z >= 0)
                        {
                            count++;
                            if (cinner3.x >= 0 && cinner3.y >= 0 && cinner3.z >= 0)
                            {
                                count++;
                            }
                        }
                    }

                    if (Mathf.Abs(edgePos3.x - edgePos4.x) < 0.05f && Mathf.Abs(edgePos3.y - edgePos4.y) < 0.05f
                        && Mathf.Abs(edgePos3.z - edgePos4.z) < 0.05f)
                    {
                        //삼각형 ABC 변 위에 있다면
                        if (count >= 3)
                        {
                            collision = true;
                            //1. edgePos로 옮기기

                            //p = edgePos;

                            //2. 나누기 
                        }

                    }

                }
            }

        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(B, 0.151f);//B
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(A, 0.151f);//A
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(C, 0.151f);//C

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(p, 0.051f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(p2, 0.051f);
        Gizmos.DrawSphere(p3, 0.051f);
        Gizmos.DrawLine(p, p2);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p, p3);

        Gizmos.color = Color.white;
        Gizmos.DrawSphere(planePos, 0.05f);


        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(edgePos, 0.05f);
        Gizmos.DrawSphere(edgePos3, 0.05f);

        Gizmos.color = Color.black;
        Gizmos.DrawSphere(edgePos2, 0.05f);
        Gizmos.DrawSphere(edgePos4, 0.05f);

        if (collision)
        {
            Gizmos.DrawLine(edgePos4, edgePos2);
            Gizmos.DrawLine(edgePos4, p);
        }
    }
}
