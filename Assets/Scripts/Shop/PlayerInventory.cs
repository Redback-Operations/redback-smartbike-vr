
using System;
using System.Collections.Generic;
using UnityEngine;
public class PlayerInventory
{
    private Dictionary<string, int> items = new Dictionary<string, int>();
    private string saveKey;
    
    public PlayerInventory(string playerSaveKey = "DefaultPlayer")
    {
        saveKey = $"Inventory_{playerSaveKey}";
        LoadInventory();
    }
    
    public bool HasItem(string itemId, int quantity = 1)
    {
        return items.ContainsKey(itemId) && items[itemId] >= quantity;
    }
    
    public void AddItem(string itemId, int quantity)
    {
        if (items.ContainsKey(itemId))
            items[itemId] += quantity;
        else
            items[itemId] = quantity;
        
        SaveInventory();
    }
    
    public bool RemoveItem(string itemId, int quantity)
    {
        if (!HasItem(itemId, quantity)) return false;
        
        items[itemId] -= quantity;
        if (items[itemId] <= 0)
            items.Remove(itemId);
        
        SaveInventory();
        return true;
    }
    
    public int GetItemCount(string itemId)
    {
        return items.ContainsKey(itemId) ? items[itemId] : 0;
    }

    public void SetItemCount(string itemId,int amount)
    {
        if (items.ContainsKey(itemId))
        {
            items[itemId] = amount;
        }
    }
    
    public Dictionary<string, int> GetAllItems()
    {
        return new Dictionary<string, int>(items);
    }
    
    public void ClearInventory()
    {
        items.Clear();
        SaveInventory();
    }
    
    private void SaveInventory()
    {
        try
        {
            InventoryData data = new InventoryData(items);
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(saveKey, json);
            PlayerPrefs.Save();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save inventory: {e.Message}");
        }
    }
    
    private void LoadInventory()
    {
        try
        {
            if (PlayerPrefs.HasKey(saveKey))
            {
                string json = PlayerPrefs.GetString(saveKey);
                if (!string.IsNullOrEmpty(json))
                {
                    InventoryData data = JsonUtility.FromJson<InventoryData>(json);
                    items = data.ToDictionary();
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load inventory: {e.Message}");
            items = new Dictionary<string, int>();
        }
    }
    
    // Manual save method for critical moments
    public void ForceSave()
    {
        SaveInventory();
    }
}
