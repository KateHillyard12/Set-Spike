// using UnityEngine;

// public class SnapshotController : MonoBehaviour
// {
//     public AudioMixer mixer;
//     public AudioMixerSnapshot calmSnapshot;
//     public AudioMixerSnapshot combatSnapshot;

//     public void GoToCombat()
//     {
//         combatSnapshot.TransitionTo(1f); // 1 second fade
//     }

//     public void GoToMain()
//     {
//         calmSnapshot.TransitionTo(1f);
//     }


//     public void GoToDialogue(float blend = 1f)
// {
//     AudioMixerSnapshot[] snaps = { calmSnapshot, dialogueSnapshot };
//     float[] weights = { 1f - blend, blend };

//     mixer.TransitionToSnapshots(snaps, weights, 0.5f);
// }
// }
