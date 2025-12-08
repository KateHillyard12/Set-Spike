using UnityEngine;


/// Seagull Dead State - Final state before destruction.
/// Cleans up and notifies spawner.

public class SeagullDeadState : ISeagullState
{
    public void OnEnter(SeagullController controller)
    {
        // Notify spawner and destroy
        controller.NotifyAndDestroy();
    }

    public void OnUpdate(SeagullController controller)
    {
        // Do nothing - just waiting for destruction
    }

    public void OnExit(SeagullController controller)
    {
        // Nothing needed
    }
}
