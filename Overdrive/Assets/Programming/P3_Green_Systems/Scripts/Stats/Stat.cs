using UnityEngine;

[CreateAssetMenu(fileName = "Stat", menuName = "Stats/Stat")]
public class Stat : ScriptableObject
{
    [SerializeField] private string statName = "New Stat";

    [SerializeField] private float baseValue = 1.0f;

    [SerializeField] private float hardCap = 10.0f;

    public string getStatName()
    {
        return statName;
    }

    public float getBaseValue()
    {
        return baseValue;
    }

    public float getHardCap()
    {
        return hardCap;
    }
}
