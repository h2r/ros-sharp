using System;
using UnityEngine;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UltimateGameTools.MeshSimplifier;

public class LODSampleLODScene : MonoBehaviour
{
  [Serializable]
  public class SceneCamera
  {
    public Camera m_camera;
    public float  m_near;
    public float  m_far;

    [HideInInspector]
    public Vector3 m_v3InitialCameraPosition;
    [HideInInspector]
    public Vector3 m_v3ViewDir;
  }

  public SceneCamera[] SceneCameras;
  public Material WireframeMaterial;

	void Start ()
  {
    // Enumerate all our objects with automatic LOD components

    AutomaticLOD[] automaticLODObjects = FindObjectsOfType<AutomaticLOD>();
    m_sceneLODObjects = new List<AutomaticLOD>();

    m_objectMaterials = new Dictionary<GameObject, Material[]>();
    m_nMaxLODLevels = 0;

    foreach (AutomaticLOD automaticLOD in automaticLODObjects)
    {
      if (automaticLOD.IsRootAutomaticLOD())
      {
        m_sceneLODObjects.Add(automaticLOD);

        if (automaticLOD.GetLODLevelCount() > m_nMaxLODLevels)
        {
          m_nMaxLODLevels = automaticLOD.GetLODLevelCount();
        }

        AddMaterials(automaticLOD.gameObject, m_objectMaterials);
      }
    }

    // Setup camera

    if (SceneCameras != null && SceneCameras.Length > 0)
    {
      foreach(SceneCamera sceneCamera in SceneCameras)
      {
        sceneCamera.m_v3InitialCameraPosition = sceneCamera.m_camera.transform.position;
        sceneCamera.m_v3ViewDir               = sceneCamera.m_camera.transform.forward;
      }

      SetActiveCamera(0);
    }

    m_bWireframe = false;
	}

  void Update()
  {
    m_nCamMode = 0;

    if(Input.GetKey(KeyCode.I))
    {
      m_nCamMode = 1;
    }
    else if (Input.GetKey(KeyCode.O))
    {
      m_nCamMode = -1;
    }

    if (m_nCamMode != 0)
    {
      m_fCurrentDistanceSlider -= Time.deltaTime * 0.1f * m_nCamMode;
      m_fCurrentDistanceSlider = Mathf.Clamp01(m_fCurrentDistanceSlider);
      UpdateCamera(m_fCurrentDistanceSlider);
    }

    if (Input.GetKeyDown(KeyCode.W))
    {
      m_bWireframe = !m_bWireframe;
      SetWireframe(m_bWireframe);
    }
  }

  void OnGUI()
  {
    int nWidth = 400;

    if(SceneCameras == null)
    {
      return;
    }

    if (SceneCameras.Length == 0)
    {
      return;
    }

    GUI.Box(new Rect(0, 0, nWidth + 10, 260), "");

    GUILayout.Space(20);
    GUILayout.Label("Select camera:", GUILayout.Width(nWidth));

    GUILayout.BeginHorizontal();

    for (int i = 0; i < SceneCameras.Length; i++)
    {
      if (GUILayout.Button(SceneCameras[i].m_camera.name))
      {
        SetActiveCamera(i);
      }
    }

    GUILayout.EndHorizontal();

    GUILayout.Label("Camera distance:", GUILayout.Width(nWidth));
    GUI.changed = false;
    m_fCurrentDistanceSlider = GUILayout.HorizontalSlider(m_fCurrentDistanceSlider, 0.0f, 1.0f);
    if (GUI.changed)
    {
      UpdateCamera(m_fCurrentDistanceSlider);
    }

    GUI.changed = false;
    m_bWireframe = GUILayout.Toggle(m_bWireframe, "Show wireframe");

    if (GUI.changed)
    {
      SetWireframe(m_bWireframe);
    }

    GUILayout.Space(20);

    GUILayout.Label("Select LOD:");

    GUILayout.BeginHorizontal();

    if (GUILayout.Button("Automatic LOD"))
    {
      foreach(AutomaticLOD automaticLOD in m_sceneLODObjects)
      {
        automaticLOD.SetAutomaticCameraLODSwitch(true);
      }
    }

    for (int i = 0; i < m_nMaxLODLevels; i++)
    {
      if (GUILayout.Button("LOD " + i))
      {
        foreach(AutomaticLOD automaticLOD in m_sceneLODObjects)
        {
          automaticLOD.SetAutomaticCameraLODSwitch(false);
          automaticLOD.SwitchToLOD(i, true);
        }
      }
    }

    GUILayout.EndHorizontal();

    GUILayout.Space(20);

    int nLODVertexCount   = 0;
    int nTotalVertexCount = 0;

    foreach(AutomaticLOD automaticLOD in m_sceneLODObjects)
    {
      nLODVertexCount   += automaticLOD.GetCurrentVertexCount(true);
      nTotalVertexCount += automaticLOD.GetOriginalVertexCount(true);
    }

    GUILayout.Label("Vertex count: " + nLODVertexCount + "/" + nTotalVertexCount + " " + Mathf.RoundToInt(100.0f * ((float)nLODVertexCount / (float)nTotalVertexCount)).ToString() + "%");

    GUILayout.Space(20);
  }

