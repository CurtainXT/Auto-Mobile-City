using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSMState
{
    public StateType stateType;
    public FSMSystem fsm;
    public GameObject npc;
    // 当前状态应执行的操作
    public virtual void Act() { }
    // 执行转换的判断
    public virtual void Reason() { }
    public virtual void Enter() { }
    public virtual void Exit() { }
    public FSMState(FSMSystem FSM, GameObject npc)
    {
        fsm = FSM;
        npc = npc;
    }
}
