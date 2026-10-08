using System;
using UnityEngine;

/// <summary>
/// Objeto recolectable: gira sobre sí mismo y, cuando el jugador lo
/// toca, avisa por el evento estático Collected y se destruye.
/// No conoce al ScoreManager; así el prefab se puede usar en
/// cualquier escena sin tener que asignarle referencias.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Collectible : MonoBehaviour
{
    private const string PlayerTag = "Player";

    /// <summary>Se dispara al recolectar, con el recolectable que lo generó.</summary>
    public static event Action<Collectible> Collected;

    [Tooltip("Puntos que suma al recolectarlo. La variante Gold lo sobreescribe.")]
    [SerializeField] private int points = 1;

    [SerializeField] private float rotationSpeed = 90f;

    private bool isCollected;

    public int Points => points;

    private void Reset()
    {
        // Al agregar el componente, el collider queda como trigger
        // para que el jugador lo atraviese en vez de chocarlo.
        GetComponent<Collider>().isTrigger = true;
    }

    private void Update()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        // isCollected evita sumar dos veces si el jugador tiene más de
        // un collider o el trigger se dispara de nuevo en el mismo frame.
        if (isCollected || !other.CompareTag(PlayerTag)) return;

        isCollected = true;
        Collected?.Invoke(this);
        Destroy(gameObject);
    }
}
