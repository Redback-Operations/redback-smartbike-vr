// using Gameplay.Missions;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;
//
// namespace UI.MissionSelection
// {
//     public class MissionView : BasicView
//     {
//         [SerializeField] private TMP_Text missionDescription;
//         [SerializeField] private Button startButton;
//
//         public override void Show(object data = null)
//         {
//             base.Show(data);
//             if (data is MissionData missionData)
//             {
//                 Title = missionData.missionName;
//                 missionDescription.text = missionData.missionDescription;
//                 startButton.onClick.RemoveAllListeners();
//                 startButton.onClick.AddListener(() =>
//                 {
//                     PlayerPrefs.SetInt("MissionNumber", missionData.missionNumber);
//                     MapLoader.LoadScene(missionData.targetScene);
//                 });
//             }
//         }
//     }
// }