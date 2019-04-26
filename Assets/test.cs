using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour {

	// Use this for initialization
	void Start () {
        //Debug.Log(typeof(DeploymentSerializer.DeploymentSerializer).Name);
        DeploymentSerializer.DeploymentSerializer.saveObject(new testClass(), "newFileNameTwo", true);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

[System.Serializable]
public class testClass
{
    public int t;
}

public class nClass
{
    public int c;
}
