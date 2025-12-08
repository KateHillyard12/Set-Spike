using UnityEngine;
using System.Collections;

/// <summary>
/// Seagull Hit State - Reaction when struck by the ball.
/// Plays VFX, disables visuals, and transitions to dead state.
/// </summary>
public class SeagullHitState : ISeagullState
{
    private ParticleSystem hitVFX;
    private float destroyTimer;

    public SeagullHitState(ParticleSystem vfxPrefab, Vector3 hitPosition, Collision collision)
    {
        // Spawn VFX at impact point
        if (vfxPrefab != null)
        {
            Vector3 spawnPos = hitPosition;
            if (collision != null && collision.contactCount > 0)
                spawnPos = collision.GetContact(0).point;
            else
                spawnPos += Camera.main.transform.forward * -2.0f;

            hitVFX = Object.Instantiate(vfxPrefab, spawnPos, Quaternion.identity);
            hitVFX.Play();

            var main = hitVFX.main;
            destroyTimer = main.duration + main.startLifetime.constantMax;
        }
    }

    public void OnEnter(SeagullController controller)
    {
        // Play sound
        GameAudio.Instance?.PlaySeagullHit();
        
        // Disable collision and visuals
        controller.DisableVisuals();
    }

    public void OnUpdate(SeagullController controller)
    {
        // Count down destruction timer
        destroyTimer -= Time.deltaTime;
        if (destroyTimer <= 0f)
            controller.ChangeState(new SeagullDeadState());
    }

    public void OnExit(SeagullController controller)
    {
        // Clean up VFX if still alive
        if (hitVFX != null)
            Object.Destroy(hitVFX.gameObject);
    }
}
