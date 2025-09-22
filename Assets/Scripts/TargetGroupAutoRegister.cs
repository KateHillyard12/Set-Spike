using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

[DefaultExecutionOrder(100)]
public class TargetGroupAutoRegister : MonoBehaviour
{
    [Header("Lookup")]
    public string targetGroupTag = "CamTargetGroup";

    [Header("Weights & Radius")]
    public float weight = 1f;
    public float radius = 1f;

    CinemachineTargetGroup group;
    bool added;

    void OnEnable() { StartCoroutine(RegisterNextFrame()); }

    IEnumerator RegisterNextFrame()
    {
        yield return null; // let spawn/teleport finish
        var go = GameObject.FindGameObjectWithTag(targetGroupTag);
        if (!go) { Debug.LogWarning($"[TGAR] No '{targetGroupTag}' found."); yield break; }

        group = go.GetComponent<CinemachineTargetGroup>();
        if (!group) { Debug.LogWarning("[TGAR] TargetGroup missing on tagged object."); yield break; }

        group.AddMember(transform, weight, radius);
        added = true;
    }

    void OnDisable()
    {
        if (group && added) { group.RemoveMember(transform); added = false; }
    }
}
