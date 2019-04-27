using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeploymentSerializer;

public class test : MonoBehaviour {

	// Use this for initialization
	void Start () {
        //DeploymentSerializer.DeploymentSerializer.saveObject(new testClass(13), "persistentClass", true);
        //DeploymentSerializer.DeploymentSerializer.saveObject(new testClass(42), "devClass", false);
        DS_MessageLogger.clearBuildLog();
        DS_MessageLogger.logMessageToBuildFile("ERROR message", SerializerLogType.Error);
        DS_MessageLogger.logMessageToBuildFile("Test Message 346", SerializerLogType.Standard);
        Debug.Log(Application.persistentDataPath);

        foreach (string s in DS_MessageLogger.parseBuildLogsToArray())
        {
            Debug.Log(s);
        }
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
