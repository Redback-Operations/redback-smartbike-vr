using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventoryData
{
    public List<string> itemIds = new List<string>();
    public List<int> quantities = new List<int>();
    public InventoryData()
    {
        itemIds = new List<string>();
        quantities = new List<int>();
    }
    
    public InventoryData(Dictionary<string, int> items)
    {
        itemIds = new List<string>(items.Keys);
        quantities = new List<int>(items.Values);
    }
    public Dictionary<string, int> ToDictionary()
    {
        Dictionary<string, int> items = new Dictionary<string, int>();
        for (int i = 0; i < itemIds.Count && i < quantities.Count; i++)
        {
            items[itemIds[i]] = quantities[i];
        }
        return items;
    }
}