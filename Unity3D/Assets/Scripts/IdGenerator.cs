using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity;
using System.Linq;
using UnityEngine;


public class IdGenerator : Singleton<IdGenerator>
{

    private HashSet<string> ids { get; set; }
    public bool PointsToStart { get; set; }

    public IdGenerator()
    {
        Debug.Log("Hihi");
        ids = new HashSet<string>();
        ids.Add("START");
        PointsToStart = false;
    }

    public string CreateId()
    {
        // Here 5 is the lenght of the id
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string id = "";
        int charAmount = 5;
        for (int i = 0; i < charAmount; i++)
        {
            id += chars[Random.Range(0, chars.Length)];
        }
        if (ids.Contains(id))
        {
            return CreateId();
        }
        ids.Add(id);
        //Debug.Log(ids.)
        return id;
    }
}
