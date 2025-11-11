// --------------------------------------------------------------
// Creation Date: 2025-11-11 10:48
// Author: nyuig
// Description: -
// --------------------------------------------------------------
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyProgressionData))]
public class EnemyProgressionDataEditor : Editor
{
    private SerializedProperty entityNameProp;
    private SerializedProperty dataProp;

    private void OnEnable()
    {
        entityNameProp = serializedObject.FindProperty("entityName");
        dataProp = serializedObject.FindProperty("data");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Enemy Progression Data", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(entityNameProp);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Daily Encounter Data", EditorStyles.boldLabel);

        // Draw list size and items
        for (int i = 0; i < dataProp.arraySize; i++)
        {
            SerializedProperty dayData = dataProp.GetArrayElementAtIndex(i);
            SerializedProperty dayNumber = dayData.FindPropertyRelative("dayNumber");
            SerializedProperty combatStats = dayData.FindPropertyRelative("combatStats");

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Day {dayNumber.intValue}", EditorStyles.boldLabel);
            if (GUILayout.Button("X", GUILayout.Width(22)))
            {
                dataProp.DeleteArrayElementAtIndex(i);
                break;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(dayNumber);

            EditorGUILayout.PropertyField(combatStats, new GUIContent("Combat Stats"), true);

            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("+ Add New Day"))
        {
            dataProp.InsertArrayElementAtIndex(dataProp.arraySize);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
