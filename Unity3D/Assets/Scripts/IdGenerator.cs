using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity;
using System.Linq;
using UnityEngine;


public class IdGenerator : Singleton<IdGenerator>
{

    private Dictionary<string, GameObject> id_to_obj { get; set; }
    public bool PointsToStart { get; set; }

    public IdGenerator()
    {
        id_to_obj = new Dictionary<string, GameObject>();
        id_to_obj.Add("START", null); 
        PointsToStart = false;
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
        if (id_to_obj.ContainsKey(id))
        {
            return CreateId(go);
        }
        id_to_obj.Add(id, go);
        return id;
    }

    
}
