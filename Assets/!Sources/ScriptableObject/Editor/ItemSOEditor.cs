#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

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

        // Draw basic item fields
        item.itemName = EditorGUILayout.TextField("Item Name", item.itemName);
          EditorGUILayout.LabelField("Item Description", EditorStyles.boldLabel);
        item.itemDescription = EditorGUILayout.TextArea(item.itemDescription, GUILayout.Height(60));
        item.itemSprite = (Sprite)EditorGUILayout.ObjectField("Item Sprite", item.itemSprite, typeof(Sprite), false);
        item.mandatoryItem = EditorGUILayout.Toggle("Mandatory Item", item.mandatoryItem);
        //item.itemEffectPrefab = (GameObject)EditorGUILayout.ObjectField("Item Effect", item.itemEffectPrefab, typeof(GameObject), false);
        item.itemWidth = Mathf.Max(1, EditorGUILayout.IntField("Item Width", item.itemWidth));
        item.itemHeight = Mathf.Max(1, EditorGUILayout.IntField("Item Height", item.itemHeight));

        // Draw item shape grid
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

        // --- Effects section ---
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
            EditorGUILayout.LabelField("Effect " + index + ": " + typeName);

            if (GUILayout.Button("Select Effect Type"))
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
                        item.effects[index] = (Effect)Activator.CreateInstance(effectType);
                        EditorUtility.SetDirty(item);
                    });
                }

                menu.ShowAsContext();
            }

            if (effect != null)
                DrawFields(effect, "Effect Fields", item);

            if (GUILayout.Button("Remove Effect"))
            {
                List<Effect> temp = new List<Effect>(item.effects);
                temp.RemoveAt(index);
                item.effects = temp.ToArray();
                EditorUtility.SetDirty(item);
                break;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        if (GUILayout.Button("Add Effect"))
        {
            List<Effect> temp = new List<Effect>(item.effects);
            temp.Add(null);
            item.effects = temp.ToArray();
            EditorUtility.SetDirty(item);
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

            if (f.FieldType == typeof(int))
                f.SetValue(obj, EditorGUILayout.IntField(f.Name, (int)value));
            else if (f.FieldType == typeof(float))
                f.SetValue(obj, EditorGUILayout.FloatField(f.Name, (float)value));
            else if (f.FieldType == typeof(bool))
                f.SetValue(obj, EditorGUILayout.Toggle(f.Name, (bool)value));
            else if (f.FieldType == typeof(string))
                f.SetValue(obj, EditorGUILayout.TextField(f.Name, (string)value));
            else if (f.FieldType.IsEnum)
            {
                Enum currentEnum = (Enum)value;
                Enum newEnum = EditorGUILayout.EnumPopup(f.Name, currentEnum);
                f.SetValue(obj, newEnum);
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(f.FieldType))
            {
                UnityEngine.Object newObj = EditorGUILayout.ObjectField(f.Name, (UnityEngine.Object)value, f.FieldType, true);
                f.SetValue(obj, newObj);
            }
            else if (typeof(Conditions[]).IsAssignableFrom(f.FieldType))
            {
                Conditions[] arr = value as Conditions[] ?? new Conditions[0];
                arr = DrawConditionsArray(arr, f.Name, targetObj);
                f.SetValue(obj, arr);
            }
            else
            {
                EditorGUILayout.LabelField(f.Name + ": (type not supported in inspector)");
            }
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
            EditorGUILayout.LabelField("Condition " + i + ": " + typeName);

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
                        EditorUtility.SetDirty(targetObj);
                    });
                }

                menu.ShowAsContext();
            }

            if (cond != null)
                DrawFields(cond, "Condition Fields", targetObj);

            if (GUILayout.Button("Remove Condition"))
            {
                List<Conditions> temp = new List<Conditions>(arr);
                temp.RemoveAt(index);
                arr = temp.ToArray();
                EditorUtility.SetDirty(targetObj);
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
            EditorUtility.SetDirty(targetObj);
        }

        return arr;
    }
}
#endif
