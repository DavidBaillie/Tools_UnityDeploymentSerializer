using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeploymentSerializer;

public class test : MonoBehaviour {

    bool run = true;
    int count = 0;
    testClass one;
    testClass two;
    testClass three;

    // Use this for initialization
    void Awake ()
    {
        ObjectSerializer.testCall();

        /*
        Debug.Log(Application.persistentDataPath);
        DS_MessageLogger.clearBuildLog();
        ObjectSerializer.saveObject(new testClass(27), "persistentSaveOne", true);
        ObjectSerializer.saveObject(new testClass(38), "persistentSaveTwo", true);
        ObjectSerializer.saveObject(new testClass(11), "developerSaveOne", false);
        */
    }

    private void Update()
    {
        /*
        count++;

        if (run && count > 50)
        {
            run = false;
            one = ObjectSerializer.loadObject<testClass>("persistentSaveOne", true);
            Debug.Log(one.t);
            two = ObjectSerializer.loadObject<testClass>("persistentSaveTwo", true);
            Debug.Log(two == null ? "Two is null" : two.t.ToString());
            three = ObjectSerializer.loadObject<testClass>("developerSaveOne", false);
            Debug.Log(three.t);
        }
        */
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
