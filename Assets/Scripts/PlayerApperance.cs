using UnityEngine;

public class PlayerAppearance : MonoBehaviour
{
    [Tooltip("Renderers to tint. If empty, will auto-grab all child renderers on first Apply().")]
    public Renderer[] tintRenderers;

    [Tooltip("Optional: where to spawn a model prefab (if you use different meshes per player).")]
    public Transform modelAnchor;

    private GameObject spawnedModel;

    public void Apply(Material mat)
    {
        if ((tintRenderers == null || tintRenderers.Length == 0))
            tintRenderers = GetComponentsInChildren<Renderer>(true);

        if (!mat || tintRenderers == null) return;

        foreach (var r in tintRenderers)
        {
            var mats = r.sharedMaterials;

            // Only replace the BODY material (slot 1)
            // Make sure this matches your mesh material order
            if (mats.Length > 1)
                mats[1] = mat;

            r.sharedMaterials = mats;
        }
    }

   //overload
    public void Apply(Material mat, GameObject modelPrefab)
    {
        if (modelPrefab && modelAnchor)
        {
            if (spawnedModel) Destroy(spawnedModel);
            spawnedModel = Instantiate(modelPrefab, modelAnchor);
            spawnedModel.transform.localPosition = Vector3.zero;
            spawnedModel.transform.localRotation = Quaternion.identity;
            spawnedModel.transform.localScale = Vector3.one;
        }
        Apply(mat);
    }
}
