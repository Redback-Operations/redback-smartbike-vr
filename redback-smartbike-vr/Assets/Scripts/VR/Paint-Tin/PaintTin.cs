using UnityEngine;

public class PaintTin : MonoBehaviour
{
	public GameObject Lid;

    public MeshFilter EmptyLocation;
    public GameObject StreamPrefab;

    public Color PaintColor;

    public SprayGun SprayGun;

    public Transform Location;

    private Vector3 _origin = Vector3.zero;
    private LiquidStream _currentStream = null;
    private bool _isPouring = false;

    void Update()
    {
        var pourCheck = CanPour();

        if (_isPouring != pourCheck)
        {
            _origin = CalculateOrigin();
            _isPouring = pourCheck;

            if (_isPouring)
                StartPour();
            else
                EndPour();
        }
        else
        {
            if (_isPouring)
                _origin = CalculateOrigin();
        }

        if (_currentStream != null)
            _currentStream.transform.position = _origin;

        if (_isPouring)
            FillTarget();
    }

    public void RemoveLid()
	{
		if (Lid != null)
			Lid.SetActive(false);

        if (Location != null)
            SprayGun?.Disassemble(Location);
    }

    public void ReplaceLid()
	{
		if (Lid != null)
			Lid.SetActive(true);

        SprayGun?.Assemble();
    }

    private bool CanPour()
    {
        var angle = CalculateFlowAngle();

        if (angle > 30)
        {
            return true;
        }

        return false;
    }

    private float CalculateFlowAngle()
    {
        return Vector3.Angle(Vector3.up, EmptyLocation.transform.up);
    }

    private Vector3 CalculateOrigin()
    {
        var lowestPoint = Vector3.positiveInfinity;
        Matrix4x4 localToWorld = EmptyLocation.transform.localToWorldMatrix;

        foreach (var vertex in EmptyLocation.mesh.vertices)
        {
            var worldPosition = localToWorld.MultiplyPoint3x4(vertex);

            if (lowestPoint.y > worldPosition.y)
                lowestPoint = worldPosition;
        }

        return lowestPoint;
    }

    private void StartPour()
    {
        _currentStream = CreateStream();
        _currentStream.Begin();
    }

    private void EndPour()
    {
        _currentStream.End();
        _currentStream = null;
    }

    private LiquidStream CreateStream()
    {
        var streamObject = Instantiate(StreamPrefab, _origin, Quaternion.identity, transform);
        var line = streamObject.GetComponent<LineRenderer>();

        if (line != null)
            line.material.color = PaintColor;

        return streamObject.GetComponent<LiquidStream>();
    }

    private void FillTarget()
    {
        var ray = new Ray(_origin, Vector3.down);
        var hits = Physics.RaycastAll(ray, 2.0f);

        foreach (var hit in hits)
        {
            if (hit.collider.gameObject == gameObject)
                continue;

            var fill = hit.collider.gameObject.GetComponentInParent<SprayGun>();

            if (fill == null)
                continue;

            if (hit.collider != fill.FillLocation)
                continue;

            fill.AdjustFill(PaintColor);
        }
    }
}
