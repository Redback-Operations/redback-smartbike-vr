using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Outline))]
public class OutlineEditor : Editor {
    
  
    private Outline.ColorType _colorType;
    
    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(serializedObject.FindProperty("outlineMode"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("colorType"));
        _colorType = (Outline.ColorType)serializedObject.FindProperty("colorType").enumValueIndex;
        EditorGUILayout.Space();
        switch (_colorType)
        {
            case Outline.ColorType.SingleColor:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("outlineColor"));
                break;
            case Outline.ColorType.Gradient:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("gradient"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("colorAnimationCurve"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("animationSpeed"));
                break;
            default:
                break;
        }
        
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("outlineWidth"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("precomputeOutline"));
        serializedObject.ApplyModifiedProperties();
    }
    
}