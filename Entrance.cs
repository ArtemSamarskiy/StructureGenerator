using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entrance : MonoBehaviour
{
    [Header("THEMES")]
    public Color Color = new Color(1, 1, 1, 1);
    [Min(0)] public float Size = 0.1f;
    
    [Header("SETTINGS TAG")]
    public string TagEntrances;
    public List<string> TagsConnectingStructure = new List<string>();
    public List<string> TagsConnectingEntrances = new List<string>();

    [HideInInspector] public bool busy;

    private void OnDrawGizmos()
    {
        Vector3 positionPoint = transform.position;
        Gizmos.color = busy ? Color.red : Color;
        Gizmos.DrawSphere(positionPoint, Size);
        Gizmos.DrawRay(positionPoint, transform.forward);
    }
}
