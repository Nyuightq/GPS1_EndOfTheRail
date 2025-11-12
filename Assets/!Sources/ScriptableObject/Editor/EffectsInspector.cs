using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

[CustomEditor(typeof(TEsting))]
public class EffectsInspector : Editor
{
    SerializedProperty effectsProp;

    private void OnEnable()
    {
        effectsProp = serializedObject.FindProperty("effects");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        TEsting myTarget = (TEsting)target;

        EditorGUILayout.LabelField("Effects", EditorStyles.boldLabel);

        if (myTarget.effects == null)
            myTarget.effects = new Effect[0];

        // Draw each Effect
        for (int i = 0; i < myTarget.effects.Length; i++)
        {
            int index = i; // capture index
            Effect effect = myTarget.effects[index];

            EditorGUILayout.BeginVertical("box");
            string typeName = effect == null ? "None" : effect.GetType().Name;
            EditorGUILayout.LabelField("Element " + index + ": " + typeName);

            // Type selection button
            if (GUILayout.Button("Select Type"))
            {
                GenericMenu menu = new GenericMenu();

                Type[] effectTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => t.IsSubclassOf(typeof(Effect)) && !t.IsAbstract)
                    .ToArray();

                foreach (var t in effectTypes)
                {
                    Type effectType = t; // capture type
                    menu.AddItem(new GUIContent(effectType.Name), false, () =>
                    {
                        myTarget.effects[index] = (Effect)Activator.CreateInstance(effectType);
                        EditorUtility.SetDirty(myTarget);
                    });
                }

                menu.ShowAsContext();
            }

            // Draw serialized fields of the Effect
            if (effect != null)
            {
                SerializedObject effectSO = new SerializedObject(myTarget);
                SerializedProperty prop = effectsProp.GetArrayElementAtIndex(index);
                DrawFields(effect, "Effect Fields");
            }

            // Remove button
            if (GUILayout.Button("Remove Effect"))
            {
                List<Effect> temp = new List<Effect>(myTarget.effects);
                temp.RemoveAt(index);
                myTarget.effects = temp.ToArray();
                EditorUtility.SetDirty(myTarget);
                break; // avoid iteration issues
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        // Add new slot
        if (GUILayout.Button("Add Effect"))
        {
            List<Effect> temp = new List<Effect>(myTarget.effects);
            temp.Add(null);
            myTarget.effects = temp.ToArray();
            EditorUtility.SetDirty(myTarget);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawFields(object obj, string label)
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        var fields = obj.GetType()
                        .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .Where(f => f.IsPublic || f.GetCustomAttribute<SerializeField>() != null);

        foreach (var f in fields)
        {
            object value = f.GetValue(obj);

            if (f.FieldType == typeof(int))
            {
                f.SetValue(obj, EditorGUILayout.IntField(f.Name, (int)value));
            }
            else if (f.FieldType == typeof(float))
            {
                f.SetValue(obj, EditorGUILayout.FloatField(f.Name, (float)value));
            }
            else if (f.FieldType == typeof(bool))
            {
                f.SetValue(obj, EditorGUILayout.Toggle(f.Name, (bool)value));
            }
            else if (typeof(Conditions[]).IsAssignableFrom(f.FieldType))
            {
                // Draw nested Conditions array
                Conditions[] arr = value as Conditions[];
                if (arr == null) arr = new Conditions[0];
                arr = DrawConditionsArray(arr, f.Name);
                f.SetValue(obj, arr);
            }
            else
            {
                EditorGUILayout.LabelField(f.Name + ": (type not supported in inspector)");
            }
        }
    }

    private Conditions[] DrawConditionsArray(Conditions[] arr, string label)
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        for (int i = 0; i < arr.Length; i++)
        {
            int index = i;
            Conditions cond = arr[index];
            string typeName = cond == null ? "None" : cond.GetType().Name;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Element " + i + ": " + typeName);

            if (GUILayout.Button("Select Condition Type"))
            {
                GenericMenu menu = new GenericMenu();
                Type[] condTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => t.IsSubclassOf(typeof(Conditions)) && !t.IsAbstract)
                    .ToArray();

                foreach (var t in condTypes)
                {
                    Type condType = t;
                    menu.AddItem(new GUIContent(condType.Name), false, () =>
                    {
                        arr[index] = (Conditions)Activator.CreateInstance(condType);
                        EditorUtility.SetDirty(target);
                    });
                }

                menu.ShowAsContext();
            }

            // Draw fields of the Condition
            if (cond != null)
                DrawFields(cond, "Condition Fields");

            if (GUILayout.Button("Remove Condition"))
            {
                List<Conditions> temp = new List<Conditions>(arr);
                temp.RemoveAt(index);
                arr = temp.ToArray();
                EditorUtility.SetDirty(target);
                break;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        if (GUILayout.Button("Add Condition"))
        {
            List<Conditions> temp = new List<Conditions>(arr);
            temp.Add(null);
            arr = temp.ToArray();
            EditorUtility.SetDirty(target);
        }

        return arr;
    }
}
