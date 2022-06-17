using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class calcForces : MonoBehaviour
{

    public static calcForces instance;

    private void Awake()
    {
        calcForces.instance = this;

    }


    public void calcExternalForces(int vertexCount, int lastIndex, float damping, float mass,
        float windCoef, List<Vector3> Forces, Vector3 windDir,
        List<Vector3> Velocities, Vector3 gravity)
    {
        int currentCol = 0; //current
        int currentRow = 0;
        int sCol = 0; //spring
        int sRow = 0;

        for (int i = 0; i < vertexCount; i++)
        {
            Vector3 force = Vector3.zero;

            currentCol = i % (lastIndex + 1);
            currentRow = i / (lastIndex + 1);

            for (int j = 0; j < vertexCount; j++)
            {
                if (i == j) { continue; }
                sCol = j % (lastIndex + 1);
                sRow = j / (lastIndex + 1);
                if (Mathf.Abs(currentCol - sCol) <= 1)
                {
                    if (Mathf.Abs(currentRow - sRow) <= 1)
                    {
                        force -= damping * (Velocities[i] - Velocities[j]);
                    }
                }

            }
            //force -= damping * Velocities[i];
            force += mass * gravity;
            //windDir = new Vector3(vthird.cc.input.x, Mathf.Abs(vthird.cc.input.x + vthird.cc.input.z), vthird.cc.input.z);
            force += (windDir - Velocities[i]) * windCoef;
            Forces[i] += force;
        }

    }

    public void calcSpringForces( int lastIndex, List<Vector3> Forces, List<Vector3> Positions,
        float spacing, float stiffness, float shearStiff, float bendingStiff)
    {
        int currentCol = 0; //current
        int currentRow = 0;

        for (int index = 0; index < Positions.Count; index++)
        {

            currentCol = index % (lastIndex + 1);
            currentRow = index / (lastIndex + 1);

            if (currentCol == 0 || currentCol == (lastIndex) || currentRow == 0
                || currentRow == (lastIndex)) { continue; }


            Vector3 force = Vector3.zero;

            for (int i = 0; i < findSprings.instance.structurals[index].Length; i++)
            {

                Vector3 diff = Positions[findSprings.instance.structurals[index][i]] - Positions[index];
                float distance = diff.magnitude;
                float initialLen = spacing;

                force += stiffness * (distance - initialLen) * diff / distance;
            }
            for (int i = 0; i < findSprings.instance.shears[index].Length; i++)
            {
                Vector3 diff = Positions[findSprings.instance.shears[index][i]] - Positions[index];
                float distance = diff.magnitude;
                float initialLen = spacing * Mathf.Sqrt(2);
                force += shearStiff * (distance - initialLen) * diff / distance;
            }
            for (int i = 0; i < findSprings.instance.bends[index].Length; i++)
            {
                Vector3 diff = Positions[findSprings.instance.bends[index][i]] - Positions[index];
                float distance = diff.magnitude;
                float initialLen = spacing * 2;
                force += bendingStiff * (distance - initialLen) * diff / distance;

            }

            Forces[index] += force;
        }
    }
}
