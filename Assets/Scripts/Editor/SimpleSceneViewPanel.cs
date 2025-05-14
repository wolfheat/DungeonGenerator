using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EditorToolkit
{

    public class SimpleSceneViewPanel
    {

#if UNITY_EDITOR
        private const float VPAD = 6f, HPAD = 8f, SPAD = 12f, ROW_H = 16f;
#endif

        public string Caption { get; private set; }
        public ScreenCorner Corner { get; set; }
        public int Rows { get; private set; }

        public bool Visible { get; set; }

        bool _enabled;
        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
#if UNITY_EDITOR
                if (_enabled) SceneView.duringSceneGui += onSceneGui;
                else SceneView.duringSceneGui -= onSceneGui;
#endif
            }
        }

        public delegate PanelRow CallbackDelegate(int row);
        CallbackDelegate _callback;

        int _windowId = -1;

        public SimpleSceneViewPanel(string caption, float width, int rows, CallbackDelegate callback, ScreenCorner corner = ScreenCorner.BottomLeft)
        {
            Caption = caption;
            Rows = rows;
            _callback = callback;
            Corner = corner;

#if UNITY_EDITOR
            _windowId = GUIUtility.GetControlID(FocusType.Passive);
            determinePanelSize(width);
#endif

            Enabled = true;
            Visible = true;
        }

        public void Refresh()
        {
#if UNITY_EDITOR
            if (_lastCamera != null) drawWindow(_lastCamera);
#endif
        }

#if UNITY_EDITOR
        Vector2 _panelSize;
        Camera _lastCamera;

        void onSceneGui(SceneView view)
        {
            if (Visible) drawWindow(_lastCamera = view.camera);
        }

        void drawWindow(Camera sceneCam)
          => GUI.Window(_windowId, getWindowRect(Corner, sceneCam.pixelRect.size), drawWindowContent, Caption);

        Rect getWindowRect(ScreenCorner corner, Vector2 screenSize)
        {
            return new Rect(getPos(corner, screenSize), _panelSize);

            Vector2 getPos(ScreenCorner corner, Vector2 screenSize)
              => corner switch
              {
                  ScreenCorner.TopLeft => vec(-1, -1),
                  ScreenCorner.TopRight => vec(+1, -1),
                  ScreenCorner.BottomRight => vec(+1, +1),
                  _ => vec(-1, +1)
              };

            Vector2 vec(int sign1, int sign2) => new Vector2(coord(0, sign1), coord(1, sign2));
            float coord(int axis, int sign) => sign < 0 ? SPAD : screenSize[axis] - _panelSize[axis] - SPAD;
        }

        void drawWindowContent(int id)
        {
            if (id != _windowId || _callback is null) return;

            var rect = new Rect(HPAD, VPAD + ROW_H, _panelSize.x - 2f * HPAD, ROW_H);

            for (int i = 0; i < Rows; i++) {
                var feedback = _callback(i);
                if (feedback is null) continue;

                var saved = GUI.enabled;
                GUI.enabled = feedback.enabled;

                Rect nr;

                switch (feedback.rowClass) {

                    case RowClass.Label:
                        GUI.Label(rect, feedback.label);
                        rect.y += ROW_H;
                        break;

                    case RowClass.Button:
                        rect.y += 4f;
                        nr = new Rect(rect);
                        nr.height += 1f;
                        GUI.enabled &= feedback.onClick is not null;
                        if (GUI.Button(nr, feedback.label)) feedback.onClick.Invoke(i, true);
                        rect.y += ROW_H + 2f;
                        break;

                    case RowClass.Check:
                        rect.y += 4f;
                        nr = new Rect(rect);
                        nr.height += 1f;
                        GUI.enabled &= feedback.onClick is not null;
                        var oldState = feedback.state;
                        var newState = GUI.Toggle(nr, oldState, feedback.label, EditorStyles.miniButton);
                        if (newState != oldState) feedback.onClick.Invoke(i, newState);
                        rect.y += ROW_H + 2f;
                        break;

                }

                GUI.enabled = saved;
            }
        }

        void determinePanelSize(float width)
        {
            var h = ROW_H + VPAD / 2f - 1f;

            if (_callback is not null) {
                for (int i = 0; i < Rows; i++) {
                    var feedback = _callback(i);
                    if (feedback is null) continue;

                    h += feedback.rowClass switch
                    {
                        RowClass.Button => ROW_H + 6f,
                        RowClass.Check => ROW_H + 6f,
                        _ => ROW_H
                    };
                }
            }

            _panelSize = new Vector2(width, 2f * VPAD + h);
        }
#endif

        public enum ScreenCorner
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        public enum RowClass
        {
            Label,
            Button,
            Check
        }

        public class PanelRow
        {

            public RowClass rowClass;
            public string label;
            public bool enabled;
            public bool state;
            public Action<int, bool> onClick;

            static public PanelRow Label(string label)
              => new PanelRow()
              {
                  rowClass = RowClass.Label,
                  label = label,
                  enabled = true,
                  onClick = null
              };

            static public PanelRow Button(string label, Action<int, bool> onClick, bool enabled = true)
              => new PanelRow()
              {
                  rowClass = RowClass.Button,
                  label = label,
                  enabled = enabled,
                  onClick = onClick
              };

            static public PanelRow Check(string label, bool value, Action<int, bool> onClick, bool enabled = true)
              => new PanelRow()
              {
                  rowClass = RowClass.Check,
                  label = label,
                  state = value,
                  enabled = enabled,
                  onClick = onClick
              };

        }

    }

}