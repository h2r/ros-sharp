using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity;
using System.Linq;
using UnityEngine;
using RosSharp.RosBridgeClient;

/*
 * This class is a singleton and is responsible for issuing out Group IDs (GIDs) and gripper
 * IDs (SIDs). It also contains a map that is globally available from GID to Group objects.
 * This map is basically used so that GIDs can be used as pointers and hence we can fake 
 * passing by reference.
 */
public class IdGenerator : Singleton<IdGenerator>
{
    public Dictionary<string, Group> GIDtoGroup;

    public IdGenerator()
    {
        GIDtoGroup = new Dictionary<string, Group>();
    }

    /*
     * Code to create a SID. A gripper always belongs to a group. Note, it is up to the caller to
     * add the SID to the Group's SIDToObj map
     */
    public string CreateSID(string gid)
    {
        // code to generate a random string of length 5
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

    /*
     * Code to create a GID.
     */
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
        GIDtoGroup.Add(id, g);
        return id;
    }
}
