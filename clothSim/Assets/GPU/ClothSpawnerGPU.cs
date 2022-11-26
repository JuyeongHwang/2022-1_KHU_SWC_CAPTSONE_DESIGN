using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ClothSpawnerGPU : MonoBehaviour
{
    public int iter = 0;
    #region Parameters
    public GetCollider sphere;
    //public Transform lefthand, leftarm, leftarm2;
    //public Transform righthand, rightarm, rightarm2;
    //public Transform spine3, spine2;

    public Transform leftup;
    //public Transform leftmiddle, rightmiddle;
    //public Transform centerup, centermiddle;
    public Transform rightup;

    // simulation settings
    Vector3 positionOffset = Vector3.zero;
    public int resolution = 5;
    public float size = 1.0f;
    public int loops = 100;
    public float cor = 0.1f; // ?
    public float mass = 1.0f;

    //structural
    public float structuralScale = 1.0f;
    public float structuralKs = -10000.0f;
    public float structuralKd = -1000.0f;

    //diagonal
    public float diagonalScale = 1.0f;
    public float diagonalKs = -10000.0f;
    public float diagonalKd = -1000.0f;

    //bending
    public float bendingScale = 1.0f;
    public float bendingKs = -10000.0f;
    public float bendingKd = -1000.0f;

    //wind
    public float windScale = 1.0f;
    public Vector3 windVelocity = Vector3.zero;

    //mesh
    private Mesh mesh;
    public Vector3[] vertices;
    private int[] triangles;

    //============================

    //compute shader and kernel
    public ComputeShader clothCompute;
    private int springKernel;
    private int IntegrateKernel;

    //compute buffers
    ComputeBuffer positionBuffer;
    ComputeBuffer forceBuffer;
    ComputeBuffer velocityBuffer;
    ComputeBuffer restrainedBuffer;

    //other
    private int count;
    private int dim;
    private float spacing;
    private Vector3[] zeros;
    private bool successfullyInitialized = false;

    //collision

    [System.Serializable]
    public struct SphereCollider
    {
        public Vector3 center;
        public float radius;
    }
    static int SphereColliderSize()
    {
        return 16;
    }


    public SphereCollider[] sphereColliders;
    private ComputeBuffer sphereBuffer;
    public int sphereCount;

    #endregion

    #region Mono


    private void Awake()
    {
        Recalculate();

        mesh = CreateMesh("ClothMesh");
        GetComponent<MeshFilter>().mesh = mesh;

        SetBuffers();

    }


    Vector3 moveDirection;
    private void Update()
    {

        Stopwatch watch = new Stopwatch();
        watch.Start();
        //world->local setBuffer해야하는건 아닐까
        //print("origin : "+vertices[10]);
        //print("local to world : " + transform.TransformPoint(vertices[10]));
        //print("world to local : " + transform.InverseTransformPoint(vertices[10]));
        while (true)
        {
            if (iter <= 0) { break; }
            UpdateSimulation(loops, Time.deltaTime);

            positionBuffer.GetData(vertices);

            mesh.vertices = vertices;

            mesh.RecalculateNormals();
            iter--;

        }
        watch.Stop();
        print("GPU :" + watch.ElapsedMilliseconds + "ms");

        //moveDirection = new Vector3(Input.GetAxis("Horizontal"),0,
        //    Input.GetAxis("Vertical"));
        //if (Input.GetKey(KeyCode.Q))
        //{
        //    moveDirection += Vector3.up;
        //}
        //else if (Input.GetKey(KeyCode.E))
        //{

        //    moveDirection -= Vector3.up;
        //}
        ////controlCape(0,Time.deltaTime);
    }


    private void FixedUpdate()
    {


        if (successfullyInitialized)
        {

        }
    }
    public bool use_char = false;

    float speed = 2.0f;
    //control cape
    void controlCape(int t, float dt)
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed = 5.0f;
        }
        else
        {
            speed = 2.0f;
        }
        if (use_char)
        {
            leftup.position += speed * moveDirection * dt;

            rightup.position += speed * moveDirection * dt;

            //leftmiddle.position += speed * moveDirection * dt;

            //rightmiddle.position += speed * moveDirection * dt;

            //centerup.position += speed * moveDirection * dt;

            //centermiddle.position += speed * moveDirection * dt;
        }

    }

    private void OnDestroy()
    {
        releaseBuffersData();
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < sphereColliders.Length; i++)
        {
            Gizmos.DrawWireSphere(sphereColliders[i].center, sphereColliders[i].radius);

        }
    }

    public void Recalculate()
    {
        dim = resolution + 1;
        count = dim * dim;

        spacing = size / (float)resolution;

        vertices = new Vector3[count];
        positionOffset = this.transform.position;
        int index = 0;
        for (int y = 0; y < dim; y++)
        {
            for (int x = 0; x < dim; x++)
            {
                vertices[y * dim + x] = new Vector3(size*0.5f-(float)x * spacing, 0f,
                    0-(float)y * spacing)+ positionOffset;
            }
        }
    }

    private Mesh CreateMesh(string name)
    {
        Mesh inMesh = new Mesh();
        inMesh.name = name;

        inMesh.vertices = vertices;

        triangles = new int[resolution * resolution * 6];
        for(int ti =0, vi = 0, y=0; y<resolution; y++, vi++)
        {
            for(int x = 0; x<resolution; x++, ti+=6, vi++)
            {
                triangles[ti] = vi; //vi : vertex
                triangles[ti + 3] = triangles[ti + 2] = vi + 1; //right
                triangles[ti + 4] = triangles[ti + 1] = vi + resolution + 1; // 아래
                triangles[ti + 5] = vi + resolution + 2;

            }
        }
        inMesh.triangles = triangles;

        Vector2[] uvs = new Vector2[vertices.Length];
        for(int y = 0; y<dim; y++)
        {
            for(int x = 0; x<dim; x++)
            {
                float u = (float)x / (float)resolution * 2.0f;
                float v = (float)y / (float)resolution * 2.0f;
                uvs[y * dim + x] = new Vector2(u, v);
            }
        }

        inMesh.uv = uvs;
        inMesh.RecalculateNormals();

        return inMesh;
    }


    #endregion

    #region ComputeShader

    private void SetBuffers()
    {
        positionBuffer = new ComputeBuffer(count, 12); // 4 byte * 3
        positionBuffer.SetData(vertices);

        zeros = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            zeros[i] = Vector3.zero;
        }

        forceBuffer = new ComputeBuffer(count, 12); // 4 byte * 3
        forceBuffer.SetData(zeros);

        velocityBuffer = new ComputeBuffer(count, 12); // 4 byte * 3
        velocityBuffer.SetData(zeros);

        int[] restrainedArray = new int[count];
        for (int i = 0; i < count; i++)
        {
            if (i == 0 || i == dim - 1)
            {
                restrainedArray[i] = 1;
            }
            else
            {
                restrainedArray[i] = 0;
            }
        }
        restrainedBuffer = new ComputeBuffer(count, 4);
        restrainedBuffer.SetData(restrainedArray);

        //Vector3Int[] triangleArray = new Vector3Int[triangles.Length / 3]; //3개씩 묶어주는 과정
        //for (int i = 0; i < triangles.Length; i += 3)
        //{
        //    triangleArray[i / 3] = new Vector3Int(triangles[i], triangles[i + 1], triangles[i + 2]);
        //}

        //triangleBuffer = new ComputeBuffer(triangleArray.Length, 12);
        //triangleBuffer.SetData(triangleArray);

        //get kernels from compute shader
        springKernel = clothCompute.FindKernel("Spring");
        IntegrateKernel = clothCompute.FindKernel("Integrate");

        //upload the buffers to the gpu, and make them available to each kernel

        clothCompute.SetBuffer(springKernel, "positionBuffer", positionBuffer);
        clothCompute.SetBuffer(springKernel, "velocityBuffer", velocityBuffer);
        clothCompute.SetBuffer(springKernel, "forceBuffer", forceBuffer);

        clothCompute.SetBuffer(IntegrateKernel, "positionBuffer", positionBuffer);
        clothCompute.SetBuffer(IntegrateKernel, "velocityBuffer", velocityBuffer);
        clothCompute.SetBuffer(IntegrateKernel, "forceBuffer", forceBuffer);
        clothCompute.SetBuffer(IntegrateKernel, "restrainedBuffer", restrainedBuffer);

        clothCompute.SetInt("count", count);
        clothCompute.SetInt("dim", dim);

        clothCompute.SetFloat("structuralRl", spacing);
        clothCompute.SetFloat("diagonalRl", spacing * Mathf.Sqrt(2.0f));
        clothCompute.SetFloat("bendingRl", spacing * Mathf.Sqrt(2.0f) * 2);

        successfullyInitialized = true;
    }

    void UpdateSimulation(int t, float dt)
    {
        clothCompute.SetFloat("mass", mass);
        clothCompute.SetFloat("cor", cor);
        clothCompute.SetFloat("dt", dt/(float)t);

        clothCompute.SetFloat("windScale", windScale);
        clothCompute.SetVector("windVelocity", windVelocity);

        clothCompute.SetFloat("structuralScale", structuralScale);
        clothCompute.SetFloat("structuralKs", structuralKs);
        clothCompute.SetFloat("structuralKd", structuralKd);

        clothCompute.SetFloat("diagonalScale", diagonalScale);
        clothCompute.SetFloat("diagonalKs", diagonalKs);
        clothCompute.SetFloat("diagonalKd", diagonalKd);

        clothCompute.SetFloat("bendingScale", bendingScale);
        clothCompute.SetFloat("bendingKs", bendingKs);
        clothCompute.SetFloat("bendingKd", bendingKd);

        //controlVertices(t, dt);
        if (use_char)
        {
            clothCompute.SetVector("leftup", leftup.position);
            //clothCompute.SetVector("rightmiddle", rightmiddle.position);
            //clothCompute.SetVector("leftmiddle", leftmiddle.position);
            //clothCompute.SetVector("centerup", centerup.position);
            //clothCompute.SetVector("centermiddle", centermiddle.position);
            clothCompute.SetVector("rightup", rightup.position);
        }



        //if (sphereBuffer != null)
        //{
        //    sphereBuffer.Release();
        //}
        //if (sphereColliders != null && sphereColliders.Length > 0)
        //{
        //    sphereCount = sphereColliders.Length;
        //    if (sphereColliders.Length > 8)
        //    {
        //    }
        //    sphereBuffer = new ComputeBuffer(sphereCount, SphereColliderSize());
        //    //sphereColliders[0].center += Vector3.forward * Time.deltaTime * 0.5f;
        //    sphereBuffer.SetData(sphereColliders);
        //    clothCompute.SetInt("sphereCount", sphereCount);
        //    clothCompute.SetBuffer(IntegrateKernel, "sphereBuffer", sphereBuffer);
        //}
        //else
        //{
        //    clothCompute.SetInt("sphereCount", 0);
        //}

        for (int i = 0; i < t; i++)
        {
            forceBuffer.SetData(zeros);
            //controlVertices();
            clothCompute.Dispatch(springKernel, count / 512 + 1, 1, 1);
            clothCompute.Dispatch(IntegrateKernel, count / 512 + 1, 1, 1);

        }
    }

    void releaseBuffersData()
    {
        if(positionBuffer != null)
        {
            positionBuffer.Release();
        }
        if(velocityBuffer != null)
        {
            velocityBuffer.Release();
        }
        if(forceBuffer != null)
        {
            forceBuffer.Release();
        }
        if(restrainedBuffer != null)
        {
            restrainedBuffer.Release();
        }
        if(sphereBuffer != null)
        {
            sphereBuffer.Release();
        }
    }
    #endregion

}
