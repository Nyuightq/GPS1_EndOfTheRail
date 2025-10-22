// --------------------------------------------------------------
// Creation Date: 2025-10-20 02:07
// Author: User
// Description: -
// --------------------------------------------------------------
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemSO))]
public class ItemSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ItemSO item = (ItemSO)target;

        // Draw default fields for width/height
        item.itemName = EditorGUILayout.TextField("Item Name", item.itemName);
        item.itemSprite = (Sprite)EditorGUILayout.ObjectField("Item Sprite", item.itemSprite, typeof(Sprite), false);
        item.itemWidth = Mathf.Max(1, EditorGUILayout.IntField("Item Width", item.itemWidth));
        item.itemHeight = Mathf.Max(1, EditorGUILayout.IntField("Item Height", item.itemHeight));

        // Resize button
        if (GUILayout.Button("Resize Shape"))
        {
            item.resizeShape();
        }

        // Draw grid if valid size
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
                    {
                        cell.filled = EditorGUILayout.Toggle(cell.filled, GUILayout.Width(20));
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(item);
        }
    }
}
#endif
