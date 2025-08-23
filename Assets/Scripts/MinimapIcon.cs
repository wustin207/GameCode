using UnityEngine;

public class MinimapIcon : MonoBehaviour
{
    #region Target & Icon
    [Header("Target & Icon")]
    public Transform target;
    public RectTransform icon;
    #endregion

    #region Map Settings
    [Header("Map Settings")]
    public float mapScale = 4f;
    #endregion

    #region Player
    [Header("Player")]
    public Transform playerTransform;
    #endregion

    void Update()
    {
        if (icon == null) return;

        if (target == null)
        {
            Destroy(gameObject);
            return;
        }
        //Reset icons to the centre to guaranty correct position
        icon.anchoredPosition = Vector2.zero;

        if (target == null || icon == null || playerTransform == null) return;
        
        //Determines the location and which direction the target are
        Vector3 offset = target.position - playerTransform.position;

        float visibleRadius = 10f;
        offset.x = Mathf.Clamp(offset.x, -visibleRadius, visibleRadius);
        offset.z = Mathf.Clamp(offset.z, -visibleRadius, visibleRadius);

        //Convert eevrything into the map space
        icon.anchoredPosition = new Vector2(offset.x * mapScale, offset.z * mapScale);
    }
}
