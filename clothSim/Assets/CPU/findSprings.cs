using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class findSprings : MonoBehaviour
{

    public static findSprings instance;
    public List<int[]> structurals = new List<int[]>();
    public List<int[]> shears = new List<int[]>();
    public List <int[]> bends = new List<int[]>();


    private void Awake()
    {
        if(findSprings.instance==null)
            findSprings.instance = this;
    }

    public void findStructural(int vertexCount, int lastIndex)
    {
        int currentCol = 0; //current
        int currentRow = 0;
        int sCol = 0; //spring
        int sRow = 0;

        for (int i = 0; i < vertexCount; i++)
        {
            currentCol = i % (lastIndex + 1);
            currentRow = i / (lastIndex + 1);
            List<int> struc = new List<int>();
            for (int j = 0; j < vertexCount; j++)
            {
                if (i == j) { continue; }
                sCol = j % (lastIndex + 1);
                sRow = j / (lastIndex + 1);

                if (sCol == currentCol) //수직
                {
                    if (Mathf.Abs(sRow - currentRow) == 1)
                    {
                        struc.Add(j);
                    }
                }
                if (sRow == currentRow) //수평
                {
                    if (Mathf.Abs(sCol - currentCol) == 1)
                    {
                        struc.Add(j);
                    }
                }

            }
            structurals.Add(struc.ToArray());
        }
    }

    public void findShear(int vertexCount, int lastIndex)
    {
        int currentCol = 0; //current
        int currentRow = 0;
        int sCol = 0; //spring
        int sRow = 0;

        for (int i = 0; i < vertexCount; i++)
        {
            currentCol = i % (lastIndex + 1);
            currentRow = i / (lastIndex + 1);
            List<int> shear = new List<int>();
            for (int j = 0; j < vertexCount; j++)
            {
                if (i == j) { continue; }
                sCol = j % (lastIndex + 1);
                sRow = j / (lastIndex + 1);

                if (Mathf.Abs(sRow - currentRow) == 1)
                {
                    if (Mathf.Abs(sCol - currentCol) == 1)
                    {
                        shear.Add(j);
                    }
                }
            }
            shears.Add(shear.ToArray());
        }

    }

    public void findBend(int vertexCount, int lastIndex)
    {
        int currentCol = 0; //current
        int currentRow = 0;
        int sCol = 0; //spring
        int sRow = 0;

        for (int i = 0; i < vertexCount; i++)
        {
            currentCol = i % (lastIndex + 1);
            currentRow = i / (lastIndex + 1);
            List<int> bend = new List<int>();
            for (int j = 0; j < vertexCount; j++)
            {
                if (i == j) { continue; }
                sCol = j % (lastIndex + 1);
                sRow = j / (lastIndex + 1);

                if (Mathf.Abs(sRow - currentRow) == 2)
                {
                    if (Mathf.Abs(sCol - currentCol) == 2 || Mathf.Abs(sCol - currentCol) == 0)
                    {
                        bend.Add(j);
                    }
                }
                if (Mathf.Abs(sCol - currentCol) == 2)
                {
                    if (sRow == currentRow)
                    {
                        bend.Add(j);
                    }
                }
            }
            bends.Add(bend.ToArray());
        }
    }
}
