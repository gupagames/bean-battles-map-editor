using UnityEditor;
using UnityEngine;

namespace GG.BeanBattles.MapEditor
{
    [CustomPropertyDrawer(typeof(ShowOnlyAttribute))]
    public class ShowOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            string valueStr;

            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    valueStr = prop.intValue.ToString();
                    break;
                case SerializedPropertyType.Boolean:
                    valueStr = prop.boolValue.ToString();
                    break;
                case SerializedPropertyType.Float:
                    valueStr = prop.floatValue.ToString("0.00000");
                    break;
                case SerializedPropertyType.Vector2:
                    valueStr = prop.vector2Value.ToString();
                    break;
                case SerializedPropertyType.String:
                    valueStr = prop.stringValue;
                    break;
                case SerializedPropertyType.Enum:
                    string display = "";
                    if (prop.enumValueIndex >= 0) display += prop.enumDisplayNames[prop.enumValueIndex] + " - ";
                    display += prop.intValue.ToString();
                    valueStr = display;
                    break;
                case SerializedPropertyType.ObjectReference:
                    valueStr = prop.name; // no work lol
                    break;
                default:
                    valueStr = "(not supported)";
                    break;
            }

            var labelCopy = new GUIContent(label.text);
            var showOnly = (ShowOnlyAttribute)attribute;
            if (!string.IsNullOrEmpty(showOnly.Tooltip)) labelCopy.tooltip = showOnly.Tooltip;

            EditorGUI.LabelField(position, labelCopy, new GUIContent(valueStr));
        }
    }
}