  private void SetActiveCamera(int index)
  {
    foreach (SceneCamera sceneCamera in SceneCameras)
    {
      sceneCamera.m_camera.gameObject.SetActive(false);
    }

    m_selectedCamera = SceneCameras[index];

    m_selectedCamera.m_camera.gameObject.SetActive(true);
    m_selectedCamera.m_camera.transform.position = m_selectedCamera.m_v3InitialCameraPosition;
    m_fCurrentDistanceSlider = m_selectedCamera.m_near / (m_selectedCamera.m_near - m_selectedCamera.m_far);
  }

  private void UpdateCamera(float fPos)
  {
    Vector3 v3Position = Vector3.Lerp(m_selectedCamera.m_v3InitialCameraPosition + (m_selectedCamera.m_v3ViewDir * m_selectedCamera.m_near),
                                      m_selectedCamera.m_v3InitialCameraPosition + (m_selectedCamera.m_v3ViewDir * m_selectedCamera.m_far),
                                      fPos);

    m_selectedCamera.m_camera.transform.position = v3Position;
  }

  private void AddMaterials(GameObject theGameObject, Dictionary<GameObject, Material[]> dicMaterials)
  {
    Renderer     theRenderer  = theGameObject.GetComponent<Renderer>();
    AutomaticLOD automaticLOD = theGameObject.GetComponent<AutomaticLOD>();

    if (theRenderer != null && theRenderer.sharedMaterials != null && automaticLOD != null && automaticLOD != null)
    {
      dicMaterials.Add(theGameObject, theRenderer.sharedMaterials);
    }

    for (int i = 0; i < theGameObject.transform.childCount; i++)
    {
      AddMaterials(theGameObject.transform.GetChild(i).gameObject, dicMaterials);
    }
  }

  private void SetWireframe(bool bEnabled)
  {
    m_bWireframe = bEnabled;

    foreach (KeyValuePair<GameObject, Material[]> pair in m_objectMaterials)
    {
      Renderer theRenderer = pair.Key.GetComponent<Renderer>();

      if (bEnabled)
      {
        Material[] materials = new Material[pair.Value.Length];

        for (int i = 0; i < pair.Value.Length; i++)
        {
          materials[i] = WireframeMaterial;
        }

        theRenderer.sharedMaterials = materials;
      }
      else
      {
        theRenderer.sharedMaterials = pair.Value;
      }
    }
  }

  Dictionary<GameObject, Material[]> m_objectMaterials;
  SceneCamera                        m_selectedCamera;
  bool                               m_bWireframe;
  List<AutomaticLOD>                 m_sceneLODObjects;
  int                                m_nMaxLODLevels;
  float                              m_fCurrentDistanceSlider;

  int                                m_nCamMode = 0; // -1 = zoom out, 1 = zoom in
}
