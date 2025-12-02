using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BaseInventorySO))]
public class BaseInventorySOEditor : Editor
{
    private BaseInventorySO inventorySO;

    private void OnEnable()
    {
        inventorySO = (BaseInventorySO)target;
    }

    public override void OnInspectorGUI()
    {
        // Update the serialized object
        serializedObject.Update();

        // Draw default inspector for width/height
        DrawDefaultInspector();

        if (inventorySO.inventoryShape == null || inventorySO.width <= 0 || inventorySO.height <= 0)
        {
            serializedObject.ApplyModifiedProperties();
            return;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Inventory Shape", EditorStyles.boldLabel);

        // Draw grid directly using serializedProperty for proper saving
        for (int y = 0; y < inventorySO.height; y++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < inventorySO.width; x++)
            {
                int index = y * inventorySO.width + x;
                if (index >= inventorySO.inventoryShape.Length)
                    continue;

                // Ensure the cell exists
                if (inventorySO.inventoryShape[index] == null)
                    inventorySO.inventoryShape[index] = new BaseInventoryCell();

                // Register undo
                Undo.RecordObject(inventorySO, "Toggle Inventory Cell");

                // Toggle the filled state
                inventorySO.inventoryShape[index].filled = GUILayout.Toggle(
                    inventorySO.inventoryShape[index].filled, "", GUILayout.Width(20));
            }
            EditorGUILayout.EndHorizontal();
        }

        // Mark as dirty so Unity saves the changes
        if (GUI.changed)
        {
            EditorUtility.SetDirty(inventorySO);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
