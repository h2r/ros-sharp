using UnityEngine;
using System.Collections;

public class LODSceneSelection : MonoBehaviour
{
  [System.Serializable]
  public class SceneOption
  {
    public string m_sceneName;
    public string m_sceneDisplayName;
  }

  public int BoxWidth  = 300;
  public int BoxHeight = 50;
  public int MarginH   = 20;
  public int MarginV   = 20;

  public SceneOption[] SceneOptions;

	void OnGUI()
  {
    Rect boxRect  = new Rect((Screen.width / 2) - (BoxWidth / 2), 0, BoxWidth, BoxHeight);
    Rect areaRect = new Rect(boxRect.x + MarginH, boxRect.y + MarginV, BoxWidth - (MarginH * 2), BoxHeight - (MarginV * 2));
    GUI.Box(boxRect, "");
    GUI.Box(boxRect, "");
    GUILayout.BeginArea(areaRect);

    GUILayout.Label("Scene selection:");

    GUILayout.BeginHorizontal();
    foreach(SceneOption sceneOption in SceneOptions)
    {
      if(GUILayout.Button(sceneOption.m_sceneDisplayName))
      {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneOption.m_sceneName);
      }
    }

    if (GUILayout.Button("Exit"))
    {
      Application.Quit();
    }

    GUILayout.EndHorizontal();

    GUILayout.EndArea();
  }
}
