using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUIButtonManager
{

    //UI State Interface - defines generic actions of states
    void SetState(IUIState nextState);

    //nested base state
    interface IUIState
    {
        void OnEnterState();
        void OnExitState();
        void ButtonInteract(IUIButton inButton);
    }
}
