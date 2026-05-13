namespace BirdVolleyball.Models;

public sealed class Bird
{
    public Bird(float x, float y, bool isPlayer)
    {
        Position = new PointF(x, y);
        IsPlayer = isPlayer;
    }

    public float Width => GameSettings.BirdWidth;
    public float Height => GameSettings.BirdHeight;
    public bool IsPlayer { get; }
    public PointF Position { get; set; }
    public PointF Velocity { get; set; }
    public bool OnGround { get; set; }
    public bool IsSmashing { get; set; }
    public bool SmashTriggered { get; set; }
    public float ActionCooldown { get; set; }

    public RectangleF Bounds => new(Position.X, Position.Y, Width, Height);
    public PointF Center => new(Position.X + Width / 2f, Position.Y + Height / 2f);

    // Сброс птицы в начальное состояние
    public void Reset(float y, float x)
    {
        Position = new PointF(x, y);
        Velocity = PointF.Empty;
        OnGround = true;
        IsSmashing = false;
        SmashTriggered = false;
        ActionCooldown = 0;
    }
}