using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay.Missions
{
    [CreateAssetMenu(fileName = "Mission Data", menuName = "MissionData")]
    public class MissionData : ScriptableObject
    {
        public int missionNumber = 0;
        public string targetScene = "CityScene";
        public string missionName = "Mission Name";
        [TextArea(5,25)]
        public string missionDescription;
        
    }
}