using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkTrigger : Trigger
{
    public void OnEnter(TriggerParams triggerParams)
    {
        LogManager.LogInfo("Talking");
    }

    public void OnExit(TriggerParams triggerParams)
    {
        LogManager.LogInfo("Finished talking.");
    }

    public void OnStay(TriggerParams triggerParams)
    {
        
    }

    public void OnUse(TriggerParams triggerParams)
    {
        
    }
}