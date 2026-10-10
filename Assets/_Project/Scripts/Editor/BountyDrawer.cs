using UnityEditor;
using UnityEngine;

/// <summary>SerializeReference 보상 목록에서 골드/아이템의 실제 타입을 선택합니다.</summary>
[CustomPropertyDrawer(typeof(Bounty), true)]
public class BountyDrawer : PropertyDrawer
{
    private static readonly string[] Types = { "선택하세요", "Gold", "Item" };

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;
        if (property.managedReferenceValue == null) return height;
        SerializedProperty child = property.Copy();
        SerializedProperty end = property.GetEndProperty();
        if (child.NextVisible(true))
            do
            {
                if (SerializedProperty.EqualContents(child, end)) break;
                height += EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(child, true);
            } while (child.NextVisible(false));
        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        Rect row = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        int current = property.managedReferenceValue is GoldBounty ? 1 :
            property.managedReferenceValue is ItemBounty ? 2 : 0;
        EditorGUI.BeginChangeCheck();
        int selected = EditorGUI.Popup(row, label.text, current, Types);
        if (EditorGUI.EndChangeCheck())
        {
            property.managedReferenceValue = selected == 1 ? (Bounty)new GoldBounty() :
                selected == 2 ? new ItemBounty() : null;
            EditorGUI.EndProperty();
            return;
        }

        if (property.managedReferenceValue != null)
        {
            EditorGUI.indentLevel++;
            SerializedProperty child = property.Copy();
            SerializedProperty end = property.GetEndProperty();
            if (child.NextVisible(true))
                do
                {
                    if (SerializedProperty.EqualContents(child, end)) break;
                    row.y += row.height + EditorGUIUtility.standardVerticalSpacing;
                    row.height = EditorGUI.GetPropertyHeight(child, true);
                    EditorGUI.PropertyField(row, child, true);
                } while (child.NextVisible(false));
            EditorGUI.indentLevel--;
        }
        EditorGUI.EndProperty();
    }
}
