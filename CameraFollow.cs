using UnityEngine;

/// <summary>
/// Kamerayı oyuncunun arkasından, hafif gecikmeli (smooth) şekilde takip ettirir.
/// Subway Surfers tarzı sabit açılı 3. şahıs kamera.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0, 4f, -6f);
    public float smoothSpeed = 8f;
    public bool lookAtPlayer = true;

    private void LateUpdate()
    {
        if (player == null) return;

        // Sadece Z ekseninde takip et (X ekseni oyuncunun lane değişiminden etkilenmesin
        // veya hafif takip etsin, tasarım tercihine göre ayarlanabilir)
        Vector3 targetPosition = new Vector3(
            player.position.x * 0.3f, // hafif yatay takip, tam kilitlemek istemezseniz 0 yapın
            player.position.y + offset.y,
            player.position.z + offset.z
        );

        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

        if (lookAtPlayer)
        {
            Vector3 lookTarget = player.position + Vector3.up * 1.5f;
            transform.LookAt(lookTarget);
        }
    }
}
