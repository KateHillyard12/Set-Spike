using UnityEngine;


/// Interface for seagull AI states.
/// Allows addition of new behaviors without modifying SeagullController.

public interface ISeagullState
{
  
    /// Called when entering this state.
   
    void OnEnter(SeagullController controller);

    
    /// Called every frame while in this state.
  
    void OnUpdate(SeagullController controller);

    /// Called when exiting this state.
    
    void OnExit(SeagullController controller);
}
