using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SprayGun : MonoBehaviour
{
    public Color SprayColor;
    private Color _previous;

    public float FillTime;

    public Collider FillLocation;

    public GameObject Body;
    public GameObject Liquid;
    public GameObject Glass;

    public ParticleSystem Spray;
    public Collider SprayCollider;

    public Renderer LiquidRenderer;

    public static SprayGun Instance;

    void Start()
    {
        if (Instance != null)
            Destroy(gameObject);

        Instance = this;

        StopSpraying();
        SetColor(SprayColor);
    }

    void Update()
    {
        if (_previous != SprayColor)
            SetColor(SprayColor);

        // check if upside down and reset color
        var angle = Vector3.Angle(Vector3.up, transform.up);

        if (angle > 170)
            SprayColor = Color.black;
    }

    public void SetColor(Color color)
    {
        if (Spray != null)
        {
            var main = Spray.main;
            main.startColor = new ParticleSystem.MinMaxGradient(color);

            var renderer = Spray.GetComponent<Renderer>();

            if (renderer != null)
                renderer.material.color = color;
        }

        if (LiquidRenderer != null)
        {
            LiquidRenderer.material.SetColor("_SurfaceColor", color);
            LiquidRenderer.material.SetColor("_LiquidColor", color);
        }

        _previous = color;
    }

    public void Assemble()
    {
        Body.transform.localPosition = new Vector3(0, 0, 0);
        Body.transform.localEulerAngles = new Vector3(0, 180, 0);

        Liquid.transform.SetParent(Body.transform, false);
        Glass.transform.SetParent(Body.transform, false);
        FillLocation.transform.SetParent(Body.transform, true);
    }

    public void Disassemble(Transform target)
    {
        FillLocation.transform.SetParent(Liquid.transform, true);
        Glass.transform.SetParent(target, false);
        Liquid.transform.SetParent(target, false);

        Body.transform.localPosition = new Vector3(0.169f, 0.049f, 0.095f);
        Body.transform.localEulerAngles = new Vector3(7.5f, 180, -84.5f);
    }

    public void AdjustFill(Color color)
    {
        var fillTime = FillTime;

        if (fillTime <= 0)
            fillTime = 5;

        var r = Mathf.Clamp(SprayColor.r + color.r * Time.deltaTime / fillTime, 0, 1);
        var g = Mathf.Clamp(SprayColor.g + color.g * Time.deltaTime / fillTime, 0, 1);
        var b = Mathf.Clamp(SprayColor.b + color.b * Time.deltaTime / fillTime, 0, 1);

        SprayColor = new Color(r, g, b);
    }

    public void StartSpraying()
    {
        SprayCollider.enabled = true;
    }

    public void StopSpraying()
    {
        SprayCollider.enabled = false;
    }
}
