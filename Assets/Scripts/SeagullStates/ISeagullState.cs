using UnityEngine;

/// <summary>
/// Interface for seagull AI states.
/// Allows easy addition of new behaviors without modifying SeagullController.
/// </summary>
public interface ISeagullState
{
    /// <summary>
    /// Called when entering this state.
    /// </summary>
    void OnEnter(SeagullController controller);

    /// <summary>
    /// Called every frame while in this state.
    /// </summary>
    void OnUpdate(SeagullController controller);

    /// <summary>
    /// Called when exiting this state.
    /// </summary>
    void OnExit(SeagullController controller);
}
