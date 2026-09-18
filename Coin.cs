using UnityEngine;

/// <summary>
/// Toplanabilir coin objesi. Oyuncu tetiklediğinde (trigger) skor ekler.
/// Collider'ın "Is Trigger" olarak işaretlenmesi gerekir.
/// </summary>
public class Coin : MonoBehaviour
{
    public int coinValue = 1;
    public float rotateSpeed = 90f;
    public GameObject collectEffect; // opsiyonel parçacık efekti
    public AudioClip collectSound;

    private void Update()
    {
        // Basit görsel dönüş animasyonu
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        GameManager.Instance.AddCoin(coinValue);

        if (collectEffect != null)
            Instantiate(collectEffect, transform.position, Quaternion.identity);

        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position);

        Destroy(gameObject);
    }
}
