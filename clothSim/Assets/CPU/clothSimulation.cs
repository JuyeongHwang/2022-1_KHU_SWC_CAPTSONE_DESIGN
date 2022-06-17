using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class clothSimulation : MonoBehaviour
{


    [Header("Particles Info")]
    public List<Vector3> Positions = new List<Vector3>() { };
    public List<Vector3> Forces = new List<Vector3>() { };
    public List<Vector3> Velocities = new List<Vector3>() { };
    List<Vector3> OldPos = new List<Vector3>() { };

    [Space(1)]
    [Header("Simulation Parameters")]
    public int width = 6;
    public int height = 6;
    public float timestep = 1 / 60.0f / 10;
    public float mass = 1.0f;
    public Vector3 gravity = new Vector3(0, -9.8f, 0);

    public float stiffness = 1.0f;//k0
    public float shearSpring = 1.0f;//k1
    public float bendingSpring = 1.0f;//k2
    public float damping = 0.5f;
    public float windCoef = 0.5f;
    public float spacing = 0.5f;
    public int space = 2;
    public Vector3 windDir = new Vector3(-0.5f, 0, 0);

    int lastIndex;
    void Start()
    {
        lastIndex = width * space - 1;
        mass /= width * height * 4;
        for (float i = height; i > 0.0001; i -= spacing)
        {
            for (float j = 0; j < width - 0.0001; j += spacing)
            {
                Vector3 pos = new Vector3(j, i, 0);
                pos += this.transform.position;
                Positions.Add(pos);
                OldPos.Add(pos);
                Forces.Add(mass * gravity);
                Velocities.Add(Vector3.zero);

            }
        }

        findSprings.instance.findStructural(Positions.Count, lastIndex);
        findSprings.instance.findShear(Positions.Count, lastIndex);
        findSprings.instance.findBend(Positions.Count, lastIndex);

        meshGenerator.instance.meshGen(width, space, Positions);

    }

    // Update is called once per frame
    void Update()
    {
        calcForces.instance.calcExternalForces(Positions.Count, lastIndex, damping, mass, windCoef, Forces, windDir, Velocities, gravity);
        calcForces.instance.calcSpringForces(lastIndex, Forces, Positions, spacing, stiffness, shearSpring, bendingSpring);
        verlet();


        meshGenerator.instance.changeMesh(width, space, Positions);


        for (int index = 0; index < Positions.Count; index++)
        {
            for (int i = 0; i < findSprings.instance.structurals[index].Length; i++)
            {
                Debug.DrawLine(Positions[index], Positions[findSprings.instance.structurals[index][i]], Color.blue);
                //Debug.Log(i + "(structurals) : " + findSprings.instance.structurals[index][i]);
            }
            for (int i = 0; i < findSprings.instance.shears[index].Length; i++)
            {
                Debug.DrawLine(Positions[index], Positions[findSprings.instance.shears[index][i]], Color.red);
                //Debug.Log(i + "(shear) : " + findSprings.instance.shears[index][i]);
            }
            for (int i = 0; i < findSprings.instance.bends[index].Length; i++)
            {
                Debug.DrawLine(Positions[index], Positions[findSprings.instance.bends[index][i]], Color.blue);
                //Debug.Log(i + "(bends) : " + findSprings.instance.bends[index][i]);
            }
        }
    }


    public float maxForce = 15;
    void verlet()
    {
        for (int i = 0; i < Positions.Count; i++)
        {
            if (i == 0 || i == lastIndex)
            {
                Forces[i] = Vector3.zero;
                continue;
            }

            if (Mathf.Abs(Forces[i].x) > maxForce)
            {
                Forces[i] = new Vector3(Forces[i].x / Mathf.Abs(Forces[i].x) * maxForce, Forces[i].y, Forces[i].z);
            }
            if (Mathf.Abs(Forces[i].y) > maxForce)
            {
                Forces[i] = new Vector3(Forces[i].x, Forces[i].y / Mathf.Abs(Forces[i].y) * maxForce, Forces[i].z);
            }
            if (Mathf.Abs(Forces[i].z) > maxForce)
            {
                Forces[i] = new Vector3(Forces[i].x, Forces[i].y, Forces[i].z / Mathf.Abs(Forces[i].z) * maxForce);
            }

            Vector3 newPos = 2 * Positions[i] - OldPos[i] + Forces[i] * mass * timestep;

            newPos = cubeCollision.instance.checkCubeCollision(i, mass, newPos, Positions, OldPos, Velocities);

            newPos = collision.instance.clothclothCollide2(i, newPos, Positions, spacing);
            newPos = deformationConstraints2(i, newPos);

            OldPos[i] = Positions[i];
            Positions[i] = newPos;
            Velocities[i] = Positions[i] - OldPos[i];

            
            //if (collision.instance.checkCubeCollision(i, mass,
            //    newPos, Positions, OldPos, Velocities))
            //{

            //}
            //else
            //{
            //    OldPos[i] = Positions[i];
            //    Positions[i] = newPos;
            //    Velocities[i] = Positions[i] - OldPos[i];
            //}


        }


    }

    public float allowedRate = 0.5f;

    Vector3 deformationConstraints2(int i, Vector3 newPos)
    {
        float structLen = spacing;
        float shearLen = spacing * Mathf.Sqrt(2);

        for (int j = 0; j < findSprings.instance.shears[i].Length; j++)
        {
            //if (i+ (lastIndex) < shears[i][j]) { continue; }
            newPos = rePosition2(findSprings.instance.shears[i][j], shearLen, newPos);
        }
        for (int j = 0; j < findSprings.instance.structurals[i].Length; j++)
        {
            //if (i+(lastIndex+1) == structurals[i][j]) { continue; }
            newPos = rePosition2(findSprings.instance.structurals[i][j], structLen, newPos);
        }


        return newPos;
    }


    Vector3 rePosition2(int j, float naturalLength, Vector3 newPos)
    {
        Vector3 diff = Positions[j] - newPos;
        float curDistance = (diff.magnitude);
        if (curDistance >= (allowedRate * naturalLength + naturalLength))
        {
            Vector3 orientation = Positions[j] - newPos;
            if (j == 0 || j == lastIndex)// || j == 3 || j == lastIndex-3 || j == 2 || j == lastIndex - 2 || j == 1 || j == lastIndex - 1|| j == lastIndex / 2)
            {
                newPos += orientation / orientation.magnitude * (curDistance - allowedRate * naturalLength - naturalLength) * 0.2f;
            }

            else
            {
                newPos += orientation / orientation.magnitude * (curDistance - allowedRate * naturalLength - naturalLength) * 0.1f;
                //OldPos[j] = Positions[j];
                Positions[j] -= orientation / orientation.magnitude * (curDistance - allowedRate * naturalLength - naturalLength) * 0.1f;
                Velocities[j] = Positions[j] - OldPos[j];
            }
        }
        return newPos;

    }



}