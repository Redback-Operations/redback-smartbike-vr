using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class BasicView : View
    {
        [SerializeField] private Button backButton;
        [SerializeField] private TMP_Text viewTitle;
    
        public string Title
        {
            get => viewTitle.text;
            set => viewTitle.text = value;
        }

        private void OnClickBack()
        {
            UIViewManager.Instance.PopView();
        }

        public override void Show(object data = null)
        {
            base.Show(data);            
            backButton.onClick.AddListener(OnClickBack);
        }

        public override void Hide()
        {
            base.Hide();
            backButton.onClick.RemoveListener(OnClickBack);
        }
        public void PushMyself()
        { 
            UIViewManager.Instance.PushView(this,null);
        }
    }
}