using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Oddment", menuName = "RPG/Stats/Oddment")]
public class Oddment : ScriptableObject
{
    public int oddment;
    public OddmentItem item;
}

public class OddmentItem : ScriptableObject
{

}

[CreateAssetMenu(fileName = "ItemDrop", menuName = "RPG/Stats/ItemDrop")]
public class OddmentItemDrop : OddmentItem
{
    public int id = -1;
}

[CreateAssetMenu(fileName = "EnemyEncounter", menuName = "RPG/Stats/EnemyEncounter")]
public class OddmentEnemyEncounter : OddmentItem
{
    public string backgroundPath = string.Empty;
    public List<int> enemyIds = new List<int>();
}

public class OddmentTable : MonoBehaviour
{
    private int oddmentTotal = 0;
    private Oddment empty = new Oddment();
    private List<Oddment> oddments = new List<Oddment>();

    private void Start()
    {
        oddmentTotal = CalcOddment();
    }

    public int CalcOddment()
    {
        int total = 0;
        foreach (var oddment in oddments)
            total += oddment.oddment;
        return total;
    }

    public OddmentItem Pick()
    {
        int pick = Random.Range(0, oddmentTotal);
        int total = 0;
        foreach(var oddment in oddments)
        {
            total += oddment.oddment;
            if (total >= pick)
                return oddment.item;
        }
        var last = oddments.Count - 1;
        return last < 0 ? empty.item : oddments[last].item;
    }
}