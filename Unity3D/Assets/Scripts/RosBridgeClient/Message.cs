/*
© Siemens AG, 2017-2018
Author: Dr. Martin Bischoff (martin.bischoff@siemens.com)

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at
<http://www.apache.org/licenses/LICENSE-2.0>.
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using Newtonsoft.Json;


public class Message {
    [JsonIgnore]
    public string RosMessageType {
        get { return MessageTypes.RosMessageType(GetType()); }
    }
}

public class MoveitTarget : Message {
    public GeometryPose left_arm;
    public GeometryPose right_arm;
    public StandardString left_open;
    public StandardString right_open;
    public StandardString sid;
    public StandardString gid;
    public StandardString prev_id;
    public StandardString next_id;
    public MoveitTarget() {
        left_arm = new GeometryPose();
        right_arm = new GeometryPose();
        left_open = new StandardString();
        right_open = new StandardString();
        sid = new StandardString();
        gid = new StandardString();
        prev_id = new StandardString();
        next_id = new StandardString();

        left_open.data = "0";
        right_open.data = "0";
        sid.data = "";
        gid.data = "";
        prev_id.data = "";
        next_id.data = "";


    }
}

public class MoveItDisplayTrajectory : Message {
    public string model_id;
    public MoveItRobotTrajectory[] trajectory;
    public MoveItRobotState trajectory_start;
    public MoveItDisplayTrajectory() {
        model_id = "";
        trajectory = new MoveItRobotTrajectory[0];
        trajectory_start = new MoveItRobotState();
    }
}

// wrapper for a MoveItRobotTrajectory with an id field -Samir
public class MoveitPlan : Message
{
    public string id;
    public MoveItRobotTrajectory plan;
    public MoveitPlan()
    {
        id = "";
        plan = new MoveItRobotTrajectory();
    }
}

public class MoveItRobotTrajectory : Message {
    public TrajectoryJointTrajectory joint_trajectory;
    public TrajectoryMultiDOFJointTrajectory multi_dof_joint_trajectory;
    public MoveItRobotTrajectory() {
        joint_trajectory = new TrajectoryJointTrajectory();
        multi_dof_joint_trajectory = new TrajectoryMultiDOFJointTrajectory();
    }
}

public class TrajectoryJointTrajectory : Message {
    public StandardHeader header;
    public string[] joint_names;
    public TrajectoryJointTrajectoryPoint[] points;
    public TrajectoryJointTrajectory() {
        header = new StandardHeader();
        joint_names = new string[0];
        points = new TrajectoryJointTrajectoryPoint[0];
    }
}

public class TrajectoryJointTrajectoryPoint : Message {
    public float[] positions;
    public float[] velocities;
    public float[] accelerations;
    public float[] effort;
    public StandardTime time_from_start;
    public TrajectoryJointTrajectoryPoint() {
        positions = new float[0];
        velocities = new float[0];
        accelerations = new float[0];
        effort = new float[0];
        time_from_start = new StandardTime();
    }
}

public class TrajectoryMultiDOFJointTrajectory : Message {
    public StandardHeader header;
    public string[] joint_names;
    public TrajectoryMulitDOFJointTrajectoryPoint[] points;
    public TrajectoryMultiDOFJointTrajectory() {
        header = new StandardHeader();
        joint_names = new string[0];
        points = new TrajectoryMulitDOFJointTrajectoryPoint[0];
    }
}

public class TrajectoryMulitDOFJointTrajectoryPoint : Message {
    public GeometryTransform[] transforms;
    public GeometryTwist[] velocities;
    public GeometryTwist[] accelerations;
    public StandardTime time_from_start;
    public TrajectoryMulitDOFJointTrajectoryPoint() {
        transforms = new GeometryTransform[0];
        velocities = new GeometryTwist[0];
        accelerations = new GeometryTwist[0];
        time_from_start = new StandardTime();
    }
}

public class MoveItRobotState : Message {
    public SensorJointStates joint_state;
    public SensorMultiDOFJointState attached_dof_joint_state;
    public MoveItAttachedCollisionObject[] attached_collision_objects;
    public bool is_diff;
    public MoveItRobotState() {
        joint_state = new SensorJointStates();
        attached_dof_joint_state = new SensorMultiDOFJointState();
        attached_collision_objects = new MoveItAttachedCollisionObject[0];
    }
}

public class MoveItAttachedCollisionObject : Message {
    public string link_name;
    public MoveItCollisionObject obj;
    public string[] touch_links;
    public TrajectoryJointTrajectory detach_posture;
    public float weight;
    public MoveItAttachedCollisionObject() {
        link_name = "";
        obj = new MoveItCollisionObject();
        touch_links = new string[0];
        detach_posture = new TrajectoryJointTrajectory();
        weight = 0f;
    }
}

public class MoveItCollisionObject : Message {
    public StandardHeader header;
    public string id;
    public ObjectRecognitionObjectType type;
    public ShapeSolidPrivitive[] primitives;
    public GeometryPose[] primitive_poses;
    public ShapeMesh[] meshes;
    public GeometryPose[] mesh_poses;
    public ShapePlane[] planes;
    public GeometryPose[] plane_poses;
    public byte operation;
    public byte ADD = 0;
    public byte REMOVE = 1;
    public byte APPEND = 2;
    public byte MOVE = 3;
    public MoveItCollisionObject() {
        header = new StandardHeader();
        id = "";
        type = new ObjectRecognitionObjectType();
        primitives = new ShapeSolidPrivitive[0];
        primitive_poses = new GeometryPose[0];
        meshes = new ShapeMesh[0];
        mesh_poses = new GeometryPose[0];
        planes = new ShapePlane[0];
        plane_poses = new GeometryPose[0];
        operation = 0;
    }
}

public class ObjectRecognitionObjectType : Message {
    public string key;
    public string db;
    public ObjectRecognitionObjectType() {
        key = "";
        db = "";
    }
}

public class ShapeSolidPrivitive : Message {
    public byte BOX = 1;
    public byte SPHERE = 2;
    public byte CYLINDER = 3;
    public byte CONE = 4;
    public byte BOX_X = 0;
    public byte BOX_Y = 1;
    public byte BOX_Z = 2;
    public byte SPHERE_RADIUS = 0;
    public byte CYLINDER_HEIGHT = 0;
    public byte CYLINDER_RADIUS = 1;
    public byte CONE_HEIGHT = 0;
    public byte CONE_RADIUS = 1;
    public byte type;
    public float[] dimensions;
    public ShapeSolidPrivitive() {
        type = 0;
        dimensions = new float[0];
    }
}

public class ShapeMesh : Message {
    public ShapeMeshTriangle[] triangles;
    public GeometryPoint[] verticies;
    public ShapeMesh() {
        triangles = new ShapeMeshTriangle[0];
        verticies = new GeometryPoint[0];
    }
}

public class ShapeMeshTriangle : Message {
    public uint[] vertex_indicies;
    public ShapeMeshTriangle() {
        vertex_indicies = new uint[3];
    }
}

public class ShapePlane : Message {
    public float[] coef;
    public ShapePlane() {
        coef = new float[4];
    }
}

public class GeometryTransform : Message {
    public GeometryVector3 translation;
    public GeometryQuaternion rotation;
    public GeometryTransform() {
        translation = new GeometryVector3();
        rotation = new GeometryQuaternion();
    }
}

public class GeometryTwist : Message {
    public GeometryVector3 linear;
    public GeometryVector3 angular;
    public GeometryTwist() {
        linear = new GeometryVector3();
        angular = new GeometryVector3();
    }
}
public class StandardString : Message {
    public string data;
    public StandardString() {
        data = "";
    }
}

public class GeometryAccel : Message {
    public GeometryVector3 linear;
    public GeometryVector3 angular;
    public GeometryAccel() {
        linear = new GeometryVector3();
        angular = new GeometryVector3();
    }
}

public class SensorJointStates : Message {
    public StandardHeader header;
    public string[] name;
    public float[] position;
    public float[] velocity;
    public float[] effort;
    public SensorJointStates() {
        header = new StandardHeader();
        name = new string[0];
        position = new float[0];
        velocity = new float[0];
        effort = new float[0];
    }
}

public class SensorMultiDOFJointState : Message {
    public StandardHeader header;
    public string[] joint_names;
    public GeometryTransform[] transforms;
    public GeometryTwist[] twist;
    public GeometryWrench[] wrench;
    public SensorMultiDOFJointState() {
        header = new StandardHeader();
        joint_names = new string[0];
        transforms = new GeometryTransform[0];
        twist = new GeometryTwist[0];
        wrench = new GeometryWrench[0];
    }
}

public class GeometryWrench : Message {
    public GeometryVector3 force;
    public GeometryVector3 torque;
    public GeometryWrench() {
        force = new GeometryVector3();
        torque = new GeometryVector3();
    }
}
public class GeometryVector3 : Message {
    public float x;
    public float y;
    public float z;
    public GeometryVector3() {
        x = 0f;
        y = 0f;
        z = 0f;
    }
}
public class SensorJoy : Message {
    public StandardHeader header;
    public float[] axes;
    public int[] buttons;

    public SensorJoy() {
        header = new StandardHeader();
        axes = new float[0];
        buttons = new int[0];
    }
}

public class NavigationOdometry : Message {
    public StandardHeader header;
    public string child_frame_id;
    public GeometryPoseWithCovariance pose;
    public GeometryTwistWithCovariance twist;
    public NavigationOdometry() {
        header = new StandardHeader();
        child_frame_id = "";
        pose = new GeometryPoseWithCovariance();
        twist = new GeometryTwistWithCovariance();
    }
}
public class StandardHeader : Message {
    public int seq;
    public StandardTime stamp;
    public string frame_id;
    public StandardHeader() {
        seq = 0;
        stamp = new StandardTime();
        frame_id = "";
    }
}

public class GeometryPoseWithCovariance : Message {
    public GeometryPose pose;
    public float[] covariance;
    public GeometryPoseWithCovariance() {
        pose = new GeometryPose();
        covariance = new float[32];
    }
}
public class GeometryTwistWithCovariance : Message {
    public GeometryTwist twist;
    public float[] covariance;
    public GeometryTwistWithCovariance() {
        twist = new GeometryTwist();
        covariance = new float[32];
    }
}

public class GeometryPose : Message {
    public GeometryPoint position;
    public GeometryQuaternion orientation;
    public GeometryPose() {
        position = new GeometryPoint();
        orientation = new GeometryQuaternion();
    }
}

public class GeometryPoseStamped : Message {
    public StandardHeader header;
    public GeometryPose pose;
    public GeometryPoseStamped() {
        header = new StandardHeader();
        pose = new GeometryPose();
    }
}

public class GeometryPoint : Message {
    public float x;
    public float y;
    public float z;
    public GeometryPoint() {
        x = 0;
        y = 0;
        z = 0;
    }
}
public class GeometryQuaternion : Message {
    public float x;
    public float y;
    public float z;
    public float w;
    public GeometryQuaternion() {
        x = 0;
        y = 0;
        z = 0;
        w = 0;
    }
}
public class SensorPointCloud2 : Message {
    public StandardHeader header;
    public int height;
    public int width;
    public SensorPointField[] fields;
    public bool is_bigendian;
    public int point_step;
    public int row_step;

    public byte[] data;
    public bool is_dense;
    public SensorPointCloud2() {
        header = new StandardHeader();
        height = 0;
        width = 0;
        fields = new SensorPointField[0];
        is_bigendian = false;
        point_step = 0;
        row_step = 0;
        is_dense = false;
        data = new byte[0];
    }
}
public class SensorPointField : Message {
    public int datatype;
    public string name;
    public int offset;
    public int count;
    public SensorPointField() {
        datatype = 0;
        name = "";
        offset = 0;
        count = 0;
    }
}
public class SensorImage : Message {
    public StandardHeader header;
    public int height;
    public int width;
    public string encoding;
    public bool is_bigendian;
    public int step;
    public byte[] data;
    public SensorImage() {
        header = new StandardHeader();
        height = 0;
        width = 0;
        encoding = "undefined";
        is_bigendian = false;
        step = 0;
        data = new byte[0];
    }
}
public class SensorCompressedImage : Message {
    public StandardHeader header;
    public string format;
    public byte[] data;
    public SensorCompressedImage() {
        header = new StandardHeader();
        format = "";
        data = new byte[0];
    }
}

public class StandardTime : Message {
    public int secs;
    public int nsecs;
    public StandardTime() {
        secs = 0;
        nsecs = 0;
    }
}

public class NavigationMapMetaData : Message {
    public StandardTime map_load_time;
    public float resolution;
    public uint width;
    public uint height;
    public GeometryPose origin;

    public NavigationMapMetaData() {
        map_load_time = null;
        resolution = 0;
        width = 0;
        height = 0;
        origin = new GeometryPose();
    }
}

public class NavigationOccupancyGrid : Message {
    public StandardHeader header;
    public NavigationMapMetaData info;
    public sbyte[] data;
    public NavigationOccupancyGrid() {
        header = new StandardHeader();
        info = new NavigationMapMetaData();
        data = null;
    }
}

public class ParamName : Message {
    public string name;
    public ParamName(string _name) {
        name = _name;
    }
}

public class SetParam : Message {
    public string name;
    public string value;
    public SetParam(string _name, string _value) {
        name = _name;
        value = _value;
    }
}

public class ParamValueString : Message {
    public string value;
}

public class ParamValueByte : Message {
    public byte[] value;
}

