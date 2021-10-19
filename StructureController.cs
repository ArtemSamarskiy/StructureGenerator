using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;


public class StructureController : MonoBehaviour
{
    [Header("SETTINGS")]
    public int CountRooms;
    [SerializeField] private List<RulesClass> _rules = new List<RulesClass>();

    [Header("LINKS")]
    [SerializeField] private List<Structure> _structures = new List<Structure>();
    
    private List<Structure> _createdStructures = new List<Structure>();
    [HideInInspector] public EventInt OnGeneration;
    [HideInInspector] public UnityEvent OnDone;
    [HideInInspector] public UnityEvent OnAwakeDone;

    [ContextMenu("Destroy All Structure")]
    private void DestroyAllStructure()
    {
        foreach (var structure in _createdStructures)
            if(structure) DestroyImmediate(structure.gameObject);
        foreach (var rule in _rules)
            rule.Count = 0;
        _createdStructures.Clear();
        StopAllCoroutines();
    }

    [ContextMenu("Start generator")]
    private void StartContextMenu() => StartGeneration();
    
    public void StartGeneration(int count = default)
    {
        DestroyAllStructure();
        if(count != default)
            CountRooms = count;
        _createdStructures.Add(Instantiate(RandomStructure(_structures, default, true), transform.position, Quaternion.identity));
        StartCoroutine(GeneratorStructure());
    }

    private Structure RandomStructure(List<Structure> structures, Entrance entrance = default, bool firstStructure = false)
    {
        while (true)
        {
            Structure structure = structures[Random.Range(0, structures.Count)];
            if(entrance == default || entrance.TagsConnectingStructure.Count <= 0 || entrance.TagsConnectingStructure.Contains(structure.ID))
            {
                if ((!structure.NotTheFirst && firstStructure) || !firstStructure)
                {
                    bool returnStructure = true;
                    if(entrance != default)
                        foreach (var rule in structure.StructureRulesList)
                        {
                            List<Collider> target_colliders = Physics.OverlapSphere(entrance.transform.position, rule.Radius).ToList();
                            foreach (var ignore_collider in structure.IgnoreColliders)
                                target_colliders.Remove(ignore_collider);
                            int countStructures = 0;
                            foreach (var targetCollider in target_colliders)
                                if (targetCollider.TryGetComponent(out Structure target_structure))
                                    if (target_structure.ID == rule.TagStructure)
                                        countStructures++;
                            Debug.Log($"Count: {countStructures} | Rule count: {rule.Count} | Tag: {rule.TagStructure}");
                            if (countStructures != rule.Count)
                            {
                                returnStructure = false;
                                break;
                            }
                        }
                    if(returnStructure)
                        return structure;
                }
            }
        }
    }

    private IEnumerator RandomEntrance(Structure structure, Action<Entrance, int> Entrances, Entrance target = default)
    {
        List<Entrance> Entrancess = structure.Entrances;
        List<int> variants_viewed = new List<int>();
        while (variants_viewed.Count < Entrancess.Count)
        {
            int index = Random.Range(0, Entrancess.Count); // Получить рандомный индекс выхода
            if(!variants_viewed.Contains(index)) variants_viewed.Add(index); // Добавить индекс в лист - если его там нет
            Entrance entrance = Entrancess[index]; // Получить выход

            bool isConnecting = true;
            
            if(target != default)
                if (entrance.TagsConnectingEntrances.Count > 0 && !string.IsNullOrEmpty(target.TagEntrances))
                    isConnecting = entrance.TagsConnectingEntrances.Contains(target.TagEntrances);
            
            if (entrance.busy == false && isConnecting) { Entrances(entrance, index); yield break; } // Если нашли не закрытый выход
            yield return null;
        }
    }

    public void SetPositionStructureAtConnectionPoint(Structure structure, Entrance current, Entrance target)
    {
        InverseLoockAt(structure.transform, current.transform, target.transform);
        Vector3 StructurePositionOffset = current.transform.position - structure.transform.position;
        structure.transform.position = target.transform.position - StructurePositionOffset;
    }
    
    private void InverseLoockAt(Transform structure, Transform from, Transform to) =>
        structure.RotateAround(from.position, Vector3.up, Mathf.DeltaAngle(from.eulerAngles.y, to.eulerAngles.y) + 180);

    private bool TryRule(string id_target, out RulesClass rule_out)
    {
        foreach (var rule in _rules)
            if (rule.ID == id_target)
            { rule_out = rule; return true; }
        rule_out = null;
        return false;
    }
    
    private IEnumerator GeneratorStructure()
    {
        int countStructure = 0;

        while (countStructure < CountRooms)
        {
            Structure targetStructure = RandomStructure(_createdStructures);
            Entrance targetEntrance = null;
            
            Debug.Log($"Search entrance in target: {targetStructure.name}");
            yield return StartCoroutine(RandomEntrance(targetStructure, delegate(Entrance data, int index) { targetEntrance = data; }));
            
            Structure spawnStructure;
            RulesClass rule_spawn = null;
            
            while (true)
            {
                spawnStructure = RandomStructure(_structures, targetEntrance);
                if (TryRule(spawnStructure.ID, out RulesClass rule))
                {
                    rule_spawn = rule;
                    if(rule_spawn.Count < rule_spawn.MaxCount)
                        break;
                }
                else
                    break;
                yield return null;
            }
            
            Entrance spawnEntrance = null;
            int EntranceSpawnIndex = 0;
            
            Debug.Log($"Search entrance in spawn: {spawnStructure.name}");
            yield return StartCoroutine(RandomEntrance(spawnStructure, delegate(Entrance data, int index) { spawnEntrance = data; EntranceSpawnIndex = index; }, targetEntrance));

            Debug.Log("GENERATING...");
            
            if(targetEntrance != null && spawnEntrance != null)
            {
                Structure createdStructure = Instantiate(spawnStructure, targetEntrance.transform.position, Quaternion.identity);
                SetPositionStructureAtConnectionPoint(createdStructure, createdStructure.Entrances[EntranceSpawnIndex], targetEntrance);
                List<Collider> colliders = Physics.OverlapBox(createdStructure.PositionCollider(), createdStructure.CheckCollider.bounds.extents, createdStructure.transform.rotation).ToList();

                foreach (var ignoreCollider in targetStructure.IgnoreColliders)
                    colliders.Remove(ignoreCollider);
                foreach (var ignoreCollider in createdStructure.IgnoreColliders)
                    colliders.Remove(ignoreCollider);
                
                if(colliders.Count == 0)
                {
                    countStructure++;
                    if (rule_spawn != null)
                        rule_spawn.Count++;
                    targetEntrance.busy = true;
                    createdStructure.name = $"Structured: {countStructure}";
                    createdStructure.Entrances[EntranceSpawnIndex].busy = true;
                    _createdStructures.Add(createdStructure);
                    OnGeneration.Invoke(countStructure);
                    if(countStructure == CountRooms-5)
                        OnAwakeDone.Invoke();
                }
                else
                    DestroyImmediate(createdStructure.gameObject);
            }
            else
                Debug.LogWarning($"targetEntrance: {targetEntrance} or spawnEntrance: {spawnEntrance}");
            
            Debug.Log($"Count structure: {countStructure}");
            yield return null;
        }
        OnDone.Invoke();
        Debug.Log("STOP GENERATING");
    }
}

[Serializable]
public class EventInt : UnityEvent<int> {}

[Serializable]
public class RulesClass
{
    public string ID;
    [Min(0)] public int MaxCount;

    [HideInInspector] public int Count;
}
