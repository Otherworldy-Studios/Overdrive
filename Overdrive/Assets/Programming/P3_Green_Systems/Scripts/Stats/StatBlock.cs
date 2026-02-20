using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Stat Block", menuName = "Stats/Stat Block")]
public class StatBlock : ScriptableObject
{
    [SerializeField] private List<Stat> stats;
    
    public List<Stat> GetStatBlock()
    {
        return stats;
    }

    public static List<StatRuntime> InitializeBlock(List<Stat> stats)
    {
        List<StatRuntime> runtimeStats = new();

        foreach (Stat stat in stats)
        {
            StatRuntime runtimeStat = new StatRuntime(stat);

            if (runtimeStats.Exists(rs => rs.GetStatID() == stat.getStatID()))
            {
                Debug.LogWarning("Duplicate Stat Found: " + runtimeStat.GetStatID());
            }
            else
            {
                runtimeStats.Add(runtimeStat);
            }
        }
        
        return runtimeStats;
    }
}
