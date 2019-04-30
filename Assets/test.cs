using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeploymentSerializer;

public class test : MonoBehaviour {

	// Use this for initialization
	void Start () {      
        Debug.Log(Application.persistentDataPath);

	}
}

[System.Serializable]
public class testClass
{
    public int t;

    public testClass (int x)
    {
        t = x;
    }    
}
