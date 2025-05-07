using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.BikeSelection
{
    public class BikeControllerSelectUI : MonoBehaviour
    {
        [SerializeField] StringGameObjectPair[] typesAndButtonPairs;

        [Serializable]
        public class StringGameObjectPair
        {
            public string type;
            public GameObject gameObject;
        }

        private void Start()
        {
            string controllerType = PlayerPrefs.GetString("BikeControllerType", typesAndButtonPairs[0].type);
            SetType(controllerType);
            foreach (var pair in typesAndButtonPairs)
            {
                pair.gameObject.GetComponent<Button>().onClick.AddListener(() => { SetType(pair.type); });
            }
        }

        private void RefreshButtons(string type)
        {
            foreach (var pair in typesAndButtonPairs)
            {
                pair.gameObject.GetComponent<Button>().interactable = pair.type != type;
            }
        }

        private void SetType(string type)
        {
            PlayerPrefs.SetString("BikeControllerType", type);
            RefreshButtons(type);
        }
    }
}