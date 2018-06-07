using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class TrajectoryPrimitive : MonoBehaviour {

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // public abstract void Execute(IExeParams parameters);

}

interface IPrimitiveParams<T>
{
    T unpack();
}