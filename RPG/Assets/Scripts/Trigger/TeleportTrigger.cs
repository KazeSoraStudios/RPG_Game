using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportTrigger : Trigger
{
    public void OnEnter(TriggerParams triggerParams)
    {
        LogManager.LogInfo("teleporting");
    }

    public void OnExit(TriggerParams triggerParams)
    {
        LogManager.LogInfo("Finished teleporting.");
    }

    public void OnStay(TriggerParams triggerParams)
    {
        throw new System.NotImplementedException();
    }

    public void OnUse(TriggerParams triggerParams)
    {
        throw new System.NotImplementedException();
    }
}