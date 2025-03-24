using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using static UnityEngine.GraphicsBuffer;

public class MissionSelectButtonManager : MonoBehaviour
{
    public MissionSelectButton[] Missions;
    public MissionSelectButton BackButton;
    public MissionSelectButton GoButton;
    public CanvasRenderer DescriptionPanel;
    public TextMeshProUGUI DescriptionText;

    public Canvas Base;
    private MissionData _activeMissionData;

    //public MissionSelectButton MenuMain;
    public IUIState ActiveState;

    public bool doDynamicMissionAlign = true;

    private void Awake()
    {
        if (doDynamicMissionAlign) DynamicAlignMissions();

        //start with mission list
        Debug.Log("Initial state set");
        SetState(new BaseState(this));
    }


    //dynamic menu alignment instead of manual
    private void DynamicAlignMissions()
    {
        float y;

        for (int i = 0; i < Missions.Length; i++)
        {
            y = 98 - (42 * i); //+ 52 + 98; //some evil hocus pocus to get them into dynamic position
            Missions[i].gameObject.transform.localPosition = new Vector3(5.446314f, y, -0.000456969f);
        }
    }

    private void SetState(IUIState nextState)
    {
        if (nextState == null) return; 

        Debug.Log($"Switching state from {ActiveState}");
        if (ActiveState != null) ActiveState.OnExitState();

        ActiveState = nextState;
        Debug.Log($"New state: {ActiveState}");
        ActiveState.OnEnterState();
    }

    public void ButtonInteract(MissionSelectButton button)
    {
        Debug.Log($"Received button hit: {button.name}");
        //foreach (var but in Buttons)
        //{
        //    but.gameObject.SetActive(false);
        //}
        ActiveState.ButtonInteract(button);
    }

    //UI State handler
    public interface IUIState
    {
        void OnEnterState();
        void OnExitState();

        void ButtonInteract(MissionSelectButton button);
    }

    //Lists all the possible missions
    public class BaseState : IUIState
    {
        private MissionSelectButtonManager _manager;

        //constructor
        public BaseState(MissionSelectButtonManager manager)
        {
            _manager = manager;
        }

        public void OnEnterState() 
        {
            _manager.Base.gameObject.SetActive(false);
        }

        public void OnExitState() 
        {
            _manager.Base.gameObject.SetActive(true);
        }

        //we can start the mission or go back
        public void ButtonInteract(MissionSelectButton button)
        {
            MissionSelectButton.ButtonAction ButtonAction;

            ButtonAction = button.Action;

            switch (ButtonAction)
            {
                case MissionSelectButton.ButtonAction.Base:
                    {
                        _manager.SetState(new MissionListState(_manager));
                        break;
                    }
                default:
                    return;
            }
        }

    }

    public class MissionListState: IUIState
    {
        private MissionSelectButtonManager _manager;

        public MissionListState(MissionSelectButtonManager manager)
        {
            _manager = manager;
        }

        public void OnEnterState() 
        {
            foreach (var button in _manager.Missions)
            {
                button.gameObject.SetActive(true);
            }

            _manager.BackButton.gameObject.SetActive(true);
        }

        public void OnExitState() {
            foreach (var button in _manager.Missions)
            {
                button.gameObject.SetActive(false);
            }
        }

        public void ButtonInteract(MissionSelectButton button) 
        {
            MissionSelectButton.ButtonAction ButtonAction;
            
            ButtonAction = button.Action;
            switch (ButtonAction)
            {
                //when selecting mission get the mission data and select the mission, if we can
                case MissionSelectButton.ButtonAction.MissionSelect:
                    {               
                        _manager._activeMissionData = button.GetComponent<MissionData>();
                        if (_manager._activeMissionData != null)
                        {
                            _manager.SetState(new MissionDisplayState(_manager));
                        }
                        else { Debug.Log("ERror! Couldn't get mission data!"); }
                        break;
                }
                case MissionSelectButton.ButtonAction.Back:
                    {
                        _manager.SetState(new BaseState(_manager));
                        break;
                    }
                default:
                    return;
            }
        }

        //menu for when the mission has been selected
    }
    public class MissionDisplayState : IUIState
    {
        private MissionSelectButtonManager _manager;

        //constructor
        public MissionDisplayState(MissionSelectButtonManager manager)
        {
            _manager = manager;
        }

        public void OnEnterState()
        {
            //activate description panel and go/start buttons
            _manager.GoButton.gameObject.SetActive(true);
            _manager.BackButton.gameObject.SetActive(true);
            _manager.DescriptionPanel.gameObject.SetActive(true);

            //display our mission data if we have it and a valid renderer
            if (_manager._activeMissionData != null && _manager.DescriptionText != null) 
            { _manager.DescriptionText.text = _manager._activeMissionData.DescriptionText; }
        }
        public void OnExitState()
        {
            //deactivate description panel and go/start buttons
            _manager.GoButton.gameObject.SetActive(false);
            _manager.BackButton.gameObject.SetActive(false);
            _manager.DescriptionPanel.gameObject.SetActive(false);
        }

        //we can start the mission or go back
        public void ButtonInteract(MissionSelectButton button)
        {
            MissionSelectButton.ButtonAction ButtonAction;

            ButtonAction = button.Action;

            switch (ButtonAction)
            {
                //activate the mission when hitting start button
                case MissionSelectButton.ButtonAction.Start:
                    {
                        MissionData MissionData = _manager._activeMissionData;
                        if (MissionData != null)
                        {
                            PlayerPrefs.SetInt("MissionNumber", MissionData.MissionID);
                            MapLoader.LoadScene(MissionData.TargetScene);
                        }
                        else { Debug.Log("ERror! Couldn't get mission data!"); }
                        break;
                    }
                case MissionSelectButton.ButtonAction.Back:
                    {
                        _manager.SetState(new MissionListState(_manager));
                        break;
                    }
                default:
                    return;
            }
        }

    }
}

