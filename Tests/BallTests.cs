using NUnit.Framework;
using BirdVolleyball.Models;  // ← Исправлено!
using System.Drawing;

namespace BirdVolleyball.Tests;

[TestFixture]
public class BallTests
{
    // Проверка создания мяча с правильными свойствами
    [Test]
    public void Ball_Creation_SetsCorrectProperties()
    {
        var ball = new Ball(150, 250);

        Assert.That(ball.Position.X, Is.EqualTo(150));
        Assert.That(ball.Position.Y, Is.EqualTo(250));
        Assert.That(ball.Radius, Is.EqualTo(GameSettings.BallRadius));
    }

    // Проверка пользовательского радиуса
    [Test]
    public void Ball_CustomRadius_SetsCorrectly()
    {
        var ball = new Ball(0, 0, radius: 25f);

        Assert.That(ball.Radius, Is.EqualTo(25f));
    }

    // Проверка расчёта границ мяча
    [Test]
    public void Ball_Bounds_CalculatedFromRadius()
    {
        var ball = new Ball(100, 100, radius: 20f);

        var bounds = ball.Bounds;

        Assert.That(bounds.X, Is.EqualTo(80));
        Assert.That(bounds.Y, Is.EqualTo(80));
        Assert.That(bounds.Width, Is.EqualTo(40));
        Assert.That(bounds.Height, Is.EqualTo(40));
    }

    // Проверка: центр мяча равен позиции
    [Test]
    public void Ball_Center_EqualsPosition()
    {
        var ball = new Ball(300, 400);

        Assert.That(ball.Center, Is.EqualTo(ball.Position));
    }

    // Проверка сброса мяча
    [Test]
    public void Ball_Reset_ClearsVelocityAndPreviousPosition()
    {
        var ball = new Ball(100, 100);
        ball.Velocity = new PointF(50, -30);
        ball.Position = new PointF(200, 200);
        ball.PreviousPosition = new PointF(150, 150);

        ball.Reset(50, 50);

        Assert.That(ball.Position.X, Is.EqualTo(50));
        Assert.That(ball.Position.Y, Is.EqualTo(50));
        Assert.That(ball.PreviousPosition, Is.EqualTo(new PointF(50, 50)));
        Assert.That(ball.Velocity, Is.EqualTo(PointF.Empty));
    }

    // Проверка расчёта расстояния до точки
    [Test]
    public void Ball_DistanceTo_CalculatesEuclideanDistance()
    {
        var ball = new Ball(0, 0);
        var point = new PointF(3, 4);

        var distance = ball.DistanceTo(point);

        Assert.That(distance, Is.EqualTo(5).Within(0.001f));
    }

    // Проверка: расстояние до той же точки равно 0
    [Test]
    public void Ball_DistanceTo_SamePoint_ReturnsZero()
    {
        var ball = new Ball(100, 100);

        var distance = ball.DistanceTo(new PointF(100, 100));

        Assert.That(distance, Is.EqualTo(0).Within(0.001f));
    }
}