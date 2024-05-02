public enum FingerType
{
    None,
    Thumb,
    Index,
    Middle,
    Ring,
    Pinky
}

public class Finger
{
    public FingerType Type = FingerType.None;
    public float Current = 0.0f;
    public float Target = 0.0f;

    public Finger(FingerType type)
    {
        Type = type;
    }
}
