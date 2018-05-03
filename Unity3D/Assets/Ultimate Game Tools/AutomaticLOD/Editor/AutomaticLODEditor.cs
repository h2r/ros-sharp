using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UltimateGameTools.MeshSimplifier;

[CustomEditor(typeof(AutomaticLOD)), CanEditMultipleObjects]
public class AutomaticLODEditor : Editor
{
  void Progress(string strTitle, string strMessage, float fT)
  {
    int nPercent = Mathf.RoundToInt(fT * 100.0f);

    if(nPercent != s_nLastProgress || s_strLastTitle != strTitle || s_strLastMessage != strMessage)
    {
      s_strLastTitle   = strTitle;
      s_strLastMessage = strMessage;
      s_nLastProgress  = nPercent;

      if(EditorUtility.DisplayCancelableProgressBar(strTitle, strMessage, fT))
      {
        Simplifier.Cancelled = true;
      }
    }
  }

  void OnEnable()
  {
    m_nCurrentLODResizing = -1;
    m_nCurrentLODSelected = -1;
    m_bPreviewingLOD      = false;

    Random.InitState(4);

    if(s_bStaticDataLoaded == false || !s_texBlack || !s_texDarkGray || !s_texArrow || !s_texHandle || !s_texLODGroupSelected || !s_texLODGroupNotSelected)
    {
      LoadStaticData();
    }

    if (serializedObject == null)
    {
      return;
    }

    PropertyGenerateIncludeChildren   = serializedObject.FindProperty("m_bGenerateIncludeChildren");
    PropertyLevelsToGenerate          = serializedObject.FindProperty("m_levelsToGenerate");
    PropertySwitchMode                = serializedObject.FindProperty("m_switchMode"); 
    PropertyEvalMode                  = serializedObject.FindProperty("m_evalMode");
    PropertyEnablePrefabUsage         = serializedObject.FindProperty("m_bEnablePrefabUsage");
    PropertyMaxCameraDistanceDistance = serializedObject.FindProperty("m_fMaxCameraDistance");
    PropertyListLODLevels             = serializedObject.FindProperty("m_listLODLevels");
    PropertyExpandRelevanceSpheres    = serializedObject.FindProperty("m_bExpandRelevanceSpheres");
    PropertyRelevanceSpheres          = serializedObject.FindProperty("m_aRelevanceSpheres");
    PropertyOverrideRootSettings      = serializedObject.FindProperty("m_bOverrideRootSettings");
    PropertyLODDataDirty              = serializedObject.FindProperty("m_bLODDataDirty");

    m_bGenerateLODData         = false;
    m_bRecomputeAllMeshes      = false;
    m_bEnablePrefabUsage       = false;
    m_bDisablePrefabUsage      = false;
    m_bDeleteLODData           = false;
    m_bRemoveFromLODTree       = false;
    m_bSetupNewRelevanceSphere = false;

    m_fPreviewLODPos     = 0.0f;

    EditorApplication.update -= Update;
    EditorApplication.update += Update;

    SetHideFlags();
  }

  void OnDisable()
  {
    RestoreOriginalMeshes();

    EditorApplication.update -= Update;
  }

  void Update()
  {
    AutomaticLOD automaticLOD = target as AutomaticLOD;

    // Check for changes in UnityLODGroup mode to restore the LODGroup LOD handling if a specific LOD was forced by selecting on a LOD box.
    // We will check for changes in the scene view cameras

    if (automaticLOD != null && automaticLOD.LODSwitchMode == AutomaticLOD.SwitchMode.UnityLODGroup && m_dicSceneViewCameraInfos != null)
    {
      bool restoreLODGroupHandling = false;

      for (int i = 0; i < SceneView.sceneViews.Count; i++)
      {
        SceneView sceneView = SceneView.sceneViews[i] as SceneView;

        if (m_dicSceneViewCameraInfos.ContainsKey(sceneView) == false)
        {
          restoreLODGroupHandling = true;
          break;
        }
        else if (sceneView.camera != null)
        {
          if(sceneView.camera.transform.position != m_dicSceneViewCameraInfos[sceneView].m_v3Position ||
             sceneView.pivot    != m_dicSceneViewCameraInfos[sceneView].m_v3Pivot ||
             sceneView.rotation != m_dicSceneViewCameraInfos[sceneView].m_qRotation ||
             Mathf.Approximately(sceneView.size, m_dicSceneViewCameraInfos[sceneView].m_fSize) == false)
          {
            restoreLODGroupHandling = true;
            break;
          }
        }

        sceneView.pivot = m_dicSceneViewCameraInfos[sceneView].m_v3Pivot;
        sceneView.rotation = m_dicSceneViewCameraInfos[sceneView].m_qRotation;
        sceneView.size = m_dicSceneViewCameraInfos[sceneView].m_fSize;
      }

      if (restoreLODGroupHandling && !EditorApplication.isPlaying)
      {
        foreach (Object targetObject in targets)
        {
          automaticLOD = targetObject as AutomaticLOD;
          automaticLOD.SwitchToLOD(-1, false);
        }

        m_nCurrentLODSelected = -1;
        Repaint();
      }
    }
  }

  void OnSceneGUI()
  {
    AutomaticLOD automaticLOD = target as AutomaticLOD;

    bool bDrawSpheres = true;

    if (automaticLOD.m_LODObjectRoot != null)
    {
      if (automaticLOD.m_LODObjectRoot.m_bExpandRelevanceSpheres == false)
      {
        bDrawSpheres = false;
      }
    }
    else
    {
      if (automaticLOD.m_bExpandRelevanceSpheres == false)
      {
        bDrawSpheres = false;
      }
    }

    if (automaticLOD.m_aRelevanceSpheres != null && bDrawSpheres)
    {
      for (int nSphere = 0; nSphere < automaticLOD.m_aRelevanceSpheres.Length; nSphere++)
      {
        if (automaticLOD.m_aRelevanceSpheres[nSphere].m_bExpanded == false)
        {
          continue;
        }

        RelevanceSphere relevanceSphere = automaticLOD.m_aRelevanceSpheres[nSphere] as RelevanceSphere;

        if (Tools.current == Tool.Move)
        {
          EditorGUI.BeginChangeCheck();
          Vector3 v3Position = Handles.PositionHandle(relevanceSphere.m_v3Position, Quaternion.Euler(relevanceSphere.m_v3Rotation));
          if (EditorGUI.EndChangeCheck())
          {
            Undo.RecordObject(automaticLOD, "Move Relevance Sphere");
            relevanceSphere.m_v3Position = v3Position;
            automaticLOD.RestoreOriginalMesh(false, true);
            m_nCurrentLODSelected = -1;
            automaticLOD.SetLODDataDirty(true);
            EditorUtility.SetDirty(target);
          }
        }
        else if (Tools.current == Tool.Rotate)
        {
          EditorGUI.BeginChangeCheck();
          Quaternion qRotation = Handles.RotationHandle(Quaternion.Euler(relevanceSphere.m_v3Rotation), relevanceSphere.m_v3Position);
          if (EditorGUI.EndChangeCheck())
          {
            Undo.RecordObject(automaticLOD, "Rotate Relevance Sphere");
            relevanceSphere.m_v3Rotation = qRotation.eulerAngles;
            automaticLOD.RestoreOriginalMesh(false, true);
            m_nCurrentLODSelected = -1;
            automaticLOD.SetLODDataDirty(true);
            EditorUtility.SetDirty(target);
          }
        }
        else if (Tools.current == Tool.Scale)
        {
          EditorGUI.BeginChangeCheck();
          Vector3 v3Scale = Handles.ScaleHandle(relevanceSphere.m_v3Scale, relevanceSphere.m_v3Position, Quaternion.Euler(relevanceSphere.m_v3Rotation), HandleUtility.GetHandleSize(relevanceSphere.m_v3Position) * 1.0f);
          if (EditorGUI.EndChangeCheck())
          {
            Undo.RecordObject(automaticLOD, "Scale Relevance Sphere");
            relevanceSphere.m_v3Scale = v3Scale;
            automaticLOD.RestoreOriginalMesh(false, true);
            m_nCurrentLODSelected = -1;
            automaticLOD.SetLODDataDirty(true);
            EditorUtility.SetDirty(target);
          }
        }

        Matrix4x4 mtxHandles = Handles.matrix;
        Handles.matrix = Matrix4x4.TRS(relevanceSphere.m_v3Position, Quaternion.Euler(relevanceSphere.m_v3Rotation), relevanceSphere.m_v3Scale);
        Handles.color  = new Color(0.0f, 0.0f, 1.0f, 0.5f);
        Handles.SphereHandleCap(0, Vector3.zero, Quaternion.identity, 1.0f, Event.current.type);
        Handles.matrix = mtxHandles;
      }
    }

    Handles.color = Color.white;

    if (automaticLOD.LODSwitchMode != AutomaticLOD.SwitchMode.UnityLODGroup)
    {
      int nLODLevel = automaticLOD.GetCurrentLODLevel();

      if (nLODLevel >= 0)
      {
        Handles.Label(automaticLOD.transform.position, "LOD " + nLODLevel);
      }
    }
  }

