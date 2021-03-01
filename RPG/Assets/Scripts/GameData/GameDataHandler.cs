using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameDataHandler : MonoBehaviour
{
    protected static int GetIntFromCell(string priceCell)
    {
        var price = 0;
        if (priceCell.IsNotEmpty())
            price = int.Parse(priceCell);
        return price;
    }

    protected static  T GetEnum<T>(T defaultT, string type) where T : struct, Enum
    {
        var t = defaultT;
        if (!Enum.TryParse(type, true, out t))
            LogManager.LogError($"Cannot parse {type} into {t}. Returning default {defaultT}.");
        return t;
    }
    
    protected static T[] GetEnumArray<T>(T defaultT, string data, bool returnEmpty = false) where T: struct, Enum
    {
        if (data.IsEmpty())
            return returnEmpty ?  new T[0] : new T[] { defaultT };
        data = data.ToLower();
        var enums = data.Split(':');
        int count = enums.Length;
        var ts = new T[count];
        for (int i = 0; i < count; i++)
        {
            var t = GetEnum(defaultT, data);
            ts[i] = t;
        }
        return ts;
    }
}

