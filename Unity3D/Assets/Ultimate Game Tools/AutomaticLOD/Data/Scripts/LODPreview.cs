using System;
using UnityEngine;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UltimateGameTools.MeshSimplifier;

public class LODPreview : MonoBehaviour
{
  [Serializable]
  public class ShowcaseObject
  {
    public AutomaticLOD m_automaticLOD;
    public Vector3      m_position;
    public Vector3      m_angles;
    public Vector3      m_rotationAxis = Vector3.up;
    public string       m_description;
  }

  public ShowcaseObject[] ShowcaseObjects;
  public Material         WireframeMaterial;
  public float            MouseSensitvity    = 0.3f;
  public float            MouseReleaseSpeed  = 3.0f;

	void Start ()
  {
    if (ShowcaseObjects != null && ShowcaseObjects.Length > 0)
    {
      for (int i = 0; i < ShowcaseObjects.Length; i++)
      {
        ShowcaseObjects[i].m_description = ShowcaseObjects[i].m_description.Replace("\\n", Environment.NewLine);
      }

      SetActiveObject(0);
    }

    Simplifier.CoroutineFrameMiliseconds = 20;
	}

  void Progress(string strTitle, string strMessage, float fT)
  {
    int nPercent = Mathf.RoundToInt(fT * 100.0f);

    if(nPercent != m_nLastProgress || m_strLastTitle != strTitle || m_strLastMessage != strMessage)
    {
      m_strLastTitle   = strTitle;
      m_strLastMessage = strMessage;
      m_nLastProgress  = nPercent;
    }
  }

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.W))
    {
      m_bWireframe = !m_bWireframe;
      SetWireframe(m_bWireframe);
    }

    if (m_selectedAutomaticLOD != null)
    {
      if (Input.GetMouseButton(0) && Input.mousePosition.y > 100)
      {
        Vector3 v3Angles = ShowcaseObjects[m_nSelectedIndex].m_rotationAxis * -((Input.mousePosition.x - m_fLastMouseX) * MouseSensitvity);
        m_selectedAutomaticLOD.transform.Rotate(v3Angles, Space.Self);
      }
      else if(Input.GetMouseButtonUp(0) && Input.mousePosition.y > 100)
      {
        m_fRotationSpeed = -(Input.mousePosition.x - m_fLastMouseX) * MouseReleaseSpeed;
      }
      else
      {
        Vector3 v3Angles = ShowcaseObjects[m_nSelectedIndex].m_rotationAxis * (m_fRotationSpeed * Time.deltaTime);
        m_selectedAutomaticLOD.transform.Rotate(v3Angles, Space.Self);
      }
    }

    m_fLastMouseX = Input.mousePosition.x;
  }

  void OnGUI()
  {
    int nWidth = 400;

    if(ShowcaseObjects == null)
    {
      return;
    }

    bool bAllowInteract = true;

    if (!string.IsNullOrEmpty(m_strLastTitle) && !string.IsNullOrEmpty(m_strLastMessage))
    {
      bAllowInteract = false;
    }

    GUI.Box(new Rect(0, 0, nWidth + 10, 430), "");

    GUILayout.Label("Select model:", GUILayout.Width(nWidth));

    GUILayout.BeginHorizontal();

    for (int i = 0; i < ShowcaseObjects.Length; i++)
    {
      if (GUILayout.Button(ShowcaseObjects[i].m_automaticLOD.name) && bAllowInteract)
      {
        if (m_selectedAutomaticLOD != null)
        {
          DestroyImmediate(m_selectedAutomaticLOD.gameObject);
        }

        SetActiveObject(i);
      }
    }

    GUILayout.EndHorizontal();

    if (m_selectedAutomaticLOD != null)
    {
      GUILayout.Space(20);
      GUILayout.Label(ShowcaseObjects[m_nSelectedIndex].m_description);
      GUILayout.Space(20);

      GUI.changed = false;
      m_bWireframe = GUILayout.Toggle(m_bWireframe, "Show wireframe");

      if (GUI.changed && m_selectedAutomaticLOD != null)
      {
        SetWireframe(m_bWireframe);
      }

      GUILayout.Space(20);

      GUILayout.Label("Select predefined LOD:");

      GUILayout.BeginHorizontal();

      for (int i = 0; i < m_selectedAutomaticLOD.GetLODLevelCount(); i++)
      {
        if (GUILayout.Button("LOD " + i) && bAllowInteract)
        {
          m_selectedAutomaticLOD.SwitchToLOD(i, true);
        }
      }

      GUILayout.EndHorizontal();

      GUILayout.Space(20);

      GUILayout.Label("Vertex count: " + m_selectedAutomaticLOD.GetCurrentVertexCount(true) + "/" + m_selectedAutomaticLOD.GetOriginalVertexCount(true));

      GUILayout.Space(20);

      if (!string.IsNullOrEmpty(m_strLastTitle) && !string.IsNullOrEmpty(m_strLastMessage))
      {
        GUILayout.Label(m_strLastTitle + ": " + m_strLastMessage, GUILayout.MaxWidth(nWidth));
        GUI.color = Color.blue;

        Rect lastRect = GUILayoutUtility.GetLastRect();
        GUI.Box(new Rect(10, lastRect.yMax + 5, 204, 24), "");
        GUI.Box(new Rect(12, lastRect.yMax + 7, m_nLastProgress * 2, 20), "");
      }
      else
      {
        GUILayout.Label("Vertices: " + (m_fVertexAmount * 100.0f).ToString("0.00") + "%");
        m_fVertexAmount = GUILayout.HorizontalSlider(m_fVertexAmount, 0.0f, 1.0f, GUILayout.Width(200));

        GUILayout.BeginHorizontal();
        GUILayout.Space(3);

        if (GUILayout.Button("Compute custom LOD", GUILayout.Width(200)))
        {
          StartCoroutine(ComputeLODWithVertices(m_fVertexAmount));
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
      }
    }
  }

  private void SetActiveObject(int index)
  {
    m_nSelectedIndex = index;

    AutomaticLOD automaticLOD = Instantiate(ShowcaseObjects[index].m_automaticLOD);
    automaticLOD.transform.position = ShowcaseObjects[index].m_position;
    automaticLOD.transform.rotation = Quaternion.Euler(ShowcaseObjects[index].m_angles);

    m_selectedAutomaticLOD = automaticLOD;
    automaticLOD.SetAutomaticCameraLODSwitch(false);

    m_objectMaterials = new Dictionary<GameObject, Material[]>();
    AddMaterials(automaticLOD.gameObject, m_objectMaterials);

    m_bWireframe = false;
  }

  private void AddMaterials(GameObject theGameObject, Dictionary<GameObject, Material[]> dicMaterials)
  {
    Renderer     theRenderer  = theGameObject.GetComponent<Renderer>();
    AutomaticLOD automaticLOD = theGameObject.GetComponent<AutomaticLOD>();

    if (theRenderer != null && theRenderer.sharedMaterials != null && automaticLOD != null)
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

  private IEnumerator ComputeLODWithVertices(float fAmount)
  {
    foreach (KeyValuePair<GameObject, Material[]> pair in m_objectMaterials)
    {
      AutomaticLOD        automaticLOD = pair.Key.GetComponent<AutomaticLOD>();
      MeshFilter          meshFilter   = pair.Key.GetComponent<MeshFilter>();
      SkinnedMeshRenderer skin         = pair.Key.GetComponent<SkinnedMeshRenderer>();

      if (automaticLOD && (meshFilter != null || skin != null))
      {
        Mesh newMesh = null;

        if (meshFilter != null)
        {
          newMesh = Mesh.Instantiate(meshFilter.sharedMesh);
        }
        else if (skin != null)
        {
          newMesh = Mesh.Instantiate(skin.sharedMesh);
        }

        automaticLOD.GetMeshSimplifier().CoroutineEnded = false;

        StartCoroutine(automaticLOD.GetMeshSimplifier().ComputeMeshWithVertexCount(pair.Key, newMesh, Mathf.RoundToInt(fAmount * automaticLOD.GetMeshSimplifier().GetOriginalMeshUniqueVertexCount()), automaticLOD.name, Progress));

        while (automaticLOD.GetMeshSimplifier().CoroutineEnded == false)
        {
          yield return null;
        }

        if (meshFilter != null)
        {
          meshFilter.mesh = newMesh;
        }
        else if (skin != null)
        {
          skin.sharedMesh = newMesh;
        }
      }
    }

    m_strLastTitle   = "";
    m_strLastMessage = "";
    m_nLastProgress  = 0;
  }

  Dictionary<GameObject, Material[]> m_objectMaterials;
  AutomaticLOD                       m_selectedAutomaticLOD;
  int                                m_nSelectedIndex = -1;
  bool                               m_bWireframe;
  float                              m_fRotationSpeed = 10.0f;
  float                              m_fLastMouseX;

  Mesh                               m_newMesh;
  int                                m_nLastProgress  = -1;
  string                             m_strLastTitle   = "";
  string                             m_strLastMessage = "";

  float                              m_fVertexAmount  = 1.0f;

}
