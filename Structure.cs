using System;
using System.Collections.Generic;
using UnityEngine;

public class Structure : MonoBehaviour
{
    [Header("SETTINGS")] 
    public string ID;
    public bool NotTheFirst;
    
    [Header("RULES")] 
    public List<StructureRules> StructureRulesList = new List<StructureRules>();
    
    [Header("LINKS")]
    public Collider CheckCollider;
    
    [HideInInspector] public List<Collider> IgnoreColliders = new List<Collider>();
    [HideInInspector] public List<Entrance> Entrances = new List<Entrance>();

    public Vector3 PositionCollider(Collider target_collider = default)
    {
        Vector3 position = transform.position;
        if (target_collider == default) target_collider = CheckCollider;
        return position - (position - target_collider.bounds.center);
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        if(CheckCollider)
            Gizmos.DrawCube(PositionCollider(CheckCollider), CheckCollider.bounds.size);
        Gizmos.color = new Color(1, 0, 0, 0.25f);
        foreach (var target_collider in IgnoreColliders)
            if(target_collider && target_collider != CheckCollider)
                Gizmos.DrawCube(PositionCollider(target_collider), target_collider.bounds.size);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        foreach (var rules in StructureRulesList)
            Gizmos.DrawWireSphere(transform.position, rules.Radius);
    }
}

[Serializable]
public class StructureRules
{
    public string TagStructure;
    public float Radius;
    public int Count;
}