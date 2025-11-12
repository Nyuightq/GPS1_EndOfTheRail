// --------------------------------------------------------------
// Creation Date: 2025-11-07 09:26
// Author: User
// Description: -
// --------------------------------------------------------------

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

[CustomEditor(typeof(ItemEffect_Condition_Comparison))]
public class ItemEffect_Condition_ComparisonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty valueAProp = serializedObject.FindProperty("conditionValueA");
        SerializedProperty valueBProp = serializedObject.FindProperty("conditionValueB");

        DrawBaseValueField(valueAProp, "Condition Value A");
        DrawBaseValueField(valueBProp, "Condition Value B");

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("condition"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("healAmount"));

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawBaseValueField(SerializedProperty property, string label)
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        if (property.managedReferenceValue == null)
        {
            if (GUILayout.Button("Add Value"))
            {
                var menu = new GenericMenu();
                var types = typeof(baseValue).Assembly.GetTypes()
                    .Where(t => t.IsSubclassOf(typeof(baseValue)) && !t.IsAbstract)
                    .ToArray();

                foreach (var type in types)
                {
                    menu.AddItem(new GUIContent(type.Name), false, () =>
                    {
                        property.managedReferenceValue = Activator.CreateInstance(type);
                        serializedObject.ApplyModifiedProperties();
                    });
                }

                menu.ShowAsContext();
            }
        }
        else
        {
            EditorGUILayout.PropertyField(property, true);

            if (GUILayout.Button("Clear Value"))
            {
                property.managedReferenceValue = null;
            }
        }
    }
}
#endif
