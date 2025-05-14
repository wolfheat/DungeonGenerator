using UnityEngine;
using UnityEditor;
using EditorToolkit;
using static EditorToolkit.SimpleSceneViewPanel;

[ExecuteInEditMode]
public class SceneViewPanelTest : MonoBehaviour
{

    [SerializeField] bool _showPanel = true;
    [SerializeField] ScreenCorner _corner = ScreenCorner.BottomRight;

    SimpleSceneViewPanel _panel;

    void OnValidate()
    {
        if (_panel is not null) {
            _panel.Visible = _showPanel;
            _panel.Corner = _corner;
        }
    }

    void OnEnable()
    {
        _panel = new SimpleSceneViewPanel("My Panel", 160f, rows: 4, panelInfo, _corner);
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
    bool _snap;
    int _button;

    void onSceneGui(SceneView view)
    {
        var e = Event.current;

        if (e.isMouse) {
            _mp = e.mousePosition;
            _mpw = toWorldPoint(_mp, _snap);

            if (_panel.Visible) view.Repaint();
        }
    }

    Vector3 toWorldPoint(Vector2 pos, bool snap)
    {
        var ray = HandleUtility.GUIPointToWorldRay(pos);
        var plane = new Plane(Vector3.back, Vector3.zero);
        plane.Raycast(ray, out var result);
        var point = ray.origin + result * ray.direction;
        if (snap) return (Vector3)snapToGrid((Vector2)point);
        return point;
    }

    Vector2 snapToGrid(Vector2 point)
      => new Vector2(Mathf.Round(point.x), Mathf.Round(point.y));

    PanelRow panelInfo(int row)
      => row switch
      {
          0 => PanelRow.Label($"Mouse: {(Vector2)_mpw:F3}"),
          1 => PanelRow.Check("Snap to integer", _snap, (i, f) => _snap = f),
          2 => PanelRow.Button("Press me", (i, f) => buttonPress(0), _button != 0),
          3 => PanelRow.Button("Press me", (i, f) => buttonPress(1), _button == 0),
          _ => null
      };

    void buttonPress(int index)
    {
        Debug.Log($"Hello world {index}");
        _button = index;
    }

}