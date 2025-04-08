using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class BikePartColorPickerManager : MonoBehaviour
    {
        [SerializeField] private RectTransform parent;
        [SerializeField] private GameObject partButtonPrefab;

        [SerializeField] private SaveLoadBike saveLoadBike;

        [FormerlySerializedAs("colorPicker")] [SerializeField]
        private GameObject colorPickerScreen;


        private void Start()
        {
            saveLoadBike.onBikeSelected += Populate;
            Populate(saveLoadBike.CurrentBike);
        }

        private void OnDestroy()
        {
            saveLoadBike.onBikeSelected -= Populate;
        }

        private void Populate(Bike bike)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Destroy(parent.GetChild(i).gameObject);
            }

            FlexibleColorPicker colorPicker = colorPickerScreen.GetComponentInChildren<FlexibleColorPicker>();

            foreach (var bikePartData in bike.PartDatas)
            {
                var partButton = Instantiate(partButtonPrefab, parent);
                var bikePartInteraction = partButton.GetComponentInChildren<BikePartColorPick>();
                bikePartInteraction.partName = bikePartData.name;
                bikePartInteraction.GetComponentInChildren<TMP_Text>().text = bikePartData.name;
                partButton.name = bikePartData.name;
                bikePartInteraction.bike = bike;

                partButton.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    colorPicker.onColorChange.RemoveAllListeners();
                    colorPicker.onColorChange.AddListener(bikePartInteraction.OnColorChanged);
                    colorPickerScreen.SetActive(true);
                    gameObject.SetActive(false);
                });
            }
        }
    }
}