using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Redback.UI
{
    /// <summary>
    /// Manages the navigation between different dashboard screens based on the Figma design.
    /// </summary>
    public class DashboardController : MonoBehaviour
    {
        [Header("Dashboard Panels")]
        [Tooltip("The panel that shows the overview of the trip (Map, Route, Elevation).")]
        public GameObject tripDetailsPanel;
        
        [Tooltip("The panel that shows the ride settings (Assistance Level, Mode, Duration).")]
        public GameObject startRideSettingsPanel;
        
        [Tooltip("The panel that shows the bike selection (E-Bike, Mountain Bike, etc).")]
        public GameObject startRideBikeSelectPanel;
        
        [Tooltip("The panel that shows the user statistics (Total Users, Active Users, Charts).")]
        public GameObject userStatisticsPanel;
        
        [Tooltip("The panel that shows the map dashboard (Popular Routes, Distances).")]
        public GameObject mapDashboardPanel;

        private void Start()
        {
            // Initialize the UI by showing the Trip Details Panel by default
            ShowTripDetailsPanel();
        }

        /// <summary>
        /// Hides all panels in the dashboard.
        /// </summary>
        private void HideAllPanels()
        {
            if (tripDetailsPanel != null) tripDetailsPanel.SetActive(false);
            if (startRideSettingsPanel != null) startRideSettingsPanel.SetActive(false);
            if (startRideBikeSelectPanel != null) startRideBikeSelectPanel.SetActive(false);
            if (userStatisticsPanel != null) userStatisticsPanel.SetActive(false);
            if (mapDashboardPanel != null) mapDashboardPanel.SetActive(false);
        }

        // --- PUBLIC METHODS (To be linked to Unity UI Buttons) ---

        public void ShowTripDetailsPanel()
        {
            HideAllPanels();
            if (tripDetailsPanel != null) tripDetailsPanel.SetActive(true);
        }

        public void ShowStartRideBikeSelectPanel()
        {
            HideAllPanels();
            if (startRideBikeSelectPanel != null) startRideBikeSelectPanel.SetActive(true);
        }

        public void ShowStartRideSettingsPanel()
        {
            HideAllPanels();
            if (startRideSettingsPanel != null) startRideSettingsPanel.SetActive(true);
        }

        public void ShowUserStatisticsPanel()
        {
            HideAllPanels();
            if (userStatisticsPanel != null) userStatisticsPanel.SetActive(true);
        }

        public void ShowMapDashboardPanel()
        {
            HideAllPanels();
            if (mapDashboardPanel != null) mapDashboardPanel.SetActive(true);
        }
    }
}
