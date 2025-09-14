using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.FloodEscape
{
    public class ElevationIndicatorUI : MonoBehaviour
    {
        [SerializeField] private Slider playerSlider;
        [SerializeField] private Slider waterSlider;
        private Transform playerTf;

        private EventBinding<RisingWater.RisingWaterEvent> risingWaterEventBinding;

        private void OnEnable()
        {
            risingWaterEventBinding = new EventBinding<RisingWater.RisingWaterEvent>(HandleEvent);
            EventBus<RisingWater.RisingWaterEvent>.Register(risingWaterEventBinding);
        }


        private void HandleEvent(RisingWater.RisingWaterEvent risingWaterEvent)
        {
            if (playerTf == null)
            {
                playerTf = FindObjectOfType<PlayerController>().transform;
            }

            playerSlider.value = playerTf.transform.position.y / risingWaterEvent.TargetHeight;
            waterSlider.value = risingWaterEvent.CurrentWaterHeight / risingWaterEvent.TargetHeight;
        }

        private void OnDisable()
        {
            EventBus<RisingWater.RisingWaterEvent>.Deregister(risingWaterEventBinding);
        }
    }
}