namespace BirdVolleyball;

public sealed class Bird
{
    public Bird(float x, float y, bool isPlayer)
    {
        Position = new PointF(x, y);
        IsPlayer = isPlayer;
    }

    public float Width => 74f;

    public float Height => 72f;

    public bool IsPlayer { get; }

    public PointF Position { get; set; }

    public PointF Velocity { get; set; }

    public bool OnGround { get; set; }

    public bool IsSmashing { get; set; }

    public float ActionCooldown { get; set; }

    public RectangleF Bounds => new(Position.X, Position.Y, Width, Height);

    public PointF Center => new(Position.X + Width / 2f, Position.Y + Height / 2f);

    public void Reset(float y, float x)
    {
        Position = new PointF(x, y);
        Velocity = PointF.Empty;
        OnGround = true;
        IsSmashing = false;
        ActionCooldown = 0;
    }
}

public sealed class Ball
{
    private readonly float radius;

    public Ball(float x, float y, float radius = 18f)
    {
        Position = new PointF(x, y);
        this.radius = radius;
    }

    public PointF Position { get; set; }

    public PointF PreviousPosition { get; set; }

    public PointF Velocity { get; set; }

    public float Radius => radius;

    public PointF Center => Position;

    public RectangleF Bounds => new(Position.X - radius, Position.Y - radius, radius * 2, radius * 2);

    public void Reset(float x, float y)
    {
        Position = new PointF(x, y);
        PreviousPosition = Position;
        Velocity = PointF.Empty;
    }

    public float DistanceTo(PointF point)
    {
        var dx = Position.X - point.X;
        var dy = Position.Y - point.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }
}
