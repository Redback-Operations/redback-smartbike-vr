using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LiquidStream : MonoBehaviour
{
    private LineRenderer _lineRenderer = null;
    private Vector3 _targetPosition = Vector3.zero;

    private Coroutine _coroutine;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    private void Start()
    {
        MoveToPosition(0, transform.position);
        MoveToPosition(1, transform.position);
    }

    public void Begin()
    {
        _coroutine = StartCoroutine(BeginPour());
    }

    private IEnumerator BeginPour()
    {
        while (gameObject.activeSelf)
        {
            _targetPosition = FindEndPoint();

            MoveToPosition(0, transform.position);
            AnimateToPosition(1, _targetPosition);

            yield return null;
        }
    }

    public void End()
    {
        StopCoroutine(_coroutine);
        _coroutine = StartCoroutine(EndPour());
    }

    private IEnumerator EndPour()
    {
        while (!HasReachedPosition(0, _targetPosition))
        {
            AnimateToPosition(0, _targetPosition);
            AnimateToPosition(1, _targetPosition);

            yield return null;
        }

        Destroy(gameObject);
    }

    private Vector3 FindEndPoint()
    {
        var ray = new Ray(transform.position, Vector3.down);

        Physics.Raycast(ray, out RaycastHit hit, 2.0f);
        Vector3 endPoint = hit.collider ? hit.point : ray.GetPoint(2.0f);

        return endPoint;
    }

    private void MoveToPosition(int index, Vector3 targetPosition)
    {
        _lineRenderer.SetPosition(index, targetPosition);
    }

    private void AnimateToPosition(int index, Vector3 targetPosition)
    {
        Vector3 currentPosition = _lineRenderer.GetPosition(index);
        Vector3 newPosition = Vector3.MoveTowards(currentPosition, targetPosition, Time.deltaTime * 1.75f);

        _lineRenderer.SetPosition(index, newPosition);
    }

    private bool HasReachedPosition(int index, Vector3 targetPosition)
    {
        Vector3 currentPosition = _lineRenderer.GetPosition(index);
        return currentPosition == targetPosition;
    }
}
