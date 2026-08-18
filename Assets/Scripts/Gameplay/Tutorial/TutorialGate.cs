using UnityEngine;

namespace Gameplay.Tutorial
{
   
    [RequireComponent(typeof(BoxCollider))]
    public class TutorialGate : MonoBehaviour
    {
        public int Index;

        [SerializeField] private Renderer[] tintRenderers;
        [SerializeField] private Color passedColor = new Color(0.2f, 0.85f, 0.3f);

        private TutorialManager _manager;
        private bool _passed;

        public bool Passed => _passed;

        public void Init(TutorialManager manager)
        {
            _manager = manager;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_passed)
                return;

            var player = other.GetComponentInParent<PlayerController>();
            if (player == null)
                return;

            _passed = true;

            foreach (var tintRenderer in tintRenderers)
            {
                if (tintRenderer != null)
                    tintRenderer.material.color = passedColor;
            }

            if (_manager != null)
                _manager.OnGatePassed(this);
        }
    }
}
