using System;
using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

namespace UI.MissionSelection
{
    public class UILevelSelector : MonoBehaviour
    {
        [SerializeField] private SceneToPreview[] scenePreviews;
        [SerializeField] private ScrollRect scrollView;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button prevButton;
        [SerializeField] private GameObject levelPreviewPrefab;
        [SerializeField] private Sprite noPreviewSprite;
        private int _currentLevelIndex;
        private MissionData _selectedMissionData;

        [Serializable]
        public class SceneToPreview
        {
            [ScenePath] public string scene;
            public Sprite scenePreview;
            public string name;
        }

        public string SelectedLevel => _selectedMissionData.TargetScenes[_currentLevelIndex];

        private void Start()
        {
            nextButton.onClick.AddListener(() =>
            {
                _currentLevelIndex =
                    Mathf.Clamp(_currentLevelIndex + 1, 0, _selectedMissionData.TargetScenes.Length - 1);
                if (_currentLevelIndex == _selectedMissionData.TargetScenes.Length - 1)
                {
                    nextButton.gameObject.SetActive(false);
                }

                prevButton.gameObject.SetActive(true);
                scrollView.content.anchoredPosition +=
                    Vector2.left * scrollView.content.sizeDelta.x / scrollView.content.childCount;
            });
            prevButton.onClick.AddListener(() =>
            {
                _currentLevelIndex =
                    Mathf.Clamp(_currentLevelIndex - 1, 0, _selectedMissionData.TargetScenes.Length - 1);
                if (_currentLevelIndex == 0)
                {
                    prevButton.gameObject.SetActive(false);
                }

                nextButton.gameObject.SetActive(true);
                scrollView.content.anchoredPosition +=
                    Vector2.right * scrollView.content.sizeDelta.x / scrollView.content.childCount;
            });
        }

        public void Setup(MissionData missionData)
        {
            _selectedMissionData = missionData;

            prevButton.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(_selectedMissionData.TargetScenes.Length > 1);

            for (int i = scrollView.content.childCount - 1; i >= 0; i--)
            {
                Destroy(scrollView.content.GetChild(i).gameObject);
            }


            foreach (var scenePath in _selectedMissionData.TargetScenes)
            {
                var missionPreview = Instantiate(levelPreviewPrefab, scrollView.content);
                var scenePreviewData = scenePreviews.FirstOrDefault(e => e.scene == scenePath);
                missionPreview.GetComponent<Image>().sprite = scenePreviewData.scenePreview ?? noPreviewSprite;
                missionPreview.GetComponentInChildren<TMP_Text>().SetText(scenePreviewData.name ?? "no preview");
            }
        }
    }
}