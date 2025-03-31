using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using static IUIButtonManager;
using static UnityEngine.GraphicsBuffer;

public class MissionSelectButtonManager : MonoBehaviour, IUIButtonManager
{ 
    public MissionSelectButton[] Missions;
    public MissionSelectButton BackButton;
    public MissionSelectButton GoButton;
    public CanvasRenderer DescriptionPanel;
    public TextMeshProUGUI DescriptionText;

    public GameObject menuBase;
    private MissionData _activeMissionData;
    private IUIState _activeState;

    public bool doDynamicMissionAlign = true;
    private Stack<IUIState> _stateStack;

    private void Awake()
    {
        //activate base-state
        Debug.Log("Initial state set");
        Reset();
    }


    //dynamic menu alignment instead of manual (needs work)
    private void _DynamicAlignMissions()
    {
        float y;

        for (int i = 0; i < Missions.Length; i++)
        {
            y = 98 - (42 * i); //+ 52 + 98; //some evil hocus pocus to get them into dynamic position
            Missions[i].gameObject.transform.localPosition = new Vector3(5.446314f, y, -0.000456969f);
        }
    }

    //clears the state stack and returns to base
    public void Reset()
    {
        _stateStack = new Stack<IUIState>();
        SetState(new BaseState(this));
    }

    //returns to previous state on the stack
    public void BackState()
    {
        IUIState prevState;

        //don't go back if we're on base state
        if (_stateStack.Count <= 1)
        {
            Debug.Log("Attempting to goto previous state from Base state!");
            return;
        }

        //remove current state and get the state from before
        _stateStack.Pop();
        prevState = _stateStack.Pop();

        //assign previous state as current
        SetState(prevState);
    }

    //change the current active state to the given new state
    public void SetState(IUIState nextState)
    {
        //make sure next UIState is a mission state
        if (nextState == null) { Debug.Log("ERROR: Null State!"); return; }

        //shut down the active state
        if (_activeState != null) _activeState.OnExitState();

        //set new state, activate it, add to state stack
        Debug.Log($"Changing State!");
        _stateStack.Push(nextState);
        _activeState = nextState;
        _activeState.OnEnterState();
    }

    //when receiving button press, pass it on to the active-state
    public void ButtonInteract(IUIButton inbutton)
    {
        if (_activeState == null)
        { Debug.Log("ERROR: NO ACTIVE STATE"); return; }

        _activeState.ButtonInteract(inbutton);
    }

    //======================================================================================================================================
    // NESTED CLASSES
    //======================================================================================================================================

    //extended interface of base nested interface - only use for mission-specific state functionality IF NEEDED
    interface IMissionState : IUIState { };

    //initial menu - mission select not yet active
    public class BaseState : IMissionState
    {
        private MissionSelectButtonManager _manager;
        public BaseState(MissionSelectButtonManager manager) { _manager = manager; }

        public void OnEnterState()  { _manager.menuBase.gameObject.SetActive(false); }
        public void OnExitState() { _manager.menuBase.gameObject.SetActive(true); }

        public void ButtonInteract(IUIButton inButton) {
            MissionSelectButton.ButtonAction ButtonAction;
            MissionSelectButton button;
            
            button = (MissionSelectButton)inButton;
            if (button == null) return;

            ButtonAction = button.Action;

            switch (ButtonAction) {
                case MissionSelectButton.ButtonAction.Base:
                {_manager.SetState(new MissionListState(_manager)); break;}

                default: return;
            }
        }

    }

    //mission list display menu
    public class MissionListState: IMissionState
    {
        private MissionSelectButtonManager _manager;
        public MissionListState(MissionSelectButtonManager manager) { _manager = manager; }

        //enable mission buttons
        public void OnEnterState() {
            foreach (var button in _manager.Missions)
            { button.gameObject.SetActive(true); }

            _manager.BackButton.gameObject.SetActive(true);

            //trigger dynamic button position allocation if it's enabled
            if (_manager.doDynamicMissionAlign) _manager._DynamicAlignMissions();
        }

        //disable mission buttons
        public void OnExitState() {
            foreach (var button in _manager.Missions) 
            { button.gameObject.SetActive(false); }
        }

        public void ButtonInteract(IUIButton inButton) {
            MissionSelectButton.ButtonAction ButtonAction; 
            MissionSelectButton button;

            button = (MissionSelectButton)inButton;
            if (button == null) return;

            ButtonAction = button.Action;
            switch (ButtonAction) {
                case MissionSelectButton.ButtonAction.MissionSelect: 
                {               
                    _manager._activeMissionData = button.GetComponent<MissionData>();

                    //get the mission data ready if we have it
                    if (_manager._activeMissionData != null)
                    { _manager.SetState(new MissionDisplayState(_manager)); }

                    else { Debug.Log("Error! Couldn't get mission data!"); }
                    break;
                }

                //return to base state
                case MissionSelectButton.ButtonAction.Back:
                { _manager.BackState(); break; }

                default: return;
            }
        }

        //menu for when the mission has been selected
    }

    //individual mission details menu
    public class MissionDisplayState : IMissionState
    {
        private MissionSelectButtonManager _manager;
        public MissionDisplayState(MissionSelectButtonManager manager) { _manager = manager; }

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
        public void ButtonInteract(IUIButton inButton) {
            MissionSelectButton.ButtonAction ButtonAction; 
            MissionSelectButton button;

            button = (MissionSelectButton)inButton;
            if (button == null) return;

            ButtonAction = button.Action;
            switch (ButtonAction)
            {
                //activate the selected mission if possible
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

                //return to mission select list
                case MissionSelectButton.ButtonAction.Back:
                {
                    _manager.BackState();
                    break;
                }
                default: return;
            }
        }

    }
}

