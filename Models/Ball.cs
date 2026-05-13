namespace BirdVolleyball.Models;

public sealed class Ball
{
    private readonly float radius;

    public Ball(float x, float y, float radius = GameSettings.BallRadius)
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

    // Сброс мяча в позицию без скорости
    public void Reset(float x, float y)
    {
        Position = new PointF(x, y);
        PreviousPosition = Position;
        Velocity = PointF.Empty;
    }

    // Расчёт расстояния до точки
    public float DistanceTo(PointF point)
    {
        var dx = Position.X - point.X;
        var dy = Position.Y - point.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }
}