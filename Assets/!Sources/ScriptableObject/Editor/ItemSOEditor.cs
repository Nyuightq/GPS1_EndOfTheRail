#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemSO))]
public class ItemSOEditor : Editor
{
    SerializedProperty effectsProp;

    private void OnEnable()
    {
        effectsProp = serializedObject.FindProperty("effects");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        ItemSO item = (ItemSO)target;

        // ----------------- Basic Fields -----------------
        item.itemName = EditorGUILayout.TextField("Item Name", item.itemName);
        item.useItemDescription = EditorGUILayout.Toggle("Use Description", item.useItemDescription);

        if (item.useItemDescription)
        {
            EditorGUILayout.LabelField("Item Description", EditorStyles.boldLabel);
            item.itemDescription = EditorGUILayout.TextArea(item.itemDescription, GUILayout.Height(60));
        }
        item.itemSprite = (Sprite)EditorGUILayout.ObjectField("Item Sprite", item.itemSprite, typeof(Sprite), false);
        item.mandatoryItem = EditorGUILayout.Toggle("Mandatory Item", item.mandatoryItem);
        item.itemWidth = Mathf.Max(1, EditorGUILayout.IntField("Item Width", item.itemWidth));
        item.itemHeight = Mathf.Max(1, EditorGUILayout.IntField("Item Height", item.itemHeight));

        // ----------------- Item Shape Grid -----------------
        if (item.itemWidth > 0 && item.itemHeight > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Item Shape Grid", EditorStyles.boldLabel);

            for (int y = 0; y < item.itemHeight; y++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int x = 0; x < item.itemWidth; x++)
                {
                    var cell = item.GetCell(x, y);
                    if (cell != null)
                        cell.filled = EditorGUILayout.Toggle(cell.filled, GUILayout.Width(20));
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        // ----------------- Effects Section -----------------
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Effects", EditorStyles.boldLabel);

        if (item.effects == null)
            item.effects = new Effect[0];

        for (int i = 0; i < item.effects.Length; i++)
        {
            int index = i;
            Effect effect = item.effects[index];

            EditorGUILayout.BeginVertical("box");
            string typeName = effect == null ? "None" : effect.GetType().Name;
            EditorGUILayout.LabelField($"Effect {index}: {typeName}", EditorStyles.boldLabel);

            // Buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Change Type", GUILayout.Width(120)))
            {
                GenericMenu menu = new GenericMenu();
                Type[] effectTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => t.IsSubclassOf(typeof(Effect)) && !t.IsAbstract)
                    .ToArray();

                foreach (var t in effectTypes)
                {
                    Type effectType = t;
                    menu.AddItem(new GUIContent(effectType.Name), false, () =>
                    {
                        Undo.RecordObject(item, "Change Effect Type");
                        item.effects[index] = CreateInstanceWithDefaults(effectType) as Effect;
                        EditorUtility.SetDirty(item);
                    });
                }

                menu.ShowAsContext();
            }

            if (GUILayout.Button("Remove Effect", GUILayout.Width(80)))
            {
                Undo.RecordObject(item, "Remove Effect");
                List<Effect> tmp = new List<Effect>(item.effects);
                tmp.RemoveAt(index);
                item.effects = tmp.ToArray();
                EditorUtility.SetDirty(item);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                break;
            }
            EditorGUILayout.EndHorizontal();

            // Draw effect fields
            if (effect != null)
            {
                DrawFields(effect, "Effect Fields", item);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        // Add new Effect
        if (GUILayout.Button("Add Effect"))
        {
            GenericMenu menu = new GenericMenu();
            Type[] effectTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsSubclassOf(typeof(Effect)) && !t.IsAbstract)
                .ToArray();

            foreach (var t in effectTypes)
            {
                Type effectType = t;
                menu.AddItem(new GUIContent(effectType.Name), false, () =>
                {
                    Undo.RecordObject(item, "Add Effect");
                    List<Effect> temp = new List<Effect>(item.effects);
                    temp.Add(CreateInstanceWithDefaults(effectType) as Effect);
                    item.effects = temp.ToArray();
                    EditorUtility.SetDirty(item);
                });
            }

            menu.ShowAsContext();
        }

        if (GUI.changed)
            EditorUtility.SetDirty(item);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawFields(object obj, string label, UnityEngine.Object targetObj)
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        var fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => f.IsPublic || f.GetCustomAttribute<SerializeField>() != null);

        foreach (var f in fields)
        {
            object value = f.GetValue(obj);
            Type fieldType = f.FieldType;

            // Conditions[] special handling
            if (typeof(Conditions[]).IsAssignableFrom(fieldType))
            {
                EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(f.Name), EditorStyles.boldLabel);
                Conditions[] arr = value as Conditions[] ?? new Conditions[0];
                arr = DrawConditionsArray(arr, ObjectNames.NicifyVariableName(f.Name), targetObj);
                f.SetValue(obj, arr);
                continue; // skip EndHorizontal for this field
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(f.Name), GUILayout.Width(150));

            if (fieldType == typeof(int))
            {
                int v = (int)(value ?? 0);
                f.SetValue(obj, EditorGUILayout.IntField(v));
            }
            else if (fieldType == typeof(float))
            {
                float v = (float)(value ?? 0f);
                f.SetValue(obj, EditorGUILayout.FloatField(v));
            }
            else if (fieldType == typeof(bool))
            {
                bool v = (bool)(value ?? false);
                f.SetValue(obj, EditorGUILayout.Toggle(v));
            }
            else if (fieldType == typeof(string))
            {
                string v = (string)(value ?? string.Empty);
                f.SetValue(obj, EditorGUILayout.TextField(v));
            }
            else if (fieldType.IsEnum)
            {
                Enum currentEnum = (Enum)(value ?? Enum.GetValues(fieldType).GetValue(0));
                f.SetValue(obj, EditorGUILayout.EnumPopup(currentEnum));
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
            {
                f.SetValue(obj, EditorGUILayout.ObjectField((UnityEngine.Object)value, fieldType, true));
            }
            else
            {
                EditorGUILayout.LabelField($"(Unsupported: {fieldType.Name})");
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    private Conditions[] DrawConditionsArray(Conditions[] arr, string label, UnityEngine.Object targetObj)
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        for (int i = 0; i < arr.Length; i++)
        {
            int index = i;
            Conditions cond = arr[index];
            string typeName = cond == null ? "None" : cond.GetType().Name;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Condition {i}: {typeName}", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Change Type", GUILayout.Width(120)))
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
                        arr[index] = CreateInstanceWithDefaults(condType) as Conditions;
                        EditorUtility.SetDirty(targetObj);
                    });
                }

