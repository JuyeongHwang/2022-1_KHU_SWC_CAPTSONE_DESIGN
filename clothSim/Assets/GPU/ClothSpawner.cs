using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ClothSpawner : MonoBehaviour
{
	//public Transform right, left;
	public GameObject Cube;
	public GameObject Sphere;
	public Vector3 positionOffset = Vector3.zero;
	//public IntegrationMethod method = IntegrationMethod.EULARIAN;  // Not changeable at runtime.
	public int resolution = 25;  // Not changeable at runtime.
	public float size = 10f;  // Not changeable at runtime.
	public int loops = 1;
	public float cor = 0.1f;
	public float mass = 1.0f;

	// Mesh and mesh arrays.
	private Mesh mesh;
	public Vector3[] vertices;
	public Vector3[] oldPositions;
	public int[] triangles;

	public Vector3[] forces;
	Vector3[] velocities;

	private int count;  // total number of nodes.
	private int dim;  // number of nodes per row in cloth.
	private float segmentLength;  // length of one square of cloth.
	private Vector3[] zeros;

	// parallel spring parameters
	public float pScale = 1.0f;
	public float pKs = -10000.0f;
	public float pKd = -1000.0f;

	// Diagonal spring parameters.
	public float dScale = 1.0f;
	public float dKs = -10000.0f;
	public float dKd = -1000.0f;

	// Bending spring parameters.
	public float bScale = 1.0f;
	public float bKs = -10000.0f;
	public float bKd = -1000.0f;
	//==============================

	[System.Serializable]
	public struct SphereCollider
	{
		public Vector3 center;
		public float radius;
	}

	public static int SphereColliderSize()
	{
		return 16;
	}

	public SphereCollider[] sphereColliders;
	private ComputeBuffer sphereBuffer;
	private int sphereCount = 0;

	public void Recalculate()
	{
		// Update the resolution-related parameters.
		dim = resolution + 1;
		count = dim * dim;

		segmentLength = size / (float)resolution;

		// Recalculate the vertices array to be drawn as gizmos.
		vertices = new Vector3[count];
		oldPositions = new Vector3[count];
		forces = new Vector3[count];
		velocities = new Vector3[count];
		for (int y = 0; y < dim; y++)
		{
			for (int x = 0; x < dim; x++)
			{
				vertices[y * dim + x] = new Vector3((float)x * segmentLength,0- (float)y * segmentLength, 0);// + positionOffset;
				oldPositions[y * dim + x] = vertices[y * dim + x];
				forces[y * dim + x] = Vector3.zero;
				velocities[y * dim + x] = Vector3.zero;
			}
		}
	}

    private void Awake()
    {
		makeSphere();
		Recalculate();
		mesh = CreateMesh("ClothMesh");
		GetComponent<MeshFilter>().mesh = mesh;
	}

	private void FixedUpdate()
	{
		makeCube();
		for (int i = 0; i < count; i++)
		{
			forces[i] = Vector3.zero;
		}
		Spring();
		Integrate();

		mesh.vertices = vertices;
		mesh.RecalculateNormals();
	}



	int2 To2D(uint id)
    {
		return new int2((int)id % dim, (int)id / dim);

	}
	int2 To2D(int id)
	{
		return new int2((int)id % dim, (int)id / dim);

	}
	uint To1D(int2 id)
    {
		return (uint)(id.y * dim + id.x);
    }

	bool isValid2D(int2 id)
    {
		return !(id.x < 0 || id.x >= (int)dim || id.y < 0 || id.y >= (int)dim);
	}

	Vector3 GetSpringForces(uint a, uint b, float restLength, float ks, float kd)
    {
		Vector3 aPos = vertices[a];
		Vector3 bPos = vertices[b];

		Vector3 aVel = velocities[a];
		Vector3 bVel = velocities[b];

		if((bPos-aPos).magnitude < 0.00001)
        {
			return Vector3.zero;
        }

		Vector3 dir = (bPos - aPos).normalized;

		float springForce = -ks * ((bPos - aPos).magnitude - restLength);
		//float dampingForce = -kd * (Vector3.Dot(bVel, dir) - Vector3.Dot(aVel, dir));
		float dampingForce = -kd * Vector3.Dot(bVel-aVel, dir);  // same thing

		return (springForce + dampingForce) * dir;
	}
	Vector3 GetStructuralSpringForces(uint i)
	{
		Vector3 force = Vector3.zero;

		int2 above = To2D(i) + new int2(0, 1);
		int2 below = To2D(i) + new int2(0, -1);
		int2 left = To2D(i) + new int2(-1, 0);
		int2 right = To2D(i) + new int2(1, 0);

        if (isValid2D(above))
        {
			force += GetSpringForces(i, To1D(above), segmentLength, pKs, pKd );
        }

		if (isValid2D(below))
		{
			force += GetSpringForces(i, To1D(below), segmentLength, pKs, pKd);
		}

		if (isValid2D(left))
		{
			force += GetSpringForces(i, To1D(left), segmentLength, pKs, pKd);
		}

		if (isValid2D(right))
		{
			force += GetSpringForces(i, To1D(right), segmentLength, pKs, pKd);
		}

		return force;
	}

	/* Get the forces from all diagonal springs acting on node i. */
	Vector3 GetDiagonalSpringForces(uint i)
	{
		Vector3 force = Vector3.zero;

		int2 ll = To2D(i) + new int2(-1, -1);
		int2 ul = To2D(i) + new int2(-1, 1);
		int2 lr = To2D(i) + new int2(1, -1);
		int2 ur = To2D(i) + new int2(1, 1);

		if (isValid2D(ll))
		{
			force += GetSpringForces(i, To1D(ll), segmentLength*Mathf.Sqrt(2.0f), dKs, dKd);
		}

		if (isValid2D(ul))
		{
			force += GetSpringForces(i, To1D(ul), segmentLength * Mathf.Sqrt(2.0f), dKs, dKd);
		}

		if (isValid2D(lr))
		{
			force += GetSpringForces(i, To1D(lr), segmentLength * Mathf.Sqrt(2.0f), dKs, dKd);
		}

		if (isValid2D(ur))
		{
			force += GetSpringForces(i, To1D(ur), segmentLength * Mathf.Sqrt(2.0f), dKs, dKd);
		}

		return force;
	}

	/* Get the forces from all bending springs acting on node i. */
	Vector3 GetBendingSpringForces(uint i)
	{
		Vector3 force = Vector3.zero;

		int2 ll = To2D(i) + new int2(-2, -2);
		int2 ul = To2D(i) + new int2(-2, 2);
		int2 lr = To2D(i) + new int2(2, -2);
		int2 ur = To2D(i) + new int2(2, 2);

		if (isValid2D(ll))
		{
			force += GetSpringForces(i, To1D(ll), segmentLength * Mathf.Sqrt(2.0f)*2.0f, bKs, bKd);
		}

		if (isValid2D(ul))
		{
			force += GetSpringForces(i, To1D(ul), segmentLength * Mathf.Sqrt(2.0f) * 2.0f, bKs, bKd);
		}

		if (isValid2D(lr))
		{
			force += GetSpringForces(i, To1D(lr), segmentLength * Mathf.Sqrt(2.0f) * 2.0f, bKs, bKd);
		}

		if (isValid2D(ur))
		{
			force += GetSpringForces(i, To1D(ur), segmentLength * Mathf.Sqrt(2.0f) * 2.0f, bKs, bKd);
		}

		return force;
	}


	void Spring()
	{
		for(uint i = 0; i<count; i++)
        {
			forces[i] += GetStructuralSpringForces(i) * pScale;
			forces[i] += GetBendingSpringForces(i) * bScale;
			forces[i] += GetDiagonalSpringForces(i) * dScale;
		}
	}

	public float timestep = 1.0f/600;
	public float maxForce = 1;
	void Integrate() { 
	
	
		for(int i = 0; i<count; i++)
        {
			Vector3 gravity = new Vector3(0, -9.81f, 0);

			Vector3 a = gravity + forces[i] / mass;
			forces[i] += gravity;


			if (Mathf.Abs(forces[i].x) > maxForce)
			{
				forces[i] = new Vector3(forces[i].x / Mathf.Abs(forces[i].x) * maxForce, forces[i].y, forces[i].z);
			}
			if (Mathf.Abs(forces[i].y) > maxForce)
			{
				forces[i] = new Vector3(forces[i].x, forces[i].y / Mathf.Abs(forces[i].y) * maxForce, forces[i].z);
			}
			if (Mathf.Abs(forces[i].z) > maxForce)
			{
				forces[i] = new Vector3(forces[i].x, forces[i].y, forces[i].z / Mathf.Abs(forces[i].z) * maxForce);
			}


			if (i==0 || i== dim - 1)
            {
				//vertices[0] = right.position;
				//vertices[dim - 1] = left.position;
				forces[i] = Vector3.zero;
				continue;
            }
			Vector3 newPos = 2 * vertices[i] - oldPositions[i] + forces[i] * mass * (timestep);

            newPos = clothclothCollide2(i, newPos);
            //newPos = clothsphereCollide(i, newPos);
            //newPos = checkCubeCollision(i, mass, newPos);
            oldPositions[i] = vertices[i];
            vertices[i] = newPos;
            velocities[i] = vertices[i] - oldPositions[i];

   //         velocities[i] += a * (Time.deltaTime / (float)loops);
			//vertices[i] += velocities[i] * (Time.deltaTime / (float)loops);

		}
	
	}

	#region sphere
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

	#endregion

	#region cube
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
	float h = 0.1f;
	Vector3 A = new Vector3(1, 1, 0);
	Vector3 B = new Vector3(-1, -1, 0);
	Vector3 C = new Vector3(3, -1, 0);
	Vector3 norm = Vector3.zero;
	Vector3 p = Vector3.zero;

	public Vector3 checkCubeCollision(int curi, float mass, Vector3 newPos)
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
							if (pp.x >= -h && pp.x <= h)
							{
								if (pp.y >= -h && pp.y <= h)
								{
									if (pp.z >= -h && pp.z <= h)
									{
										collision = true;
										//else if(edge)

										//if(not edge)

										newPos = oldPositions[curi];
										//newPos = checkEdgeCollision(j, newPos, curi);
										velocities[curi] -= (2 * norm.magnitude / (1 + w1 * w1 +
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
							if (pp.x >= -h && pp.x <= h)
							{
								if (pp.y >= -h && pp.y <= h)
								{
									if (pp.z >= -h && pp.z <= h)
									{
										collision = true;
										//else if(edge)
										//if(not edge)
										newPos = oldPositions[curi];
										//newPos = checkEdgeCollision(j, newPos, curi);
										velocities[curi] -= (2 * norm.magnitude / (1 + w1 * w1 +
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

	Vector3 checkEdgeCollision(int j, Vector3 newPos, int curi)
	{
		int collisionIndex1 = 0, collisionIndex2 = 0, collisionIndex3 = 0;
		collisionIndex1 = cTriangles[0 + j]; // A
		collisionIndex2 = cTriangles[1 + j]; // B
		collisionIndex3 = cTriangles[2 + j]; // C

		bool meetEdge = false;
		for (int i = 0; i < cEdges.Count; i += 2)
		{
			if (cEdges[i] == collisionIndex1 || cEdges[i] == collisionIndex2 || cEdges[i] == collisionIndex3)
			{
				if (cEdges[i + 1] == collisionIndex1 || cEdges[i + 1] == collisionIndex2 || cEdges[i + 1] == collisionIndex3)
				{
					meetEdge = true;
				}
			}

			if (meetEdge)
			{
				Vector3 p2, p = newPos;
				Vector3 edgePos = Vector3.zero;
				Vector3 edgePos2 = Vector3.zero;

				int2 above = To2D(i) + new int2(0, 1);
				int2 below = To2D(i) + new int2(0, -1);
				int2 left = To2D(i) + new int2(-1, 0);
				int2 right = To2D(i) + new int2(1, 0);

				if (isValid2D(above))
				{
					p2 = vertices[To1D(above)];
					newPos = check(p2, p, edgePos, edgePos2, newPos, i);
					return newPos;
				}

				if (isValid2D(below))
				{
					p2 = vertices[To1D(below)];
					newPos = check(p2, p, edgePos, edgePos2, newPos, i);
					return newPos;
				}

				if (isValid2D(left))
				{
					p2 = vertices[To1D(left)];
					newPos = check(p2, p, edgePos, edgePos2, newPos, i);
					return newPos;
				}

				if (isValid2D(right))
				{
					p2 = vertices[To1D(right)];
					newPos = check(p2, p, edgePos, edgePos2, newPos, i);
					return newPos;
				}

			}
		}
		return newPos;
	}

	Vector3 check(Vector3 p2, Vector3 p, Vector3 edgePos, Vector3 edgePos2, Vector3 newPos, int i)
    {
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

		if ((edgePos - edgePos2).magnitude < 0.00003f)
		{
			//print((edgePos - edgePos2).magnitude);
			newPos = edgePos2;
		}
		return newPos;
	}

	#endregion

	public Vector3 clothclothCollide2(int curi, Vector3 newPos)
	{
		for (int j = 0; j < vertices.Length; j++)
		{
			if (curi == j) { continue; }
			float distance = (newPos - vertices[j]).magnitude;
			if (distance < 0.001f)
			{
				Vector3 dir = vertices[j] - newPos;
				newPos -= dir.normalized * (distance);
			}
		}
		return newPos;
	}


	private void OnDrawGizmos()
    {
		for(int i =0; i<count; i++)
        {
			Gizmos.DrawSphere(vertices[i], segmentLength * 0.25f);
        }
		if (sphereColliders != null)
		{
			foreach (SphereCollider s in sphereColliders)
			{
				Gizmos.DrawWireSphere(s.center, s.radius);
			}
		}

	}




	private Mesh CreateMesh(string name)
	{
		Mesh mesh = new Mesh();
		mesh.name = name;

		mesh.vertices = vertices;

		triangles = new int[resolution * resolution * 6];
		for (int ti = 0, vi = 0, y = 0; y < resolution; y++, vi++)
		{
			for (int x = 0; x < resolution; x++, ti += 6, vi++)
			{
				triangles[ti] = vi;
				triangles[ti + 3] = triangles[ti + 2] = vi + 1;
				triangles[ti + 4] = triangles[ti + 1] = vi  +resolution+ 1;
				triangles[ti + 5] = vi + resolution + 2;
			}
		}
		mesh.triangles = triangles;

		Vector2[] uvs = new Vector2[vertices.Length];
		for (int y = 0; y < dim; y++)
		{
			for (int x = 0; x < dim; x++)
			{
				float u = (float)x / (float)resolution;
				float v = (float)y / (float)resolution;
				uvs[y * dim + x] = new Vector2(u, v);
			}
		}
		mesh.uv = uvs;

		mesh.RecalculateNormals();
		return mesh;
	}

}
