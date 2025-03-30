using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUIButtonManager
{

    //UI State Interface - defines generic actions of states
    void Reset();
    void SetState(IUIState nextState);
    void BackState();
    void ButtonInteract(IUIButton inButton);

    //nested base state
    interface IUIState
    {
        void OnEnterState();
        void OnExitState();
        void ButtonInteract(IUIButton inButton);
    }
}
