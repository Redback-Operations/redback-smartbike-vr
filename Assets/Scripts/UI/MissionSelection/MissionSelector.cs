// using System.Collections;
// using System.Collections.Generic;
// using Gameplay.Missions;
// using TMPro;
// using UI;
// using UI.MissionSelection;
// using UnityEngine;
// using UnityEngine.Events;
// using UnityEngine.Serialization;
// using UnityEngine.UI;
//
// public class MissionSelector : BasicView
// {
//     [SerializeField] private MissionData[] missionDatas;
//     [SerializeField] private Transform contentParent;
//     [SerializeField] private GameObject buttonPrefab;
//     [SerializeField] private MissionView missionView;
//     void Start()
//     {
//         foreach (var missionData in missionDatas)
//         {
//             var missionButton = Instantiate(buttonPrefab, contentParent);
//             missionButton.GetComponentInChildren<TMP_Text>().text = missionData.missionName;
//             missionButton.GetComponent<Button>().onClick.AddListener(() =>
//             {
//                 UIViewManager.Instance.PushView(missionView, missionData);
//             });
//         }
//     }
// }