  public override void OnInspectorGUI()
  {
    AutomaticLOD automaticLOD;

    string strIncludeChildrenLabel = "Recurse Into Children";

    int nAutomaticLODObjectsWithLODData    = 0;
    int nAutomaticLODObjectsWithoutLODData = 0;
    int nLODLevels                         = -1;
    int nButtonWidth                       = 200;
    int nButtonWidthSmall                  = 150;

    foreach (Object targetObject in targets)
    {
      automaticLOD = targetObject as AutomaticLOD;

      if (automaticLOD.m_LODObjectRoot != null && targets.Length > 1)
      {
        EditorGUILayout.HelpBox("One or more GameObjects of the selection is not a root Automatic LOD GameObject. Only root Automatic LOD GameObjects can be edited at the same time.", MessageType.Warning);
        return;
      }

      if (automaticLOD.HasLODData() || automaticLOD.HasDependentChildren())
      {
        nAutomaticLODObjectsWithLODData++;

        if (nLODLevels == -1)
        {
          nLODLevels = automaticLOD.m_listLODLevels.Count;
        }
        else
        {
          if (automaticLOD.m_listLODLevels.Count != nLODLevels)
          {
            EditorGUILayout.HelpBox("All selected objects need to have the same number of LOD levels", MessageType.Warning);
            return;
          }
        }
      }
      else
      {
        nAutomaticLODObjectsWithoutLODData++;
      }
    }

    if (nAutomaticLODObjectsWithoutLODData > 0 && nAutomaticLODObjectsWithLODData > 0)
    {
      EditorGUILayout.HelpBox("One or more GameObjects of the selection has no LOD levels generated", MessageType.Warning);
      return;
    }

    if (targets.Length > 1)
    {
      EditorGUILayout.HelpBox("Multiple selection", MessageType.Info);
    }

    serializedObject.Update();

    EditorGUILayout.Space();

    EditorGUILayout.BeginHorizontal();
    GUILayout.FlexibleSpace();
    EditorGUILayout.EndHorizontal();
    Rect rectSpace = GUILayoutUtility.GetLastRect();

    if (nAutomaticLODObjectsWithoutLODData > 0)
    {
      EditorGUILayout.PropertyField(PropertyGenerateIncludeChildren, new GUIContent(strIncludeChildrenLabel, "If checked, we will traverse the whole GameObject's hierarchy looking for meshes"));

      GUILayout.BeginHorizontal();

      EditorGUILayout.PropertyField(PropertySwitchMode, new GUIContent("Switch Mode", "Use the new LODGroup for best performance and robustness. The other two are legacy methods: Mesh will create internal meshes and switch between them. GameObject will create LOD level hierarchy GameObjects.")); 

      //GUILayout.FlexibleSpace();
      GUILayout.EndHorizontal();

      if (PropertySwitchMode.enumNames[PropertySwitchMode.enumValueIndex] != AutomaticLOD.SwitchMode.UnityLODGroup.ToString())
      {
        GUILayout.BeginHorizontal();

        EditorGUILayout.PropertyField(PropertyLevelsToGenerate, new GUIContent("Levels To Generate", "Tells how many levels to create by default, but can be changed later on"));

        //GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
      }
      else
      {
        // LODGroup uses 4 levels by default (3 + fully culled) and there is no way to change that programatically without falling into Unity's bug where LODGroup.SetLODS() fails

        for(int i = 0; i < PropertyLevelsToGenerate.enumNames.Length; ++i)
        {
          if(PropertyLevelsToGenerate.enumNames[i] == AutomaticLOD.LevelsToGenerate._3.ToString())
          {
            PropertyLevelsToGenerate.enumValueIndex = i;
            break;
          }
        }
      }

      GUILayout.Space(10);

      GUILayout.BeginHorizontal();
      GUILayout.FlexibleSpace();

      if (GUILayout.Button(new GUIContent("Generate LODs", "Starts the LOD generation process"), GUILayout.Width(nButtonWidth)))
      {
        m_bGenerateLODData = true;
      }

      GUILayout.FlexibleSpace();
      GUILayout.EndHorizontal();

      EditorGUILayout.HelpBox("Number of LOD levels and their properties can be changed later on this panel, after pressing the \"Generate LODs\" button.\n\nSwitch Mode should have no significant speed impact, it's more a convenience option:\nSwitch Mesh will generate internal meshes and switch between them on the same GameObject. Switch GameObject will generate child GameObjects containing LOD levels and enable/disable the correct one.", MessageType.Info);
    }

    if (nLODLevels > 0)
    {
      automaticLOD = target as AutomaticLOD;

      if (automaticLOD.m_LODObjectRoot == null)
      {
        if (automaticLOD.LODSwitchMode != AutomaticLOD.SwitchMode.UnityLODGroup)
        {
          EditorGUILayout.PropertyField(PropertyEvalMode, new GUIContent("LOD Switch Criteria", "Changes the criteria used to switch from one LOD to another at runtime"));

          if (PropertyEvalMode.enumNames[PropertyEvalMode.enumValueIndex] == AutomaticLOD.EvalMode.CameraDistance.ToString())
          {
            PropertyMaxCameraDistanceDistance.floatValue = EditorGUILayout.FloatField(new GUIContent("Max Camera Distance", "Furthest camera distance to consider"), PropertyMaxCameraDistanceDistance.floatValue);
          }
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(PropertyEnablePrefabUsage, new GUIContent("Enable Prefab Usage", "Will save the generated mesh assets to disk, so that this GameObject can be used as a prefab and be instantiated at runtime. Otherwise the mesh won't be available"));

        if (EditorGUI.EndChangeCheck())
        {
          if (PropertyEnablePrefabUsage.boolValue)
          {
            m_bEnablePrefabUsage = true;
          }
          else
          {
            m_bDisablePrefabUsage = true;
          }
        }

        EditorGUILayout.Space();

        PropertyExpandRelevanceSpheres.boolValue = EditorGUILayout.Foldout(PropertyExpandRelevanceSpheres.boolValue, new GUIContent("Vertex Relevance Modifiers:"));

        if (PropertyExpandRelevanceSpheres.boolValue)
        {
          EditorGUILayout.HelpBox("Use vertex relevance spheres to select which vertices should be preserved with more or less priority when simplifying the mesh.", MessageType.Info);

          EditorGUILayout.Space();

          GUILayout.BeginHorizontal();
          GUILayout.FlexibleSpace();

          if (GUILayout.Button(new GUIContent("Add New Sphere", "Adds a new vertex relevance sphere"), GUILayout.Width(nButtonWidthSmall)))
          {
            PropertyRelevanceSpheres.InsertArrayElementAtIndex(0);
            m_bSetupNewRelevanceSphere = true;
            PropertyLODDataDirty.boolValue = true;
          }

          GUILayout.FlexibleSpace();
          GUILayout.EndHorizontal();

          EditorGUILayout.Space();

          EditorGUI.indentLevel++;

          int nSphereToDelete = -1;

          for (int i = 0; i < PropertyRelevanceSpheres.arraySize; i++)
          {
            SerializedProperty elementProperty  = PropertyRelevanceSpheres.GetArrayElementAtIndex(i);
            SerializedProperty elementExpanded  = elementProperty.FindPropertyRelative("m_bExpanded");
            SerializedProperty elementPosition  = elementProperty.FindPropertyRelative("m_v3Position");
            SerializedProperty elementRotation  = elementProperty.FindPropertyRelative("m_v3Rotation");
            SerializedProperty elementScale     = elementProperty.FindPropertyRelative("m_v3Scale");
            SerializedProperty elementRelevance = elementProperty.FindPropertyRelative("m_fRelevance");

            elementExpanded.boolValue = EditorGUILayout.Foldout(elementExpanded.boolValue, new GUIContent("Sphere"));

            if (elementExpanded.boolValue)
            {
              bool bWideMode = EditorGUIUtility.wideMode;

              EditorGUIUtility.wideMode = true;

              EditorGUI.BeginChangeCheck();
              EditorGUILayout.PropertyField(elementPosition, new GUIContent("Position"));
              if (EditorGUI.EndChangeCheck())
              {
                DeselectLODLevel();
                PropertyLODDataDirty.boolValue = true;
              }

              EditorGUI.BeginChangeCheck();
              EditorGUILayout.PropertyField(elementRotation, new GUIContent("Rotation"));
              if (EditorGUI.EndChangeCheck())
              {
                DeselectLODLevel();
                PropertyLODDataDirty.boolValue = true;
              }

              EditorGUI.BeginChangeCheck();
              EditorGUILayout.PropertyField(elementScale, new GUIContent("Scale"));
              if (EditorGUI.EndChangeCheck())
              {
                DeselectLODLevel();
                PropertyLODDataDirty.boolValue = true;
              }

              EditorGUI.BeginChangeCheck();
              elementRelevance.floatValue = EditorGUILayout.Slider(new GUIContent("Relevance", "Tells the simplification algorithm how relevant the vertices inside this sphere are. Default relevance is 0, use lower values to discard non important vertices, and higher values to keep them before others when simplifying the mesh"), elementRelevance.floatValue, -1.0f, 1.0f);
              if (EditorGUI.EndChangeCheck())
              {
                PropertyLODDataDirty.boolValue = true;
              }

              GUILayout.BeginHorizontal();
              GUILayout.FlexibleSpace();

              if (GUILayout.Button(new GUIContent("Remove Sphere", "Removes this relevance sphere"), GUILayout.Width(nButtonWidthSmall)))
              {
                DeselectLODLevel();
                nSphereToDelete = i;
                PropertyLODDataDirty.boolValue = true;
              }

              GUILayout.FlexibleSpace();
              GUILayout.EndHorizontal();

              EditorGUIUtility.wideMode = bWideMode;
            }
          }

          if (nSphereToDelete >= 0)
          {
            PropertyRelevanceSpheres.DeleteArrayElementAtIndex(nSphereToDelete);
          }

          EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        string helpBoxString = "Use the bar below to manage LOD levels:\nClick to select a LOD level and adjust parameters.";

        if(automaticLOD.LODSwitchMode == AutomaticLOD.SwitchMode.UnityLODGroup)
        {
          helpBoxString += " Use the LODGroup component to adjust the camera ranges where each LOD level is visible";
        }
        else
        {
          helpBoxString += " Right click to add / delete new LOD levels.Drag the splitters in between to adjust when each LOD level should be visible and slide the top cursor to preview the current settings in the Scene window.";
        }

        EditorGUILayout.HelpBox(helpBoxString, MessageType.Info);

        EditorGUILayout.Space();

        int y = (int)GUILayoutUtility.GetLastRect().yMax;
        GUILayout.Space(65);
        DrawAndHandleLODBar(automaticLOD, y, (int)rectSpace.width, nButtonWidthSmall);

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button(new GUIContent("Recompute all levels", "Recomputes all different LOD meshes at once"), GUILayout.Width(nButtonWidth)))
        {
          m_bRecomputeAllMeshes = true;
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button(new GUIContent("Delete LOD data...", "Deletes all LOD data and restores the original GameObject's mesh"), GUILayout.Width(nButtonWidth)))
        {
          if (EditorUtility.DisplayDialog("Delete all LOD data and restore original mesh?", "Are you sure you want to delete all LOD data and restore the original mesh?", "Delete", "Cancel"))
          {
            m_bDeleteLODData = true;
          }
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
      }
      else
      {
        GUILayout.Label("Child LOD GameObject depending on " + automaticLOD.m_LODObjectRoot.name);

        EditorGUILayout.PropertyField(PropertyOverrideRootSettings, new GUIContent("Override " + automaticLOD.m_LODObjectRoot.name + " settings", "Will allow to edit this object's own parameters, instead of inheriting those of the root Automatic LOD GameObject"));

        if (PropertyOverrideRootSettings.boolValue)
        {
          EditorGUILayout.Space();

          for (int nLevel = 0; nLevel < automaticLOD.m_listLODLevels.Count; nLevel++)
          {
            SerializedProperty elementProperty     = PropertyListLODLevels.GetArrayElementAtIndex(nLevel);
            SerializedProperty elementVertexAmount = elementProperty.FindPropertyRelative("m_fMeshVerticesAmount");

            EditorGUILayout.BeginHorizontal();

            string strControlName = "SliderLOD" + nLevel; 

            GUI.SetNextControlName(strControlName);

            float fVertexAmount = EditorGUILayout.Slider(new GUIContent("LOD " + nLevel + " vertex %", "The percentage of vertices from the original mesh to keep when simplifying it"), elementVertexAmount.floatValue * 100.0f, 0.0f, 100.0f);
            elementVertexAmount.floatValue = Mathf.Clamp01(fVertexAmount / 100.0f);

            if (GUI.GetNameOfFocusedControl() == strControlName && !EditorApplication.isPlaying)
            {
              automaticLOD.SwitchToLOD(nLevel, false);
            }

            if (GUILayout.Button(new GUIContent("Regenerate", "Recomputes this level's mesh"), GUILayout.Width(80)))
            {
              Simplifier.Cancelled = false;

              automaticLOD = target as AutomaticLOD;
              ChangeLODVertexAmount(automaticLOD, nLevel, automaticLOD.m_listLODLevels[nLevel].m_fMeshVerticesAmount, false);

              if (Simplifier.Cancelled == false)
              {
                automaticLOD.SwitchToLOD(nLevel, false);
              }

              EditorUtility.ClearProgressBar();

              if (Simplifier.Cancelled == false)
              {
                SaveMeshAssets();
              }

              Simplifier.Cancelled = false;
            }

            EditorGUILayout.EndHorizontal();
          }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField(new GUIContent("Mesh optimization stats:"));

        EditorGUI.indentLevel += 2;

        for (int nLevel = 0; nLevel < automaticLOD.m_listLODLevels.Count; nLevel++)
        {
          string strStats = "No mesh data";

          if(automaticLOD.m_listLODLevels[nLevel].m_mesh != null && automaticLOD.m_originalMesh != null)
          {
            float fPercent = automaticLOD.m_listLODLevels[nLevel].m_fMeshVerticesAmount * 100.0f;

            int levelTris = automaticLOD.m_listLODLevels[nLevel].m_bUsesOriginalMesh ? automaticLOD.m_originalMesh.triangles.Length / 3 : automaticLOD.m_listLODLevels[nLevel].m_mesh.triangles.Length / 3;
            int totalTris = automaticLOD.m_originalMesh.triangles.Length / 3;
            float fPercentTris = ((float)levelTris / (float)totalTris) * 100.0f;

            if(PropertyOverrideRootSettings.boolValue == false && automaticLOD.m_LODObjectRoot.GetLODLevelCount() > nLevel)
            {
              fPercent = automaticLOD.m_LODObjectRoot.m_listLODLevels[nLevel].m_fMeshVerticesAmount * 100.0f;
            }

            strStats = string.Format("{0}/{1} Vertices ({2:0.00}%), {3}/{4} Tris ({5:0.00}%)", automaticLOD.m_listLODLevels[nLevel].m_bUsesOriginalMesh ? automaticLOD.m_originalMesh.vertexCount : automaticLOD.m_listLODLevels[nLevel].m_mesh.vertexCount, automaticLOD.m_originalMesh.vertexCount, fPercent,
                                                                                               levelTris, totalTris, fPercentTris);
          }

          EditorGUILayout.LabelField(new GUIContent("LOD " + nLevel + ": " + strStats));
        }

        EditorGUI.indentLevel -= 2;

        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button(new GUIContent("Exclude from LOD tree...", "Restores this GameObject's original mesh and excludes it from the LOD computation"), GUILayout.Width(nButtonWidth)))
        {
          if (EditorUtility.DisplayDialog("Remove from LOD tree?", "Are you sure you want to restore this gameobject's mesh and exclude it from the LOD logic?", "Remove", "Cancel"))
          {
            m_bRemoveFromLODTree = true;
          }
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
      }
    }

    serializedObject.ApplyModifiedProperties();

    bool bRepaint = false;

    if(m_bEnablePrefabUsage)
    {
      Simplifier.Cancelled = false;
      m_bEnablePrefabUsage = false;
      SaveMeshAssets();
      Simplifier.Cancelled = false;
    }

    if(m_bDisablePrefabUsage)
    {
      m_bDisablePrefabUsage = false;

      if (PropertyEnablePrefabUsage.boolValue == false)
      {
        foreach (Object targetObject in targets)
        {
          automaticLOD = targetObject as AutomaticLOD;
          automaticLOD.DisablePrefabUsage(true);
        }
      }
    }

    if (m_bGenerateLODData && Event.current.type == EventType.Repaint)
    {
      m_bGenerateLODData = false;
      Simplifier.Cancelled = false;

      foreach (Object targetObject in targets)
      {
        automaticLOD = targetObject as AutomaticLOD;

        if (PropertyGenerateIncludeChildren.boolValue == false)
        {
          if (AutomaticLOD.HasValidMeshData(automaticLOD.gameObject) == false)
          {
            EditorUtility.DisplayDialog("Error", "Object " + automaticLOD.name + " has no MeshFilter nor Skinned Mesh to process. Please use the \"" + strIncludeChildrenLabel + "\" parameter if you want to process the whole " + automaticLOD.name + " hierarchy for meshes", "OK");
            continue;
          }
        }

        try
        {
          CreateDefaultLODS(PropertyLevelsToGenerate.enumValueIndex + 1, automaticLOD, PropertyGenerateIncludeChildren.boolValue);

          automaticLOD.ComputeLODData(true, Progress);

          if (Simplifier.Cancelled == false)
          {
            automaticLOD.ComputeAllLODMeshes(true, Progress);
          }
          else
          {
            DeleteLODDataRecursive(automaticLOD.gameObject, automaticLOD.gameObject, true);
            break;
          }

          m_nCurrentLODSelected = -1;
        }
        catch(System.Exception e)
        {
          Debug.LogError("Error generating LODs: " + e.Message + " Stack: " + e.StackTrace);
          EditorUtility.ClearProgressBar();
          Simplifier.Cancelled = false;
        }
      }

      bRepaint = true;
      EditorUtility.ClearProgressBar();
      Simplifier.Cancelled = false;
    }

    if (m_bRecomputeAllMeshes && Event.current.type == EventType.Repaint)
    {
      m_bRecomputeAllMeshes = false;
      Simplifier.Cancelled = false;

      foreach (Object targetObject in targets)
      {
        automaticLOD = targetObject as AutomaticLOD;

        try
        {
          int nRestoreLODLevel = -1;

          if (automaticLOD.HasLODDataDirty())
          {
            nRestoreLODLevel = m_nCurrentLODSelected;
            automaticLOD.RestoreOriginalMesh(false, true);
            automaticLOD.ComputeLODData(true, Progress);
          }

          if (Simplifier.Cancelled == false)
          {
            automaticLOD.ComputeAllLODMeshes(true, Progress);

            if (nRestoreLODLevel >= 0 && Simplifier.Cancelled == false)
            {
              automaticLOD.SwitchToLOD(nRestoreLODLevel, true);
            }

            if (Simplifier.Cancelled)
            {
              break;
            }
          }
          else
          {
            DeleteLODDataRecursive(automaticLOD.gameObject, automaticLOD.gameObject, true);
            break;
          }
        }
        catch (System.Exception e)
        {
          Debug.LogError("Error recomputing all meshes: " + e.Message + " Stack: " + e.StackTrace);
          EditorUtility.ClearProgressBar();
          Simplifier.Cancelled = false;
        }
      }

      bRepaint = true;
      EditorUtility.ClearProgressBar();

      if (Simplifier.Cancelled == false)
      {
        SaveMeshAssets();
      }

      Simplifier.Cancelled = false;
    }

    if (m_bDeleteLODData && Event.current.type == EventType.Repaint)
    {
      m_bDeleteLODData = false;
      DeleteLODData();
      bRepaint = true;
    }

    if (m_bRemoveFromLODTree && Event.current.type == EventType.Repaint)
    {
      m_bRemoveFromLODTree = false;
      automaticLOD = target as AutomaticLOD;
      automaticLOD.RemoveFromLODTree();

      if (Application.isEditor && Application.isPlaying == false)
      {
        UnityEngine.Object.DestroyImmediate(automaticLOD);
      }
      else
      {
        UnityEngine.Object.Destroy(automaticLOD);
      }

      bRepaint = true;
    }

    if (m_bSetupNewRelevanceSphere)
    {
      m_bSetupNewRelevanceSphere = false;

      foreach (Object targetObject in targets)
      {
        automaticLOD = targetObject as AutomaticLOD;

        if (automaticLOD.m_aRelevanceSpheres != null && automaticLOD.m_aRelevanceSpheres.Length > 0)
        {
          automaticLOD.m_aRelevanceSpheres[0].SetDefault(automaticLOD.transform, 0.2f);
        }
      }
    }

    if (bRepaint)
    {
      Repaint();
    }
  }

  Rect DrawAndHandleLODBar(AutomaticLOD automaticLOD, int y, int nTotalWidth, int nButtonWidth)
  {
    int nMargin = 10;
    int nWidth  = nTotalWidth - (nMargin * 2);
    int nHeight = 30;

    // Compute LOD bar positions (each entry tells where the LOD ends, 0-1);

    int nLODLevels = automaticLOD.m_listLODLevels.Count;

    float[] afLODPositions = new float[nLODLevels];

    bool bRectLODSelected = false;
    Rect rectLODSelected = new Rect();
    Rect rectLODSelectedBorder = new Rect();

    for (int nLOD = 0; nLOD < nLODLevels; nLOD++)
    {
      if (automaticLOD.m_evalMode == AutomaticLOD.EvalMode.ScreenCoverage)
      {
        afLODPositions[nLOD] = nLOD == nLODLevels - 1 ? 1.0f : ScreenCoverageToSlider(automaticLOD.m_listLODLevels[nLOD + 1].m_fScreenCoverage);
      }
      else if (automaticLOD.m_evalMode == AutomaticLOD.EvalMode.CameraDistance)
      {
        afLODPositions[nLOD] = nLOD == nLODLevels - 1 ? 1.0f : (automaticLOD.m_listLODLevels[nLOD + 1].m_fMaxCameraDistance / automaticLOD.m_fMaxCameraDistance);
      }
    }

    int nPreviewBarHeight = 15;
    int nPreviewVerticalMargin = 5;
    int nYPreview = y;

    if (automaticLOD.LODSwitchMode != AutomaticLOD.SwitchMode.UnityLODGroup)
    {
      // Preview bar

      Rect rectPreview = new Rect(nMargin, y, nWidth, nPreviewBarHeight + nPreviewVerticalMargin);

      if (m_nCurrentLODResizing == -1)
      {
        EditorGUIUtility.AddCursorRect(rectPreview, MouseCursor.SlideArrow);
      }

      int nPreviewControlID = GUIUtility.GetControlID(FocusType.Passive, rectPreview);

      if (Event.current.type == EventType.MouseDown && rectPreview.Contains(Event.current.mousePosition) && Event.current.button == 0)
      {
        m_bPreviewingLOD = true;
        m_fPreviewLODPos = Mathf.Clamp01((float)(Event.current.mousePosition.x - nMargin) / (float)rectPreview.width);
        PreviewLOD(m_fPreviewLODPos, afLODPositions);
        SaveSceneViewCameraInfo();
        GUIUtility.hotControl = nPreviewControlID; //Lock on your custom control to prevent interaction with other controls
        Event.current.Use();
      }
      else if (Event.current.type == EventType.MouseDrag && m_bPreviewingLOD == true)
      {
        m_fPreviewLODPos = Mathf.Clamp01((float)(Event.current.mousePosition.x - nMargin) / (float)rectPreview.width);
        PreviewLOD(m_fPreviewLODPos, afLODPositions);
        GUI.FocusControl("");
        Event.current.Use();
      }
      else if (Event.current.rawType == EventType.MouseUp && m_bPreviewingLOD == true && Event.current.button == 0 && GUIUtility.hotControl == nPreviewControlID)
      {
        m_bPreviewingLOD = false;

        foreach (Object targetObject in targets)
        {
          AutomaticLOD automaticLODSelection = targetObject as AutomaticLOD;

          if (!EditorApplication.isPlaying)
          {
            automaticLODSelection.SwitchToLOD(m_nCurrentLODSelected == -1 ? 0 : m_nCurrentLODSelected, true);
          }
        }

        LoadSceneViewCameraInfos();
        GUIUtility.hotControl = 0; // Release lock
        Event.current.Use();

        HandleUtility.Repaint();
      }
    }

    // LOD bar

    y += nPreviewBarHeight + nPreviewVerticalMargin;
    Rect rectBar = new Rect(nMargin - 1, y - 1, nWidth + 2, nHeight + 2);
    GUI.DrawTexture(rectBar, s_texBlack);

    for (int nLOD = 0; nLOD < nLODLevels; nLOD++)
    {
      int nLODWidth = nLOD == 0 ? Mathf.RoundToInt(afLODPositions[nLOD] * nWidth) : (Mathf.RoundToInt(afLODPositions[nLOD] * nWidth) - Mathf.RoundToInt(afLODPositions[nLOD - 1] * nWidth));
      int nLODStart = nLOD == 0 ? nMargin : nMargin + Mathf.RoundToInt((afLODPositions[nLOD - 1] * nWidth));

      SerializedProperty elementProperty           = PropertyListLODLevels.GetArrayElementAtIndex(nLOD);
      SerializedProperty elementMeshVerticesAmount = elementProperty.FindPropertyRelative("m_fMeshVerticesAmount");

      Rect rect = new Rect(nLODStart, y, nLODWidth, nHeight);

      int nResizeMargin = 3;
      Rect resizeRect = new Rect(rect.x + rect.width - nResizeMargin, rect.y, nResizeMargin * 2, rect.height);

      if (automaticLOD.LODSwitchMode != AutomaticLOD.SwitchMode.UnityLODGroup)
      {
        if (nLOD != afLODPositions.Length - 1 && m_bPreviewingLOD == false)
        {
          EditorGUIUtility.AddCursorRect(resizeRect, MouseCursor.ResizeHorizontal);
        }
      }

      int nResizeControlID = GUIUtility.GetControlID(FocusType.Passive, resizeRect);

      if (Event.current.type == EventType.MouseDown)
      {
        if (Event.current.button == 0)
        {
          if (resizeRect.Contains(Event.current.mousePosition) && nLOD != afLODPositions.Length - 1)
          {
            m_nCurrentLODResizing = nLOD;
            GUIUtility.hotControl = nResizeControlID; //Lock on your custom control to prevent interaction with other controls
            Event.current.Use();
          }
          else if (rect.Contains(Event.current.mousePosition))
          {
            m_nCurrentLODSelected = nLOD;

            if (!EditorApplication.isPlaying)
            {
              foreach (Object targetObject in targets)
              {
                AutomaticLOD automaticLODSelection = targetObject as AutomaticLOD;
                automaticLODSelection.SwitchToLOD(nLOD, true);
              }
            }

            Event.current.Use();

            if (automaticLOD.LODSwitchMode == AutomaticLOD.SwitchMode.UnityLODGroup)
            {
              // We force a specific LOD here using LODGroup. To exit this state we will monitor the scene cameras and if there is a change then restore control to LODGroup
              SaveSceneViewCameraInfo();
            }
          }
        }
      }
      else if (Event.current.type == EventType.MouseDrag && m_nCurrentLODResizing == nLOD && nLOD != afLODPositions.Length - 1 && automaticLOD.LODSwitchMode != AutomaticLOD.SwitchMode.UnityLODGroup)
      {
        if (automaticLOD.m_evalMode == AutomaticLOD.EvalMode.ScreenCoverage)
        {
          float fPosition = (float)(Event.current.mousePosition.x - nMargin) / (float)nWidth;
          float fScreenCoverage = SliderToScreenCoverage(fPosition);

          foreach (Object targetObject in targets)
          {
            AutomaticLOD automaticLODSelection = targetObject as AutomaticLOD;

            Undo.RecordObject(automaticLODSelection, "Change Screen Coverage");
            EditorUtility.SetDirty(automaticLODSelection);

            automaticLODSelection.m_listLODLevels[nLOD + 1].m_fScreenCoverage = fScreenCoverage;

            // Clip left

            if (automaticLODSelection.m_listLODLevels[nLOD + 1].m_fScreenCoverage > automaticLODSelection.m_listLODLevels[nLOD].m_fScreenCoverage)
            {
              automaticLODSelection.m_listLODLevels[nLOD + 1].m_fScreenCoverage = automaticLODSelection.m_listLODLevels[nLOD].m_fScreenCoverage;
            }

            // Clip right

            if (nLOD < nLODLevels - 2)
            {
              if (automaticLODSelection.m_listLODLevels[nLOD + 1].m_fScreenCoverage < automaticLODSelection.m_listLODLevels[nLOD + 2].m_fScreenCoverage)
              {
                automaticLODSelection.m_listLODLevels[nLOD + 1].m_fScreenCoverage = automaticLODSelection.m_listLODLevels[nLOD + 2].m_fScreenCoverage;
              }
            }
          }
        }
        else if (automaticLOD.m_evalMode == AutomaticLOD.EvalMode.CameraDistance)
        {
          foreach (Object targetObject in targets)
          {
            AutomaticLOD automaticLODSelection = targetObject as AutomaticLOD;

            Undo.RecordObject(automaticLODSelection, "Change Camera Distance");
            EditorUtility.SetDirty(automaticLODSelection);

            automaticLODSelection.m_listLODLevels[nLOD + 1].m_fMaxCameraDistance = Mathf.Clamp01(((float)(Event.current.mousePosition.x - nMargin) / (float)nWidth)) * automaticLODSelection.m_fMaxCameraDistance;

            // Clip left

            if (automaticLODSelection.m_listLODLevels[nLOD + 1].m_fMaxCameraDistance < automaticLODSelection.m_listLODLevels[nLOD].m_fMaxCameraDistance)
            {
              automaticLODSelection.m_listLODLevels[nLOD + 1].m_fMaxCameraDistance = automaticLODSelection.m_listLODLevels[nLOD].m_fMaxCameraDistance;
            }

            // Clip right

            if (nLOD < nLODLevels - 2)
            {
              if (automaticLODSelection.m_listLODLevels[nLOD + 1].m_fMaxCameraDistance > automaticLODSelection.m_listLODLevels[nLOD + 2].m_fMaxCameraDistance)
              {
                automaticLODSelection.m_listLODLevels[nLOD + 1].m_fMaxCameraDistance = automaticLODSelection.m_listLODLevels[nLOD + 2].m_fMaxCameraDistance;
              }
            }
          }
        }

        Event.current.Use();
      }
      else if (Event.current.type == EventType.MouseUp && automaticLOD.LODSwitchMode != AutomaticLOD.SwitchMode.UnityLODGroup)
      {
        if (Event.current.button == 0)
        {
          if (GUIUtility.hotControl == nResizeControlID)
          {
            GUIUtility.hotControl = 0; // Release lock
            m_nCurrentLODResizing = -1;
            Event.current.Use();
          }
        }
        else if (Event.current.button == 1)
        {
          if (rect.Contains(Event.current.mousePosition))
          {
            m_nCurrentLODSelected = nLOD;
            Event.current.Use();

            GenericMenu contextMenu = new GenericMenu ();

            contextMenu.AddItem(new GUIContent(MENUADDBEFORE), false, LODContextMenuCallback, MENUADDBEFORE);
            contextMenu.AddItem(new GUIContent(MENUADDAFTER),  false, LODContextMenuCallback, MENUADDAFTER);

            if (automaticLOD.m_listLODLevels.Count > 1)
            {
              contextMenu.AddSeparator("");
              contextMenu.AddItem(new GUIContent(MENUDELETE), false, LODContextMenuCallback, MENUDELETE);
            }

            contextMenu.ShowAsContext();
          }
        }
      }

      int nBorderMargin = 1;
      Rect rectLODBorder = new Rect(rect.x - nBorderMargin, rect.y - nBorderMargin, rect.width + (nBorderMargin * 2), rect.height + (nBorderMargin * 2));

      if (m_nCurrentLODSelected == nLOD)
      {
        int nSelectionMargin = 2;
        bRectLODSelected = true;
        rectLODSelected = rect;
        rectLODSelectedBorder = new Rect(rect.x - nSelectionMargin, rect.y - nSelectionMargin, rect.width + (nSelectionMargin * 2), rect.height + (nSelectionMargin * 2));

        GUILayout.Label("Selected LOD: " + nLOD, EditorStyles.boldLabel);

        string strLabel = "";

        if (automaticLOD.LODSwitchMode != AutomaticLOD.SwitchMode.UnityLODGroup)
        {
          if (automaticLOD.m_evalMode == AutomaticLOD.EvalMode.ScreenCoverage)
          {
            string strRange = string.Format("{0:0.0}% to {1:0.0}%", automaticLOD.m_listLODLevels[nLOD].m_fScreenCoverage * 100.0f, m_nCurrentLODSelected < (nLODLevels - 1) ? (automaticLOD.m_listLODLevels[nLOD + 1].m_fScreenCoverage * 100.0f) : 0.0f);
            strLabel = "Rendered when the object covers " + strRange + " screen space";
          }
          else if (automaticLOD.m_evalMode == AutomaticLOD.EvalMode.CameraDistance)
          {
            string strRange = string.Format("{0:0.0} to {1:0.0}", automaticLOD.m_listLODLevels[nLOD].m_fMaxCameraDistance, m_nCurrentLODSelected < (nLODLevels - 1) ? (automaticLOD.m_listLODLevels[nLOD + 1].m_fMaxCameraDistance) : Mathf.Infinity);
            strLabel = "Rendered when the camera is at a distance from " + strRange;
          }

          GUILayout.Label(strLabel);
        }

        float fVertexAmount = EditorGUILayout.Slider(new GUIContent("Total vertices %", "The percentage of vertices from the original mesh to keep when simplifying it"), elementMeshVerticesAmount.floatValue * 100.0f, 0.0f, 100.0f);

        elementMeshVerticesAmount.floatValue = fVertexAmount / 100.0f;

        int nLODMeshVertexCount = automaticLOD.GetLODVertexCount(nLOD, true);
        int nMeshVertexCount    = automaticLOD.GetOriginalVertexCount(true);

        int nLODMeshTriangleCount = automaticLOD.GetLODTriangleCount(nLOD, true);
        int nMeshTriangleCount    = automaticLOD.GetOriginalTriangleCount(true);
        float fPercentTriangles   = ((float)nLODMeshTriangleCount / (float)nMeshTriangleCount) * 100.0f;

        EditorGUILayout.LabelField("Vertex count: " + nLODMeshVertexCount + "/" + nMeshVertexCount);
        EditorGUILayout.LabelField("Triangle count: " + nLODMeshTriangleCount + "/" + nMeshTriangleCount + string.Format(" ({0:0.00}%)", fPercentTriangles));

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        string strButton = automaticLOD.HasVertexData(nLOD, true) ? ("Recompute Level " + nLOD) : ("Compute level " + nLOD);

        if (GUILayout.Button(new GUIContent(strButton, "Generates the mesh(es) corresponding to this LOD"), GUILayout.Width(nButtonWidth)))
        {
          Simplifier.Cancelled = false;

          ChangeLODVertexAmount(automaticLOD, nLOD, automaticLOD.m_listLODLevels[nLOD].m_fMeshVerticesAmount, true);

          if (Simplifier.Cancelled == false)
          {
            automaticLOD.SwitchToLOD(nLOD, true);
          }

          EditorUtility.ClearProgressBar();

          if (Simplifier.Cancelled == false)
          {
            SaveMeshAssets();
          }

          Simplifier.Cancelled = false;
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
      }

      if (automaticLOD.LODSwitchMode == AutomaticLOD.SwitchMode.UnityLODGroup)
      {
        GUI.DrawTexture(rectLODBorder, s_texBlack, ScaleMode.StretchToFill);
      }

      DrawLODBar(automaticLOD, rect, GetColorTexture(automaticLOD, automaticLOD.m_listLODLevels[nLOD].m_nColorEditorBarIndex), nLOD);
    }

    if(bRectLODSelected)
    {
      GUI.DrawTexture(rectLODSelectedBorder, s_texBlack, ScaleMode.StretchToFill);
      DrawLODBar(automaticLOD, rectLODSelected, GetColorTexture(automaticLOD, automaticLOD.m_listLODLevels[m_nCurrentLODSelected].m_nColorEditorBarIndex), m_nCurrentLODSelected);

      Rect rectSelectionArrow = new Rect(rectLODSelected.x + (rectLODSelected.width / 2) - (s_texArrow.width / 2), rectLODSelected.y + rectLODSelected.height - s_texArrow.height, s_texArrow.width, s_texArrow.height);
      GUI.DrawTexture(rectSelectionArrow, s_texArrow, ScaleMode.StretchToFill);
    }

    if (automaticLOD.LODSwitchMode != AutomaticLOD.SwitchMode.UnityLODGroup)
    {
      // Preview bar

      if (s_texHandle == null)
      {
        int nCursorSize = 10;

        GUI.DrawTexture(new Rect(nMargin + Mathf.RoundToInt(nWidth * m_fPreviewLODPos) - (nCursorSize / 2), nYPreview, nCursorSize, nCursorSize), s_texBlack);
        GUI.DrawTexture(new Rect(nMargin + Mathf.RoundToInt(nWidth * m_fPreviewLODPos), nYPreview, 1, nPreviewBarHeight + nPreviewVerticalMargin + nHeight), s_texBlack);
      }
      else
      {
        GUI.DrawTexture(new Rect(nMargin + Mathf.RoundToInt(nWidth * m_fPreviewLODPos) - (s_texHandle.width / 2), (nYPreview + nPreviewBarHeight + nPreviewVerticalMargin + nHeight) - s_texHandle.height, s_texHandle.width, s_texHandle.height), s_texHandle);
        //GUI.Label      (new Rect(nMargin + Mathf.RoundToInt(nWidth * m_fPreviewLODPos) + (s_texHandle.width / 2) + 5, nYPreview, 50, 20), new GUIContent("Preview"));
      }

      // Preview camera

      if (m_bPreviewingLOD)
      {
        PreviewSceneViewCameras(automaticLOD, m_fPreviewLODPos);

        for (int i = 0; i < SceneView.sceneViews.Count; i++)
        {
          SceneView sceneView = SceneView.sceneViews[i] as SceneView;

          if (sceneView.orthographic == false)
          {
            sceneView.Repaint();
          }
        }
      }
    }

    return rectBar;
  }

  void CreateDefaultLODS(int nLevels, AutomaticLOD root, bool bRecurseIntoChildren)
  {
    List<AutomaticLOD.LODLevelData> listLODLevels = new List<AutomaticLOD.LODLevelData>();

    for (int i = 0; i < nLevels; i++)
    {
      AutomaticLOD.LODLevelData data = new AutomaticLOD.LODLevelData();

      float oneminust = (float)(nLevels - i) / (float)nLevels;

      data.m_fScreenCoverage      = SliderToScreenCoverage(1.0f - oneminust);
      data.m_fMaxCameraDistance   = i == 0 ? 0.0f : i * 100.0f;
      data.m_fMeshVerticesAmount  = oneminust;
      data.m_mesh                 = null;
      data.m_bUsesOriginalMesh    = false;
      data.m_nColorEditorBarIndex = i;

      listLODLevels.Add(data);
    }

    root.SetLODLevels(listLODLevels, AutomaticLOD.EvalMode.ScreenCoverage, 1000.0f, bRecurseIntoChildren);
  }

  void PreviewLOD(float fValue, float[] afLODPositions)
  {
    if(EditorApplication.isPlaying)
    {
      return;
    }

    foreach (Object targetObject in targets)
    {
      AutomaticLOD automaticLOD = targetObject as AutomaticLOD;

      if (automaticLOD != null && automaticLOD.m_LODObjectRoot == null)
      {
        for (int nLOD = 0; nLOD < automaticLOD.m_listLODLevels.Count; nLOD++)
        {
          if (fValue < afLODPositions[nLOD])
          {
            automaticLOD.SwitchToLOD(nLOD, true);
            break;
          }
        }
      }
    }
  }

  void ChangeLODVertexAmount(AutomaticLOD automaticLOD, int nLOD, float fMeshVertexAmount, bool bRecurseIntoChildren)
  {
    if (automaticLOD)
    {
      if (nLOD >= 0 && nLOD < automaticLOD.m_listLODLevels.Count)
      {
        automaticLOD.m_listLODLevels[nLOD].m_fMeshVerticesAmount = fMeshVertexAmount;

        if(automaticLOD.HasLODDataDirty())
        {
          automaticLOD.RestoreOriginalMesh(false, true);
          automaticLOD.ComputeLODData(true, Progress);

          if(Simplifier.Cancelled)
          {
            return;
          }
        }

        automaticLOD.ComputeLODMesh(nLOD, bRecurseIntoChildren, Progress);

        if (Simplifier.Cancelled)
        {
          return;
        }

        automaticLOD.SwitchToLOD(nLOD, bRecurseIntoChildren);
      }
    }
  }

  void DeselectLODLevel()
  {
    RestoreOriginalMeshes();
    m_nCurrentLODSelected = -1;
  }
  
  Texture2D GetColorTexture(AutomaticLOD automaticLOD, int nIndex)
  {
    if(automaticLOD.LODSwitchMode == AutomaticLOD.SwitchMode.UnityLODGroup)
    {
      if(m_nCurrentLODSelected == nIndex)
      {
        return s_texLODGroupSelected;
      }
      else
      {
        return s_texLODGroupNotSelected;
      }
    }

    return s_aColorTextures[nIndex % s_aColorTextures.Length];
  }

  void LODContextMenuCallback(object parameter)
  {
    string strMenuItem = parameter.ToString();
    Simplifier.Cancelled = false;

    foreach (Object targetObject in targets)
    {
      AutomaticLOD automaticLOD = targetObject as AutomaticLOD;

      if (automaticLOD != null)
      {
        if (strMenuItem == MENUADDBEFORE)
        {
          Undo.RecordObject(automaticLOD, "Add LOD level before");

          if (automaticLOD.m_listDependentChildren != null)
          {
            foreach (AutomaticLOD childLOD in automaticLOD.m_listDependentChildren)
            {
              Undo.RecordObject(childLOD, "Add LOD level before");
            }
          }

          automaticLOD.AddLODLevel(m_nCurrentLODSelected, true, true, true);
          m_nCurrentLODSelected++;
        }
        else if(strMenuItem == MENUADDAFTER)
        {
          Undo.RecordObject(automaticLOD, "Add LOD level after");

          if (automaticLOD.m_listDependentChildren != null)
          {
            foreach (AutomaticLOD childLOD in automaticLOD.m_listDependentChildren)
            {
              Undo.RecordObject(childLOD, "Add LOD level after");
            }
          }

          automaticLOD.AddLODLevel(m_nCurrentLODSelected, false, true, true);
        }
        else if(strMenuItem == MENUDELETE)
        {
          Undo.RecordObject(automaticLOD, "Delete LOD level");

          if (automaticLOD.m_listLODLevels != null && m_nCurrentLODSelected < automaticLOD.m_listLODLevels.Count)
          {
            if (automaticLOD.m_listLODLevels[m_nCurrentLODSelected].m_mesh != null)
            {
              Undo.RecordObject(automaticLOD.m_listLODLevels[m_nCurrentLODSelected].m_mesh, "Delete LOD level");
            }
          }

          if (automaticLOD.m_listDependentChildren != null)
          {
            foreach (AutomaticLOD childLOD in automaticLOD.m_listDependentChildren)
            {
              Undo.RecordObject(childLOD, "Delete LOD level");

              if (childLOD.m_listLODLevels != null && m_nCurrentLODSelected < childLOD.m_listLODLevels.Count)
              {
                if (childLOD.m_listLODLevels[m_nCurrentLODSelected].m_mesh != null)
                {
                  Undo.RecordObject(childLOD.m_listLODLevels[m_nCurrentLODSelected].m_mesh, "Delete LOD level");
                }
              }
            }
          }

          automaticLOD.RemoveLODLevel(m_nCurrentLODSelected, true, true);

          if (m_nCurrentLODSelected > 0)
          {
            m_nCurrentLODSelected--;
          }
        }

        EditorUtility.ClearProgressBar();
        EditorUtility.SetDirty(automaticLOD);
      }

      automaticLOD.SwitchToLOD(m_nCurrentLODSelected, true);

      if (Simplifier.Cancelled == false)
      {
        SaveMeshAssets();
      }
    }

    Simplifier.Cancelled = false;
  }

  void DrawLODBar(AutomaticLOD automaticLOD, Rect rect, Texture2D tex2D, int nLevel)
  {
    GUI.DrawTexture(rect, GetColorTexture(automaticLOD, automaticLOD.m_listLODLevels[nLevel].m_nColorEditorBarIndex), ScaleMode.StretchToFill);

    string strValue = "";

    if (automaticLOD.m_evalMode == AutomaticLOD.EvalMode.ScreenCoverage)
    {
      float fCoverageStart = automaticLOD.m_listLODLevels[nLevel].m_fScreenCoverage * 100.0f;
      float fCoverageEnd   = nLevel == automaticLOD.m_listLODLevels.Count - 1 ? 0.0f : (automaticLOD.m_listLODLevels[nLevel + 1].m_fScreenCoverage * 100.0f);
      strValue = Mathf.RoundToInt(fCoverageStart).ToString() + "%" + "-" + Mathf.RoundToInt(fCoverageEnd).ToString() + "% screen";
    }
    else if(automaticLOD.m_evalMode == AutomaticLOD.EvalMode.CameraDistance)
    {
      string strDistanceStart = automaticLOD.m_listLODLevels[nLevel].m_fMaxCameraDistance.ToString("F1");
      string strDistanceEnd   = nLevel == automaticLOD.m_listLODLevels.Count - 1 ? "Infinity" : automaticLOD.m_listLODLevels[nLevel + 1].m_fMaxCameraDistance.ToString("F1");
      strValue = strDistanceStart + "-" + strDistanceEnd + " distance";
    }

    GUI.Label(rect, "LOD " + nLevel, EditorStyles.whiteMiniLabel);

    if (automaticLOD.LODSwitchMode != AutomaticLOD.SwitchMode.UnityLODGroup)
    {
      Rect rectValue = new Rect(rect);
      rectValue.y += 14;
      GUI.Label(rectValue, strValue, EditorStyles.whiteMiniLabel);
    }
  }

  void LoadStaticData()
  {
    int nColors = 9;

    s_aColorTextures = new Texture2D[nColors];

    s_aColorTextures[0] = CreateTexture(new Color32(170,  57,  57, 255));
    s_aColorTextures[1] = CreateTexture(new Color32(170, 108,  57, 255));
    s_aColorTextures[2] = CreateTexture(new Color32( 34, 102, 102, 255));
    s_aColorTextures[3] = CreateTexture(new Color32( 45, 136,  45, 255));
    s_aColorTextures[4] = CreateTexture(new Color32(102,  34,  98, 255));
    s_aColorTextures[5] = CreateTexture(new Color32(169, 170,  57, 255));
    s_aColorTextures[6] = CreateTexture(new Color32(170,  57, 166, 255));
    s_aColorTextures[7] = CreateTexture(new Color32(45, 136,  200, 255));
    s_aColorTextures[8] = CreateTexture(new Color32(45, 136,   98, 255));

    s_texLODGroupSelected    = CreateTexture(new Color32(160, 160, 160, 255)); 
    s_texLODGroupNotSelected = CreateTexture(new Color32(120, 120, 120, 255));

    s_texBlack    = CreateTexture(Color.black);
    s_texDarkGray = CreateTexture(new Color32(80, 80, 80, 255));
    s_texArrow    = LoadTexture(s_aColorsIconArrow,  s_nIconArrowWidth,  s_nIconArrowHeight);
    s_texHandle   = LoadTexture(s_aColorsIconHandle, s_nIconHandleWidth, s_nIconHandleHeight);

    s_bStaticDataLoaded = true;
  }

  Texture2D LoadTexture(Color32[] aColors, int nWidth, int nHeight)
  {
    Texture2D newTexture = new Texture2D(nWidth, nHeight);

    newTexture.SetPixels32(aColors);
    newTexture.Apply();

    return newTexture;
  }

  Texture2D CreateTexture(Color color)
  {
    int nWidth  = 2;
    int nHeight = 2;

    Color[] aPixels = new Color[nWidth * nHeight];

    for (int nPixel = 0; nPixel < aPixels.Length; ++nPixel)
    {
      aPixels[nPixel] = color;
    }

    Texture2D texResult = new Texture2D(nWidth, nHeight);
    texResult.SetPixels(aPixels);
    texResult.wrapMode = TextureWrapMode.Repeat;
    texResult.Apply();

    return texResult;
  }

  void RestoreOriginalMeshes()
  {
    foreach (Object targetObject in targets)
    {
      if (targetObject != null)
      {
        AutomaticLOD automaticLOD = targetObject as AutomaticLOD;
        automaticLOD.RestoreOriginalMesh(false, true);
      }
    }
  }

  void DeleteLODData()
  {
    foreach (Object targetObject in targets)
    {
      AutomaticLOD automaticLOD = targetObject as AutomaticLOD;

      if (automaticLOD.m_LODObjectRoot == null)
      {
        DeleteLODDataRecursive(automaticLOD.gameObject, automaticLOD.gameObject, true);
      }
    }
  }

  void DeleteLODDataRecursive(GameObject root, GameObject gameObject, bool bRecurseIntoChildren)
  {
    AutomaticLOD automaticLOD = gameObject.GetComponent<AutomaticLOD>();

    if (automaticLOD)
    {
      if (automaticLOD.m_LODObjectRoot == null || automaticLOD.m_LODObjectRoot.gameObject == root)
      {
        automaticLOD.RestoreOriginalMesh(true, false);

        if (automaticLOD.m_LODObjectRoot != null)
        {
          if (Application.isEditor && Application.isPlaying == false)
          {
            UnityEngine.Object.DestroyImmediate(automaticLOD);
          }
          else
          {
            UnityEngine.Object.Destroy(automaticLOD);
          }
        }
      }

      automaticLOD.m_aRelevanceSpheres  = null;
      automaticLOD.m_bEnablePrefabUsage = false;
      automaticLOD.m_strAssetPath       = "";
    }

    if (bRecurseIntoChildren)
    {
      for (int nChild = 0; nChild < gameObject.transform.childCount; nChild++)
      {
        DeleteLODDataRecursive(root, gameObject.transform.GetChild(nChild).gameObject, true);
      }
    }
  }

  void SaveSceneViewCameraInfo()
  {
    AutomaticLOD automaticLOD = target as AutomaticLOD;

    m_dicSceneViewCameraInfos = new Dictionary<SceneView, SceneCameraInfo>();

    for (int i = 0; i < SceneView.sceneViews.Count; i++)
    {
      SceneView sceneView = SceneView.sceneViews[i] as SceneView;

      SceneCameraInfo info = new SceneCameraInfo();

      if (sceneView.camera != null)
      {
        info.m_v3Position = sceneView.camera.transform.position;
        info.m_fDistance  = Vector3.Distance(sceneView.camera.transform.position, sceneView.pivot);

        info.m_fViewArea = automaticLOD.ComputeViewSpaceBounds(sceneView.camera.transform.position, (automaticLOD.transform.position - info.m_v3Position).normalized, Vector3.up, out info.m_v3ViewSpaceMin, out info.m_v3ViewSpaceMax, out info.m_v3ViewSpaceCenter);
      }

      info.m_v3ObjectWorldCenter = automaticLOD.ComputeWorldCenter();
      info.m_v3Pivot             = sceneView.pivot;
      info.m_qRotation           = sceneView.rotation;
      info.m_fSize               = sceneView.size;

      m_dicSceneViewCameraInfos.Add(sceneView, info);
    }
  }

  void PreviewSceneViewCameras(AutomaticLOD automaticLOD, float fPosition)
  {
    for (int i = 0; i < SceneView.sceneViews.Count; i++)
    {
      SceneView sceneView = SceneView.sceneViews[i] as SceneView;

      if (m_dicSceneViewCameraInfos.ContainsKey(sceneView))
      {
        SceneCameraInfo info = m_dicSceneViewCameraInfos[sceneView];
        Vector3 v3AutomaticLODLookAt = info.m_v3ObjectWorldCenter;
        Vector3 v3Dir = (info.m_v3Position - v3AutomaticLODLookAt).normalized;

        float fDistance = 0.0f;

        if (automaticLOD.m_evalMode == AutomaticLOD.EvalMode.CameraDistance)
        {
          fDistance = Mathf.Max(0.05f, automaticLOD.m_fMaxCameraDistance * fPosition);
        }
        else if (automaticLOD.m_evalMode == AutomaticLOD.EvalMode.ScreenCoverage)
        {
          float fTargetArea  = Mathf.Max(SliderToScreenCoverage(fPosition), 0.00001f);
          float fTryDistance = Mathf.Max(info.m_v3ViewSpaceMax.x - info.m_v3ViewSpaceMin.x, info.m_v3ViewSpaceMax.y - info.m_v3ViewSpaceMin.y, info.m_v3ViewSpaceMax.z - info.m_v3ViewSpaceMin.z) * 2.0f;

          if (Mathf.Approximately(fTryDistance, 0.0f) || fTryDistance < 0.0f)
          {
            fDistance = Vector3.Distance(sceneView.camera.transform.position, v3AutomaticLODLookAt);
          }
          else
          {
            sceneView.camera.transform.position = v3AutomaticLODLookAt + (v3Dir * fTryDistance);
            sceneView.camera.transform.rotation = Quaternion.LookRotation(-v3Dir, Vector3.up);

            float fIncDistance = fTryDistance * 0.2f;
            float fTryCoverage = automaticLOD.ComputeScreenCoverage(sceneView.camera);
            bool  bTryForward  = false;

            int nTrySteps = 50;

            for (int nTry = 0; nTry < nTrySteps; nTry++)
            {
              if (fTryCoverage < fTargetArea)
              {
                // Move forward

                if(nTry > 0)
                {
                  if (bTryForward == false)
                  {
                    // Overshoot
                    fIncDistance *= -0.5f;
                  }

                  bTryForward = true;
                }
                else
                {
                  // Start forward
                  bTryForward = true;

                  if(fIncDistance > 0.0f)
                  {
                    fIncDistance *= -0.5f;
                  }
                }
              }
              else
              {
                // Move backwards

                if (nTry > 0)
                {
                  // Overshoot

                  if (bTryForward == true)
                  {
                    fIncDistance *= -0.5f;
                  }

                  bTryForward = false;
                }
                else
                {
                  // Start backwards
                  bTryForward = false;

                  if (fIncDistance < 0.0f)
                  {
                    fIncDistance *= -0.5f;
                  }
                }
              }

              fTryDistance += fIncDistance;

              sceneView.camera.transform.position = v3AutomaticLODLookAt + (v3Dir * fTryDistance);
              sceneView.camera.transform.rotation = Quaternion.LookRotation(-v3Dir, Vector3.up);

              fTryCoverage = automaticLOD.ComputeScreenCoverage(sceneView.camera);

              if (nTry == nTrySteps - 1)
              {
                //Debug.Log("Coverage try " + nTry + ": " + fTryCoverage + " Target: " + fTargetArea + " Error: " + (Mathf.Abs(fTryCoverage - fTargetArea)) + " IncDistance " + fIncDistance + " TryDistance " + fTryDistance);
              }
            }

            fDistance = fTryDistance;
          }
        }

        sceneView.LookAtDirect(v3AutomaticLODLookAt + (v3Dir * (fDistance - info.m_fDistance)), Quaternion.LookRotation(-v3Dir, Vector3.up));
      }
    }
  }

  void LoadSceneViewCameraInfos()
  {
    if(m_dicSceneViewCameraInfos == null)
    {
      return;
    }

    for (int i = 0; i < SceneView.sceneViews.Count; i++)
    {
      SceneView sceneView = SceneView.sceneViews[i] as SceneView;

      if (m_dicSceneViewCameraInfos.ContainsKey(sceneView) && sceneView.orthographic == false)
      {
        if(sceneView.camera != null)
        {
          sceneView.camera.transform.position = m_dicSceneViewCameraInfos[sceneView].m_v3Position;
        }

        sceneView.pivot    = m_dicSceneViewCameraInfos[sceneView].m_v3Pivot;
        sceneView.rotation = m_dicSceneViewCameraInfos[sceneView].m_qRotation;
        sceneView.size     = m_dicSceneViewCameraInfos[sceneView].m_fSize;
      }
    }
  }

  void SaveMeshAssets()
  {
    try
    {
      foreach (Object targetObject in targets)
      {
        AutomaticLOD automaticLOD = targetObject as AutomaticLOD;
        GameObject   gameObject   = automaticLOD.gameObject;

        if (automaticLOD.m_LODObjectRoot == null && automaticLOD.m_bEnablePrefabUsage)
        {
          string strMeshAssetPath = automaticLOD.m_strAssetPath;

          if (string.IsNullOrEmpty(strMeshAssetPath))
          {
            strMeshAssetPath = UnityEditor.EditorUtility.SaveFilePanelInProject("Save mesh asset(s)", "mesh_" + gameObject.name + gameObject.GetInstanceID().ToString() + ".asset", "asset", "Please enter a file name to save the mesh asset(s) to");

            if (strMeshAssetPath.Length == 0)
            {
              return;
            }

            automaticLOD.m_strAssetPath = strMeshAssetPath;
          }

          int nCounter = 0;
          SaveMeshAssetsRecursive(gameObject, gameObject, strMeshAssetPath, true, System.IO.File.Exists(strMeshAssetPath), ref nCounter);
        }
      }
    }
    catch (System.Exception e)
    {
      Debug.LogError("Error saving LOD assets to disk: " + e.Message + " Stack: " + e.StackTrace);
      EditorUtility.ClearProgressBar();
      Simplifier.Cancelled = false;
    }

    EditorUtility.ClearProgressBar();
    UnityEditor.AssetDatabase.Refresh();
    Simplifier.Cancelled = false;
  }

  bool SaveMeshAssetsRecursive(GameObject root, GameObject gameObject, string strFile, bool bRecurseIntoChildren, bool bAssetAlreadyCreated, ref int nProgressElementsCounter)
  {
    if (gameObject == null || Simplifier.Cancelled)
    {
      return bAssetAlreadyCreated;
    }

    AutomaticLOD automaticLOD = gameObject.GetComponent<AutomaticLOD>();

    if (automaticLOD != null && automaticLOD.HasLODData() && (automaticLOD.m_LODObjectRoot == null || automaticLOD.m_LODObjectRoot.gameObject == root))
    {
      int nTotalProgressElements = automaticLOD.m_LODObjectRoot != null ? (automaticLOD.m_LODObjectRoot.m_listDependentChildren.Count + 1) : 1;
      nTotalProgressElements *= automaticLOD.m_listLODLevels != null ? automaticLOD.m_listLODLevels.Count : 0;

      for (int nLOD = 0; nLOD < automaticLOD.m_listLODLevels.Count; nLOD++)
      {
        if (automaticLOD.m_listLODLevels[nLOD].m_mesh != null && AutomaticLOD.HasValidMeshData(automaticLOD.gameObject))
        {
          float fT = (float)nProgressElementsCounter / (float)nTotalProgressElements;
          Progress("Saving meshes to asset file", automaticLOD.name + " LOD " + nLOD, fT);

          if (Simplifier.Cancelled)
          {
            return bAssetAlreadyCreated;
          }

          if (bAssetAlreadyCreated == false && UnityEditor.AssetDatabase.Contains(automaticLOD.m_listLODLevels[nLOD].m_mesh) == false)
          {
            UnityEditor.AssetDatabase.CreateAsset(automaticLOD.m_listLODLevels[nLOD].m_mesh, strFile);
            bAssetAlreadyCreated = true;
          }
          else
          {
            if (UnityEditor.AssetDatabase.Contains(automaticLOD.m_listLODLevels[nLOD].m_mesh) == false)
            {
              UnityEditor.AssetDatabase.AddObjectToAsset(automaticLOD.m_listLODLevels[nLOD].m_mesh, strFile);
              UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(automaticLOD.m_listLODLevels[nLOD].m_mesh));
            }
          }

          nProgressElementsCounter++;
        }
      }
    }

    if (bRecurseIntoChildren)
    {
      for (int nChild = 0; nChild < gameObject.transform.childCount; nChild++)
      {
        bAssetAlreadyCreated = SaveMeshAssetsRecursive(root, gameObject.transform.GetChild(nChild).gameObject, strFile, bRecurseIntoChildren, bAssetAlreadyCreated, ref nProgressElementsCounter);
      }
    }

    return bAssetAlreadyCreated;
  }

  void SetHideFlags()
  {
    foreach (Object targetObject in targets)
    {
      AutomaticLOD automaticLOD = targetObject as AutomaticLOD;

      if (automaticLOD.m_LODObjectRoot == null)
      {
        SetHideFlagsRecursive(automaticLOD.gameObject, automaticLOD.gameObject, true);
      }
    }
  }

  void SetHideFlagsRecursive(GameObject root, GameObject gameObject, bool bRecurseIntoChildren)
  {
    AutomaticLOD automaticLOD = gameObject.GetComponent<AutomaticLOD>();

    if (automaticLOD && automaticLOD.GetMeshSimplifier())
    {
      automaticLOD.GetMeshSimplifier().hideFlags = HideFlags.HideInInspector;
    }

    if (bRecurseIntoChildren)
    {
      for (int nChild = 0; nChild < gameObject.transform.childCount; nChild++)
      {
        SetHideFlagsRecursive(root, gameObject.transform.GetChild(nChild).gameObject, true);
      }
    }
  }

  float SliderToScreenCoverage(float fSlider)
  {
    return Mathf.Pow(1.0f - fSlider, COVERAGEPOWER);
  }

  float ScreenCoverageToSlider(float fCoverage)
  {
    return 1.0f - Mathf.Pow(fCoverage, 1.0f / COVERAGEPOWER);
  }

  class SceneCameraInfo
  {
    public Vector3    m_v3ObjectWorldCenter;
    public Vector3    m_v3Position;
    public Vector3    m_v3Pivot;
    public Quaternion m_qRotation;
    public float      m_fSize;
    public float      m_fDistance;

    public Vector3    m_v3ViewSpaceMin;
    public Vector3    m_v3ViewSpaceMax;
    public Vector3    m_v3ViewSpaceCenter;
    public float      m_fViewArea;
  }

  const string MENUADDBEFORE = "Add new/Before";
  const string MENUADDAFTER  = "Add new/After";
  const string MENUDELETE    = "Delete";
  const float  COVERAGEPOWER = 6.0f;

  Dictionary<SceneView, SceneCameraInfo> m_dicSceneViewCameraInfos = null;

  int         m_nCurrentLODResizing;
  int         m_nCurrentLODSelected;
  bool        m_bPreviewingLOD;
  float       m_fPreviewLODPos;
  bool        m_bGenerateLODData         = false;
  bool        m_bRecomputeAllMeshes      = false;
  bool        m_bEnablePrefabUsage       = false;
  bool        m_bDisablePrefabUsage      = false;
  bool        m_bDeleteLODData           = false;
  bool        m_bRemoveFromLODTree       = false;
  bool        m_bSetupNewRelevanceSphere = false;

  SerializedProperty PropertyGenerateIncludeChildren;
  SerializedProperty PropertyLevelsToGenerate;
  SerializedProperty PropertySwitchMode;
  SerializedProperty PropertyEvalMode;
  SerializedProperty PropertyEnablePrefabUsage;
  SerializedProperty PropertyMaxCameraDistanceDistance;
  SerializedProperty PropertyListLODLevels;
  SerializedProperty PropertyExpandRelevanceSpheres;
  SerializedProperty PropertyRelevanceSpheres;
  SerializedProperty PropertyOverrideRootSettings;
  SerializedProperty PropertyLODDataDirty;

  static bool        s_bStaticDataLoaded      = false;
  static Texture2D[] s_aColorTextures         = null;
  static Texture2D   s_texBlack               = null;
  static Texture2D   s_texDarkGray            = null;
  static Texture2D   s_texLODGroupSelected    = null;
  static Texture2D   s_texLODGroupNotSelected = null;
  static Texture2D   s_texHandle              = null;
  static Texture2D   s_texArrow               = null;
  static int         s_nIconArrowWidth        = 20;
  static int         s_nIconArrowHeight       = 10;
  static int         s_nIconHandleWidth       = 13;
  static int         s_nIconHandleHeight      = 47;
  static Color32[]   s_aColorsIconArrow       = new Color32[] { new Color32(255, 255, 255, 64), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 64), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 64), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 64), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 64), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 64), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 64), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 64), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 64), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 64), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 64), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 64), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 64), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 64), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 64), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 64), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 64), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 128), new Color32(255, 255, 255, 64), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 64), new Color32(255, 255, 255, 64), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0), new Color32(255, 255, 255, 0) };
  static Color32[]   s_aColorsIconHandle      = new Color32[] { new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 112), new Color32(39, 39, 39, 191), new Color32(39, 39, 39, 112), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 191), new Color32(125, 125, 123, 255), new Color32(39, 39, 39, 191), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 191), new Color32(125, 125, 123, 255), new Color32(39, 39, 39, 191), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 191), new Color32(125, 125, 123, 255), new Color32(39, 39, 39, 191), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 191), new Color32(125, 125, 123, 255), new Color32(39, 39, 39, 191), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 191), new Color32(125, 125, 123, 255), new Color32(39, 39, 39, 191), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 191), new Color32(125, 125, 123, 255), new Color32(39, 39, 39, 191), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 191), new Color32(125, 125, 123, 255), new Color32(39, 39, 39, 191), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 191), new Color32(125, 125, 123, 255), new Color32(39, 39, 39, 191), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 191), new Color32(125, 125, 123, 255), new Color32(39, 39, 39, 191), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 191), new Color32(125, 125, 123, 255), new Color32(39, 39, 39, 191), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 191), new Color32(125, 125, 123, 255), new Color32(39, 39, 39, 191), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 191), new Color32(125, 125, 123, 255), new Color32(39, 39, 39, 191), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 191), new Color32(125, 125, 123, 255), new Color32(39, 39, 39, 191), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 191), new Color32(125, 125, 123, 255), new Color32(39, 39, 39, 191), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 191), new Color32(125, 125, 123, 255), new Color32(39, 39, 39, 191), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 191), new Color32(125, 125, 123, 255), new Color32(39, 39, 39, 191), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 191), new Color32(125, 125, 123, 255), new Color32(39, 39, 39, 191), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 191), new Color32(125, 125, 123, 255), new Color32(39, 39, 39, 191), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 191), new Color32(125, 125, 123, 255), new Color32(39, 39, 39, 191), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 191), new Color32(125, 125, 123, 255), new Color32(39, 39, 39, 191), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 191), new Color32(125, 125, 123, 255), new Color32(39, 39, 39, 191), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 191), new Color32(125, 125, 123, 255), new Color32(39, 39, 39, 191), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 191), new Color32(125, 125, 123, 255), new Color32(39, 39, 39, 191), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 191), new Color32(125, 125, 123, 255), new Color32(39, 39, 39, 191), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 191), new Color32(125, 125, 123, 255), new Color32(39, 39, 39, 191), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 191), new Color32(125, 125, 123, 255), new Color32(39, 39, 39, 191), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 191), new Color32(125, 125, 123, 255), new Color32(39, 39, 39, 191), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 191), new Color32(125, 125, 123, 255), new Color32(39, 39, 39, 191), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 191), new Color32(125, 125, 123, 255), new Color32(39, 39, 39, 191), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 191), new Color32(125, 125, 123, 255), new Color32(39, 39, 39, 191), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 191), new Color32(125, 125, 123, 255), new Color32(39, 39, 39, 191), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(39, 39, 39, 0), new Color32(0, 0, 0, 0), new Color32(0, 0, 0, 0), new Color32(0, 0, 0, 0), new Color32(0, 0, 0, 0), new Color32(0, 0, 0, 1), new Color32(36, 36, 36, 194), new Color32(114, 114, 112, 255), new Color32(36, 36, 36, 195), new Color32(0, 0, 0, 2), new Color32(0, 0, 0, 0), new Color32(0, 0, 0, 0), new Color32(0, 0, 0, 0), new Color32(0, 0, 0, 0), new Color32(0, 0, 0, 0), new Color32(0, 0, 0, 0), new Color32(0, 0, 0, 0), new Color32(0, 0, 0, 2), new Color32(0, 0, 0, 18), new Color32(28, 28, 28, 206), new Color32(80, 80, 79, 255), new Color32(27, 27, 27, 208), new Color32(0, 0, 0, 23), new Color32(0, 0, 0, 3), new Color32(0, 0, 0, 0), new Color32(0, 0, 0, 0), new Color32(0, 0, 0, 0), new Color32(0, 0, 0, 0), new Color32(0, 0, 0, 0), new Color32(0, 0, 0, 3), new Color32(0, 0, 0, 22), new Color32(0, 0, 0, 70), new Color32(36, 36, 36, 228), new Color32(132, 132, 130, 255), new Color32(49, 48, 48, 232), new Color32(0, 0, 0, 78), new Color32(0, 0, 0, 27), new Color32(0, 0, 0, 4), new Color32(0, 0, 0, 0), new Color32(0, 0, 0, 0), new Color32(0, 0, 0, 0), new Color32(0, 0, 0, 4), new Color32(0, 0, 0, 26), new Color32(0, 0, 0, 77), new Color32(48, 48, 48, 160), new Color32(136, 136, 135, 251), new Color32(138, 138, 136, 255), new Color32(142, 141, 140, 253), new Color32(64, 65, 64, 176), new Color32(0, 0, 0, 85), new Color32(0, 0, 0, 31), new Color32(0, 0, 0, 5), new Color32(0, 0, 0, 0), new Color32(0, 0, 0, 5), new Color32(0, 0, 0, 31), new Color32(0, 0, 0, 85), new Color32(62, 62, 61, 174), new Color32(142, 142, 141, 251), new Color32(136, 136, 135, 255), new Color32(133, 133, 131, 255), new Color32(135, 135, 133, 255), new Color32(144, 144, 143, 253), new Color32(78, 78, 78, 187), new Color32(0, 0, 0, 91), new Color32(0, 0, 0, 34), new Color32(0, 0, 0, 6), new Color32(0, 0, 0, 22), new Color32(0, 0, 0, 85), new Color32(79, 79, 79, 187), new Color32(148, 147, 146, 253), new Color32(139, 138, 137, 255), new Color32(136, 136, 134, 255), new Color32(135, 136, 134, 255), new Color32(136, 135, 134, 255), new Color32(137, 138, 136, 255), new Color32(146, 146, 145, 255), new Color32(91, 91, 91, 197), new Color32(0, 0, 0, 90), new Color32(0, 0, 0, 24), new Color32(0, 0, 0, 41), new Color32(103, 103, 103, 186), new Color32(150, 150, 149, 255), new Color32(141, 141, 140, 255), new Color32(139, 139, 138, 255), new Color32(139, 139, 138, 255), new Color32(139, 139, 138, 255), new Color32(139, 139, 138, 255), new Color32(139, 139, 138, 255), new Color32(140, 141, 140, 255), new Color32(150, 150, 149, 255), new Color32(113, 113, 112, 196), new Color32(0, 0, 0, 42), new Color32(0, 0, 0, 48), new Color32(160, 159, 159, 255), new Color32(145, 145, 144, 255), new Color32(142, 143, 142, 255), new Color32(142, 143, 142, 255), new Color32(143, 142, 142, 255), new Color32(143, 142, 142, 255), new Color32(143, 142, 142, 255), new Color32(142, 142, 142, 255), new Color32(143, 143, 142, 255), new Color32(145, 145, 144, 255), new Color32(159, 160, 159, 255), new Color32(0, 0, 0, 48), new Color32(0, 0, 0, 48), new Color32(159, 159, 159, 255), new Color32(146, 147, 146, 255), new Color32(146, 147, 146, 255), new Color32(147, 147, 146, 255), new Color32(146, 146, 146, 255), new Color32(147, 147, 146, 255), new Color32(147, 146, 146, 255), new Color32(147, 146, 146, 255), new Color32(146, 146, 146, 255), new Color32(146, 147, 146, 255), new Color32(159, 160, 159, 255), new Color32(0, 0, 0, 48), new Color32(0, 0, 0, 48), new Color32(162, 162, 162, 255), new Color32(150, 150, 150, 255), new Color32(150, 150, 150, 255), new Color32(150, 150, 150, 255), new Color32(150, 150, 150, 255), new Color32(150, 150, 150, 255), new Color32(150, 150, 150, 255), new Color32(150, 150, 150, 255), new Color32(150, 150, 150, 255), new Color32(150, 150, 150, 255), new Color32(162, 162, 162, 255), new Color32(0, 0, 0, 48), new Color32(0, 0, 0, 48), new Color32(165, 166, 166, 255), new Color32(153, 154, 154, 255), new Color32(153, 154, 153, 255), new Color32(153, 153, 154, 255), new Color32(154, 153, 154, 255), new Color32(153, 153, 154, 255), new Color32(154, 154, 153, 255), new Color32(153, 154, 154, 255), new Color32(154, 153, 154, 255), new Color32(154, 153, 154, 255), new Color32(166, 166, 166, 255), new Color32(0, 0, 0, 48), new Color32(0, 0, 0, 48), new Color32(169, 169, 169, 255), new Color32(157, 157, 157, 255), new Color32(157, 157, 158, 255), new Color32(157, 156, 158, 255), new Color32(157, 157, 157, 255), new Color32(157, 157, 158, 255), new Color32(157, 157, 157, 255), new Color32(157, 157, 157, 255), new Color32(156, 156, 158, 255), new Color32(157, 157, 157, 255), new Color32(169, 169, 169, 255), new Color32(0, 0, 0, 48), new Color32(0, 0, 0, 48), new Color32(171, 171, 172, 255), new Color32(160, 160, 161, 255), new Color32(160, 160, 161, 255), new Color32(160, 160, 161, 255), new Color32(160, 160, 160, 255), new Color32(160, 160, 160, 255), new Color32(160, 160, 161, 255), new Color32(160, 160, 161, 255), new Color32(160, 159, 160, 255), new Color32(159, 160, 160, 255), new Color32(171, 171, 171, 255), new Color32(0, 0, 0, 48), new Color32(0, 0, 0, 36), new Color32(173, 173, 174, 255), new Color32(162, 162, 163, 255), new Color32(162, 162, 163, 255), new Color32(162, 162, 163, 255), new Color32(162, 162, 163, 255), new Color32(163, 162, 163, 255), new Color32(162, 163, 163, 255), new Color32(162, 162, 163, 255), new Color32(163, 162, 163, 255), new Color32(163, 162, 163, 255), new Color32(173, 174, 174, 255), new Color32(0, 0, 0, 36), new Color32(0, 0, 0, 12), new Color32(183, 183, 184, 255), new Color32(175, 175, 176, 255), new Color32(175, 175, 176, 255), new Color32(175, 175, 176, 255), new Color32(175, 175, 176, 255), new Color32(175, 175, 176, 255), new Color32(175, 175, 176, 255), new Color32(175, 175, 176, 255), new Color32(175, 175, 176, 255), new Color32(175, 175, 176, 255), new Color32(183, 183, 184, 255), new Color32(0, 0, 0, 12) };

  static int    s_nLastProgress  = -1;
  static string s_strLastTitle   = "";
  static string s_strLastMessage = "";
}
