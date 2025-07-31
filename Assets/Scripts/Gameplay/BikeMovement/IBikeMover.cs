using UnityEngine;

namespace Gameplay.BikeMovement
{
    public interface IBikeMover
    {
        
        public float DeltaTime { get; set; }
        public float Speed { get; set; }
        public void Init(GameObject controller);
        public void HanldeInput(Vector2 direction);
    }
}