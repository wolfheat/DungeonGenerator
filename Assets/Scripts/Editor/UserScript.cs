using UnityEngine;
using UnityEditor;
using EditorToolkit;
using static EditorToolkit.SimpleSceneViewPanel;


[ExecuteInEditMode]
public class UserScript : MonoBehaviour
{

    SimpleSceneViewPanel _panel;

    void OnEnable()
    {
        _panel = new SimpleSceneViewPanel("My Panel", 100f, rows: 3, panelInfo);
        SceneView.duringSceneGui += onSceneGui;
    }

    void OnDisable()
    {
        if (_panel is not null) {
            _panel.Enabled = false;
            _panel = null;
        }
        SceneView.duringSceneGui -= onSceneGui;
    }

    Vector2 _mp;
    Vector3 _mpw;

    void onSceneGui(SceneView view)
    {
        var e = Event.current;

        if (e.isMouse) {
            _mp = e.mousePosition;
            _mpw = toWorldPoint(_mp);
        }
    }

    Vector3 toWorldPoint(Vector2 pos)
    {
        var ray = HandleUtility.GUIPointToWorldRay(pos);
        var plane = new Plane(Vector3.back, Vector3.zero);
        plane.Raycast(ray, out var result);
        return ray.origin + result * ray.direction;
    }

    PanelRow panelInfo(int row)
      => row switch
      {
          0 => PanelRow.Label("label #1"),
          1 => PanelRow.Label("label #2"),
          2 => PanelRow.Label("label #3"),
          _ => null
      };

}