using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class UIViewManager : MonoBehaviour
    {
        public static UIViewManager Instance { get; private set; }
        private readonly Stack<View> _viewStack = new();

        [SerializeField] private View initialView;

        [Header("Animation")]
        [SerializeField] private AnimationCurve scaleAnimationCurve;
        [SerializeField] private float duration;

        
        [Header("Debug")]
        public View _currentView;

        public View[] stack;
        
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }
        private void Start()
        {
            if(initialView)
                PushView(initialView);
        }

        private void Update()
        {
            _currentView = CurrentView;
            stack = _viewStack.ToArray();
        }

        public void PushView(View view, object data = null)
        {
            if(CurrentView==view) return;
            if (_viewStack.Count > 0)
            {
                _viewStack.Peek().Hide();
            }
            view.Show(data);
            StartCoroutine(AnimateView(view));
            _viewStack.Push(view);
        }

        public void PopView()
        {
            if (_viewStack.Count == 0) return;
            Debug.Log(_viewStack.Count);
            var top = _viewStack.Pop();
            top.Hide();

            if (_viewStack.Count > 0)
            {
                _viewStack.Peek().Show();
            }
        }

        private IEnumerator AnimateView(View view)
        {
            Vector3 startScale = view.transform.localScale;
            view.transform.localScale = Vector3.zero;
            for (float t = 0; t < duration; t+=Time.deltaTime)
            {
                float progress = t / duration;
                view.transform.localScale = startScale * scaleAnimationCurve.Evaluate(progress);
                yield return null;
            }
            view.transform.localScale = startScale;
        }

        public void ClearViews()
        {
            while (_viewStack.Count>0)
            {
                _viewStack.Pop().Hide();
            }
        }
        public View CurrentView => _viewStack.Count > 0 ? _viewStack.Peek() : null;
    }
}