                menu.ShowAsContext();
            }

            if (GUILayout.Button("Remove", GUILayout.Width(80)))
            {
                List<Conditions> tmp = new List<Conditions>(arr);
                tmp.RemoveAt(index);
                arr = tmp.ToArray();
                EditorUtility.SetDirty(targetObj);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                break;
            }
            EditorGUILayout.EndHorizontal();

            if (cond != null)
                DrawFields(cond, "Condition Fields", targetObj);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        if (GUILayout.Button("Add Condition"))
        {
            List<Conditions> tmp = new List<Conditions>(arr);
            tmp.Add(null);
            arr = tmp.ToArray();
            EditorUtility.SetDirty(targetObj);
        }

        return arr;
    }

    private object CreateInstanceWithDefaults(Type t)
    {
        object obj = FormatterServices.GetUninitializedObject(t);

        var fields = t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => f.IsPublic || f.GetCustomAttribute<SerializeField>() != null);

        foreach (var f in fields)
        {
            Type fType = f.FieldType;
            object defaultValue = null;

            if (fType.IsValueType && !fType.IsEnum)
                defaultValue = Activator.CreateInstance(fType);
            else if (fType.IsEnum)
                defaultValue = Enum.GetValues(fType).GetValue(0);
            else if (typeof(UnityEngine.Object).IsAssignableFrom(fType))
                defaultValue = null;
            else if (fType == typeof(string))
                defaultValue = string.Empty;
            else if (fType.IsArray)
                defaultValue = Array.CreateInstance(fType.GetElementType(), 0);
            else
                defaultValue = null;

            try { f.SetValue(obj, defaultValue); }
            catch (Exception ex) { Debug.LogWarning($"Could not set default for field {f.Name} on {t.Name}: {ex.Message}"); }
        }

        return obj;
    }
}
#endif
