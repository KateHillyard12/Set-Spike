using UnityEngine;

/// <summary>
/// Seagull Flying State - Normal patrol/movement behavior.
/// </summary>
public class SeagullFlyingState : ISeagullState
{
    public void OnEnter(SeagullController controller)
    {
        // Enable visuals and collision
        controller.EnableVisuals();
    }

    public void OnUpdate(SeagullController controller)
    {
        // Move forward
        Vector3 moveDir = controller.GetMoveDirection();
        float speed = controller.GetSpeed();
        float dt = Time.deltaTime;
        
        controller.transform.position += moveDir * (speed * dt);

        // Bobbing animation
        float bobAmplitude = controller.bobAmplitude;
        float bobFrequency = controller.bobFrequency;
        float bobOffset = controller.GetBobOffset();

        Vector3 pos = controller.transform.position;
        float bob = Mathf.Sin((Time.time + bobOffset) * bobFrequency) * bobAmplitude;
        pos.y += bob * dt;
        controller.transform.position = pos;

        // Check if reached target (despawn)
        float targetX = controller.GetTargetX();
        if (moveDir.x > 0f && controller.transform.position.x >= targetX)
            controller.ChangeState(new SeagullDeadState());
        else if (moveDir.x < 0f && controller.transform.position.x <= targetX)
            controller.ChangeState(new SeagullDeadState());
    }

    public void OnExit(SeagullController controller)
    {
        // Nothing special needed
    }
}
