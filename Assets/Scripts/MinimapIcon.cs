using UnityEngine;

public class MinimapIcon : MonoBehaviour
{
    public Transform target;
    public RectTransform icon;

    public float mapScale = 4f;

    public Transform playerTransform;

    void Update()
    {
        if (icon == null) return;

        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        icon.anchoredPosition = Vector2.zero;

        if (target == null || icon == null || playerTransform == null) return;

        Vector3 offset = target.position - playerTransform.position;

        float visibleRadius = 10f;
        offset.x = Mathf.Clamp(offset.x, -visibleRadius, visibleRadius);
        offset.z = Mathf.Clamp(offset.z, -visibleRadius, visibleRadius);

        icon.anchoredPosition = new Vector2(offset.x * mapScale, offset.z * mapScale);
    }
}
