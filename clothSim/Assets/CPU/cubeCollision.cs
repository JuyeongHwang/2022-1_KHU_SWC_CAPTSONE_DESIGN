using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cubeCollision : MonoBehaviour
{
    public static cubeCollision instance;
    public GameObject Cube;

    private void Awake()
    {
        cubeCollision.instance = this;
    }

    public Vector3[] cVertices = new Vector3[] { };
    Vector3[] cNormals = new Vector3[] { };
    int[] cTriangles = new int[] { };
    public List<int> cEdges = new List<int>();
    private void Start()
    {
        makeCube();
        cEdges.Add(0);
        cEdges.Add(1);
        cEdges.Add(0);
        cEdges.Add(2);
        cEdges.Add(0);
        cEdges.Add(6);

        cEdges.Add(3);
        cEdges.Add(1);
        cEdges.Add(3);
        cEdges.Add(2);
        cEdges.Add(3);
        cEdges.Add(5);

        cEdges.Add(4);
        cEdges.Add(2);
        cEdges.Add(4);
        cEdges.Add(6);
        cEdges.Add(4);
        cEdges.Add(5);

        cEdges.Add(7);
        cEdges.Add(5);
        cEdges.Add(7);
        cEdges.Add(6);
        cEdges.Add(7);
        cEdges.Add(1);
    }
    public bool move = false;
    private void Update()
    {
        if (move)
        {
            Cube.transform.position -= Vector3.forward * Time.deltaTime * 0.01f;
            makeCube();

        }
    }


    void makeCube()
    {
        Mesh cubemesh = new Mesh();
        //Cube.transform.localScale *= 1f;
        cubemesh = Cube.transform.GetComponent<MeshFilter>().sharedMesh;

        cVertices = cubemesh.vertices;
        for (int i = 0; i < cVertices.Length; i++)
        {
            //cVertices[i] += Cube.transform.position;
            cVertices[i] = Cube.transform.TransformPoint(cubemesh.vertices[i]);
        }
        cTriangles = cubemesh.triangles;
        cNormals = cubemesh.normals;
    }


    Vector3 planePos = Vector3.zero;
    public float cubeDetect = 0.03f;

    bool collision = false;
    float h = 0.3f;
    Vector3 A = new Vector3(1, 1, 0);
    Vector3 B = new Vector3(-1, -1, 0);
    Vector3 C = new Vector3(3, -1, 0);
    Vector3 norm = Vector3.zero;
    Vector3 p = Vector3.zero;

    public Vector3 checkCubeCollision(int curi, float mass, Vector3 newPos, List<Vector3> Positions, List<Vector3> OldPos,
    List<Vector3> Velocities)
    {
        p = newPos;

        for (int j = 0; j < cTriangles.Length; j += 3)
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
            if (distance < cubeDetect)
            {
                if (inner1.x <= 0 && inner1.y <= 0 && inner1.z <= 0)
                {
                    if (inner2.x <= 0 && inner2.y <= 0 && inner2.z <= 0)
                    {
                        if (inner3.x <= 0 && inner3.y <= 0 && inner3.z <= 0)
                        {
                            Vector3 pp = p - planePos;
                            if (pp.x >= -h  && pp.x <= h )
                            {
                                if (pp.y >= -h  && pp.y <= h )
                                {
                                    if (pp.z >= -h  && pp.z <= h )
                                    {
                                        collision = true;
                                        //else if(edge)
                                        //newPos = checkEdgeCollision(j, newPos, curi, Positions);
                                        //if(not edge)
                                        newPos += norm * 0.000091f;

                                        Velocities[curi] -= (2 * norm.magnitude / (1 + w1 * w1 +
                                w2 * w2 + w3 * w3) / mass) * norm;
                                        return newPos;
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
                            if (pp.x >= -h  && pp.x <= h )
                            {
                                if (pp.y >= -h  && pp.y <= h )
                                {
                                    if (pp.z >= -h  && pp.z <= h )
                                    {
                                        collision = true;
                                        //else if(edge)
                                        //newPos = checkEdgeCollision(j, newPos, curi, Positions);
                                        //if(not edge)
                                        newPos += norm * 0.000091f;
                                        Velocities[curi] -= (2 * norm.magnitude / (1 + w1 * w1 +
                                w2 * w2 + w3 * w3) / mass) * norm;

                                        return newPos;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return newPos;
    }

    Vector3 checkEdgeCollision(int j, Vector3 newPos, int curi, List<Vector3> Positions)
    {
        int collisionIndex1 = 0, collisionIndex2 = 0, collisionIndex3 = 0;
        collisionIndex1 = cTriangles[0 + j]; // A
        collisionIndex2 = cTriangles[1 + j]; // B
        collisionIndex3 = cTriangles[2 + j]; // C

        bool meetEdge = false;
        for(int i = 0; i<cEdges.Count; i += 2)
        {
            if(cEdges[i] == collisionIndex1 || cEdges[i] == collisionIndex2 || cEdges[i] == collisionIndex3)
            {
                if (cEdges[i+1] == collisionIndex1 || cEdges[i+1] == collisionIndex2 || cEdges[i+1] == collisionIndex3)
                {
                    meetEdge = true;
                }
            }

            if(meetEdge)
            {
                Vector3 p2,p = newPos;
                Vector3 edgePos = Vector3.zero;
                Vector3 edgePos2 = Vector3.zero;

                for (int str = 0; str < findSprings.instance.structurals[curi].Length; str++)
                {
                    p2 = Positions[findSprings.instance.structurals[curi][str]];

                    //edge 위의 점 구하기
                    int index1 = cEdges[i + 1];
                    int index2 = cEdges[i];
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

                    if((edgePos-edgePos2).magnitude < 0.00003f)
                    {
                        //print((edgePos - edgePos2).magnitude);
                        newPos = edgePos2;
                        return newPos;
                    }

                }
            
            }
        }
        return newPos;
    }

}
