using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity;
using System.Linq;
using UnityEngine;
using RosSharp.RosBridgeClient;

public class IdGenerator : Singleton<IdGenerator>
{

    public Dictionary<string, GameObject> IdToObj { get; set; }
    public Dictionary<string, bool> OutOfBounds { get; set; }
    public int NumPoints { get; set; }
    public GameObject FirstWaypoint { get; set; }

    public IdGenerator()
    {
        IdToObj = new Dictionary<string, GameObject>();
        IdToObj.Add("START", null);
        OutOfBounds = new Dictionary<string, bool>();
        FirstWaypoint = null;
        NumPoints = 1;
    }

    public string CreateId(GameObject go)
    {
        // Here 5 is the lenght of the id
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string id = "";
        int charAmount = 5;
        for (int i = 0; i < charAmount; i++)
        {
            id += chars[Random.Range(0, chars.Length)];
        }
        if (IdToObj.ContainsKey(id))
        {
            return CreateId(go);
        }
        IdToObj.Add(id, go);
        return id;
    }

    public void SetOutOfBounds(string id)
    {
        IdToObj[id].GetComponent<TargetModelBehavior>().MakeRed();
        OutOfBounds[id] = true;
    }

    public void SetInBounds(string id)
    {
        IdToObj[id].GetComponent<TargetModelBehavior>().MakeGreen();
        OutOfBounds[id] = false;
    }
    
}
