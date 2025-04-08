using UnityEngine;
using UnityEngine.Events;

namespace UI
{
    public class EventView : View
    {
        public UnityEvent onShow;
        public UnityEvent onHide;
        public override void Hide()
        {
            base.Hide();
            onHide?.Invoke();
        }

        public override void Show(object data = null)
        {
            base.Show(data);
            onShow?.Invoke();
        }
    }
}