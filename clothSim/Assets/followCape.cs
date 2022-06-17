using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class followCape : MonoBehaviour
{
    public Transform leftCape, rightCape;
    public GameObject leftmiddle, rightmiddle;
    public Vector3 offset;
    private void Start()
    {
        //offset = new Vector3(0.180000007f, -3.01999998f, -2.06999993f);
        offset = new Vector3(0.150000006f, -3.5f, 0);
        //Vector3(0.0799999982,-3.31999993,-0.360000014)
    }
    void Update()
    {
        transform.position = leftmiddle.transform.position + offset;

    }
}
