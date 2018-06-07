using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearPrimitive : TrajectoryPrimitive {

    // Use this for initialization

    public LinearPrimitive(LinearPrimitiveParams parameters)
    {

    }

    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		// where the rendering should happen

	}

    
    
}

public class LinearPrimitiveParams : IPrimitiveParams<List<GeometryPose>>
{
    private List<GeometryPose> data;

    public LinearPrimitiveParams(ref GeometryPose start, ref GeometryPose end)
    {
        data = new List<GeometryPose>()
        {
            start,
            end
        };
    }

    public List<GeometryPose> unpack()
    {
        return data;
    }
}