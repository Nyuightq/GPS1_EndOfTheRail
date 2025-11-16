using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnemyNumberEncounterData))]
public class EnemyNumberEncounterDataEditor : Editor
{
    private SerializedProperty dataProp;
    private bool[] foldouts;

    private void OnEnable()
    {
        dataProp = serializedObject.FindProperty("data");
        foldouts = new bool[Mathf.Max(dataProp.arraySize, 1)];
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Enemy Encounter Data", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        // Buttons for adding/removing days
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+ Add Day", GUILayout.Width(100)))
        {
            int index = dataProp.arraySize;
            dataProp.InsertArrayElementAtIndex(index);
            SerializedProperty newDay = dataProp.GetArrayElementAtIndex(index);
            newDay.FindPropertyRelative("dayNumber").intValue = index + 1;
        }

        if (GUILayout.Button("- Remove Last", GUILayout.Width(100)) && dataProp.arraySize > 0)
        {
            dataProp.DeleteArrayElementAtIndex(dataProp.arraySize - 1);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        // Loop through all day data
        for (int i = 0; i < dataProp.arraySize; i++)
        {
            SerializedProperty dayProp = dataProp.GetArrayElementAtIndex(i);
            SerializedProperty dayNumberProp = dayProp.FindPropertyRelative("dayNumber");
            SerializedProperty weightsProp = dayProp.FindPropertyRelative("encounterWeights");

            if (foldouts.Length <= i)
                System.Array.Resize(ref foldouts, dataProp.arraySize);

            string dayTitle = $"Day {dayNumberProp.intValue}";
            GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold };
            foldouts[i] = EditorGUILayout.Foldout(foldouts[i], dayTitle, true, foldoutStyle);

            if (foldouts[i])
            {
                EditorGUI.indentLevel++;

                // Editable Day number
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Day Number", GUILayout.Width(90));
                dayNumberProp.intValue = EditorGUILayout.IntField(dayNumberProp.intValue);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Enemy Encounter Weights", EditorStyles.miniBoldLabel);

                // --- CALCULATE TOTAL WEIGHT ---
                float totalWeight = 0f;
                for (int j = 0; j < weightsProp.arraySize; j++)
                {
                    totalWeight += weightsProp.GetArrayElementAtIndex(j)
                        .FindPropertyRelative("weight").intValue;
                }

                // --- Display each weight with percentage ---
                for (int j = 0; j < weightsProp.arraySize; j++)
                {
                    SerializedProperty weightProp = weightsProp.GetArrayElementAtIndex(j);
                    SerializedProperty countProp = weightProp.FindPropertyRelative("enemyCount");
                    SerializedProperty wProp = weightProp.FindPropertyRelative("weight");

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField("Enemy Count", GUILayout.Width(90));
                    countProp.intValue = EditorGUILayout.IntField(countProp.intValue, GUILayout.Width(60));

                    EditorGUILayout.LabelField("Weight", GUILayout.Width(50));
                    wProp.intValue = EditorGUILayout.IntField(wProp.intValue, GUILayout.Width(50));

                    float percent = (totalWeight > 0) ? (wProp.intValue / totalWeight * 100f) : 0f;
                    GUI.enabled = false;
                    EditorGUILayout.FloatField(percent, GUILayout.Width(50));
                    GUI.enabled = true;
                    EditorGUILayout.LabelField("%", GUILayout.Width(15));

                    if (GUILayout.Button("X", GUILayout.Width(25)))
                        weightsProp.DeleteArrayElementAtIndex(j);

                    EditorGUILayout.EndHorizontal();
                }

                // Add weight button
                EditorGUILayout.Space(2);
                if (GUILayout.Button("+ Add Weight", GUILayout.Width(120)))
                {
                    weightsProp.InsertArrayElementAtIndex(weightsProp.arraySize);
                }

                // Show total
                EditorGUILayout.Space(4);
                if (totalWeight <= 0)
                {
                    EditorGUILayout.HelpBox("⚠ Total weight is 0. Percentages cannot be calculated.", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.LabelField($"Total Weight: {totalWeight}", EditorStyles.helpBox);
                }

                EditorGUILayout.Space(8);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(4);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
