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
        private BasicView colorPickerScreen;

        [SerializeField] private int columnCount = 2;
        [SerializeField] private RectOffset padding;
        [SerializeField] private float spacing;

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
            
            GameObject[] columns = new GameObject[columnCount];
            for (int i = 0; i < columnCount; i++)
            {
                var column = new GameObject($"column_{i}");
                column.transform.parent = parent;
                column.transform.localScale= Vector3.one;
                column.transform.localPosition = Vector3.zero;
                columns[i] = column;
                var verticalLayoutGroup = column.AddComponent<VerticalLayoutGroup>();
                verticalLayoutGroup.padding = padding;
                verticalLayoutGroup.spacing = spacing;
            }

            for (var index = 0; index < bike.PartDatas.Length; index++)
            {
                var bikePartData = bike.PartDatas[index];
                var partButton = Instantiate(partButtonPrefab, columns[index % columnCount].transform);
                if (!partButton.GetComponent<BikePartColorPick>())
                {
                    partButton.AddComponent<BikePartColorPick>();
                }
                var bikePartInteraction = partButton.GetComponent<BikePartColorPick>();
                bikePartInteraction.partName = bikePartData.name;
                bikePartInteraction.GetComponentInChildren<TMP_Text>().text = bikePartData.name;
                partButton.name = bikePartData.name;
                bikePartInteraction.bike = bike;
                
                partButton.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    colorPickerScreen.PushMyself();
                    colorPicker.onColorChange.RemoveAllListeners();
                    colorPicker.onColorChange.AddListener(bikePartInteraction.OnColorChanged);
                });
            }
        }
    }
}