using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BikeCustomization : MonoBehaviour
{
    public GameObject[] avatarPrefabs;
    private GameObject avatar;

    public GameObject[] Bikes;
    private GameObject _current;

    void Start()
    {
        DisplayAvatar(PlayerPrefs.GetInt("selectedAvatar", 0));
    }

    public void DisplayAvatar(int selectedAvatar)
    {
        // make sure the avatar selected is valid
        if (avatarPrefabs.Length == 0 || selectedAvatar < 0 || selectedAvatar >= avatarPrefabs.Length)
            return;

        // Spawn the avatar as a child of the bike (Player)
        GameObject prefab = avatarPrefabs[selectedAvatar];
        GameObject avatar = Instantiate(prefab, transform.position, transform.rotation);
        avatar.transform.SetParent(transform);  // Set avatar as a child of the Player (bike)

        // Adjusting the avatar's position relative to the bike
        avatar.transform.localPosition = new Vector3(-0.02f, 0.55f, 0f); // Adjust as needed
        avatar.transform.localRotation = Quaternion.identity; // Resets rotation to align with bike

        Vector3 scale = new Vector3(0.45f, 0.45f, 0.45f); // Adjust scale as needed (0.5 is 50% of the original size)
        avatar.transform.localScale = scale;

        var avatarAnimator = avatar.GetComponent<Animator>();
        var movementController = GetComponent<PlayerMovementController>();

        if (avatarAnimator != null && movementController != null)
            movementController.AssignCharacterAnimator(avatarAnimator);  // Assigning avatar's animator to the bike's controller
        else
            Debug.LogError("No animator found on avatar prefab or PlayerMovementController is missing.");
    }

    public void DisplayBike(int id)
    {
        // ensure there are bikes available to select from
        if (Bikes.Length == 0)
            return;

        // ensure the bike is available
        id = Math.Clamp(id, 0, Bikes.Length - 1);

        // loop through each bike available
        for (int index = 0; index < Bikes.Length; index++)
        {
            // update the active status based on id selected
            Bikes[index].SetActive(index == id);

            if (index == id)
                _current = Bikes[index];
        }
    }

    public void RestoreCustomization(string customization)
    {
        if (_current == null || string.IsNullOrEmpty(customization))
            return;

        var data = JsonUtility.FromJson<Customization>(customization);

        foreach (var item in data.CustomColors)
        {
            var target = FindTarget(_current, item.Name);

            if (target == null)
                continue;

            var modified = target.GetComponent<Renderer>();

            if (modified != null)
                modified.materials[item.Material].color = item.Color;
        }
    }

    private GameObject FindTarget(GameObject current, string name)
    {
        if (current.name == name)
            return current;

        foreach (Transform child in current.transform)
        {
            var target = FindTarget(child.gameObject, name);

            if (target != null)
                return target;
        }

        return null;
    }

    [Serializable]
    public class Customization
    {
        public List<CustomColor> CustomColors;
    }

    [Serializable]
    public class CustomColor
    {
        public string Name;
        public int Material;
        public Color Color;
    }
}
