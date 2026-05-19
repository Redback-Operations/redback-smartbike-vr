using UnityEngine;
using UnityEngine.Events;

public class Collectable : MonoBehaviour
{
    [Header("Collectable Settings")]
    [SerializeField] private int value = 1;
    [SerializeField] private string targetTag = "Player";

    [Header("Optional Effects")]
    [SerializeField] private GameObject collectEffect;
    [SerializeField] private AudioClip collectSound;

    [Header("Animation")]
    [SerializeField] private bool rotate = true;
    [SerializeField] private float rotateSpeed = 90f;

    private bool collected = false;
    private UnityEvent<Collectable> _listener;

    public int Value => value;
    public string Tag => targetTag;

    public void Register(UnityAction<Collectable> call)
    {
        if (_listener == null)
            _listener = new UnityEvent<Collectable>();

        _listener.AddListener(call);
    }

    public void Deregister(UnityAction<Collectable> call)
    {
        _listener?.RemoveListener(call);
    }

    public int Collect()
    {
        if (collected)
            return 0;

        collected = true;

        Debug.Log($"Collected {gameObject.name} | Value: {value}");

        _listener?.Invoke(this);

        if (collectEffect != null)
            Instantiate(collectEffect, transform.position, Quaternion.identity);

        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position);

        gameObject.SetActive(false);

        return value;
    }

    private void Update()
    {
        if (!rotate)
            return;

        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
    }
}