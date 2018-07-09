using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity;
using System.Linq;
using UnityEngine;
using RosSharp.RosBridgeClient;

public class IdGenerator : Singleton<IdGenerator>
{
    public Dictionary<string, Group> GIDtoGroup;

    //public Dictionary<string, GameObject> IdToObj { get; set; }
    //public Dictionary<string, bool> OutOfBounds { get; set; }
    //public int NumPoints { get; set; }
    //public GameObject FirstWaypoint { get; set; }

    public IdGenerator()
    {
        GIDtoGroup = new Dictionary<string, Group>();
    }

    public string CreateSID(string gid)
    {
        // Here 5 is the length of the id
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string id = "";
        int charAmount = 5;
        for (int i = 0; i < charAmount; i++)
        {
            id += chars[Random.Range(0, chars.Length)];
        }
        if (GIDtoGroup[gid].SIDToObj.ContainsKey(id))
        {
            return CreateSID(gid);
        }
        return id;
    }

    public string CreateGID(Group g)
    {
        // Here 3 is the length of the id
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string id = "";
        int charAmount = 3;
        for (int i = 0; i < charAmount; i++)
        {
            id += chars[Random.Range(0, chars.Length)];
        }
        if (GIDtoGroup.ContainsKey(id))
        {
            return CreateGID(g);
        }
        Debug.Log("GID added: " + id);
        GIDtoGroup.Add(id, g);
        return id;
    }
}
