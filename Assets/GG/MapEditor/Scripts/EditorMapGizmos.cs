using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GG.BeanBattles.MapEditor
{
    public static class EditorMapGizmos
    {
        public static void DrawSpawn(Vector3 position, Quaternion rotation, Vector3 size, Vector3 offset, Color color, string label, Vector3 labelOffset)
        {
            var relativeMatrix = Matrix4x4.TRS(position, rotation, Vector3.one);

            Gizmos.color = color;
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = relativeMatrix;
            Gizmos.DrawWireCube(offset, size);

            if (string.IsNullOrEmpty(label)) return;

#if UNITY_EDITOR
            SceneView sceneView = SceneView.currentDrawingSceneView;

            if (sceneView == null) return;

            Camera cam = sceneView.camera;

            float distance = Vector3.Distance(cam.transform.position, position);

            // scale based on distance
            int fontSize = Mathf.Clamp((int)(200 / distance), 1, 40);

            if (fontSize <= 1) return; // too small, just dont draw it

            GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
            style.normal.textColor = color;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = fontSize;

            Vector3 labelPos = offset + labelOffset;
            Handles.matrix = relativeMatrix;
            Handles.Label(labelPos, label, style);
            Handles.matrix = oldMatrix;
#endif
        }
    }
}