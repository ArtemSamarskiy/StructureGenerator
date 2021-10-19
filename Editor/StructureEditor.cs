using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Structure))]
public class StructureEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Structure structure = (Structure) target;
        
        if(string.IsNullOrEmpty(structure.ID))
            EditorGUILayout.HelpBox("НЕ УКАЗАН АЙДИ СТРУКТУРЫ!!", MessageType.Error);
        
        GUI.color = new Color(1f, 0.77f, 0.38f);
        if (GUILayout.Button("Clear entrances"))
        {
            foreach (var entrance in structure.Entrances)
                if(entrance) DestroyImmediate(entrance.gameObject);
            structure.Entrances.Clear();
        }
        if (GUILayout.Button("Clear no referenc"))
            for (var index = 0; index < structure.Entrances.Count; index++)
                if (!structure.Entrances[index]) structure.Entrances.RemoveAt(index);
        GUI.color = new Color(0.53f, 1f, 0.45f);
        if (GUILayout.Button("Set entrance and colliders"))
        {
            structure.Entrances = structure.GetComponentsInChildren<Entrance>().ToList();
            structure.IgnoreColliders = structure.GetComponentsInChildren<Collider>().ToList();
        }
        if (GUILayout.Button("Add entrance"))
        {
            GameObject gameObject = new GameObject("EntrancePoint", typeof(Entrance));
            Entrance entrance = gameObject.GetComponent<Entrance>();
            entrance.Color = Random.ColorHSV();
            entrance.TagEntrances = "entrance";
            gameObject.transform.SetParent(structure.transform);
            gameObject.transform.localPosition = Vector3.zero;
            structure.Entrances.Add(entrance);
        }
        
        GUI.color = Color.white;
        GUILayout.BeginVertical("box");
        foreach (var entrance in structure.Entrances)
        {
            GUI.color = Color.white;
            GUILayout.BeginVertical("box");
            if (entrance)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Tag: ({entrance.TagEntrances}) | Name: ({entrance.name})");
                GUI.color = entrance.Color;
                GUILayout.Label("<########>");
                GUILayout.EndHorizontal();
            }
            else GUILayout.Label($"No reference!");
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();
    }
}
