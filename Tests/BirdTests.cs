using NUnit.Framework;
using BirdVolleyball.Models;
using System.Drawing;

namespace BirdVolleyball.Tests;

[TestFixture]
public class BirdTests
{
    // Проверка создания птицы с правильными свойствами
    [Test]
    public void Bird_Creation_SetsCorrectProperties()
    {
        var bird = new Bird(100, 200, isPlayer: true);

        Assert.That(bird.Position.X, Is.EqualTo(100));
        Assert.That(bird.Position.Y, Is.EqualTo(200));
        Assert.That(bird.IsPlayer, Is.True);
        Assert.That(bird.OnGround, Is.False);
    }

    // Проверка размеров птицы из настроек
    [Test]
    public void Bird_WidthHeight_ReturnsGameSettings()
    {
        var bird = new Bird(0, 0, isPlayer: false);

        Assert.That(bird.Width, Is.EqualTo(GameSettings.BirdWidth));
        Assert.That(bird.Height, Is.EqualTo(GameSettings.BirdHeight));
    }

    // Проверка расчёта границ птицы
    [Test]
    public void Bird_Bounds_CalculatedCorrectly()
    {
        var bird = new Bird(50, 100, isPlayer: true);

        var bounds = bird.Bounds;

        Assert.That(bounds.X, Is.EqualTo(50));
        Assert.That(bounds.Y, Is.EqualTo(100));
        Assert.That(bounds.Width, Is.EqualTo(GameSettings.BirdWidth));
        Assert.That(bounds.Height, Is.EqualTo(GameSettings.BirdHeight));
    }

    // Проверка расчёта центра птицы
    [Test]
    public void Bird_Center_CalculatedCorrectly()
    {
        var bird = new Bird(100, 200, isPlayer: true);

        var center = bird.Center;

        Assert.That(center.X, Is.EqualTo(100 + GameSettings.BirdWidth / 2));
        Assert.That(center.Y, Is.EqualTo(200 + GameSettings.BirdHeight / 2));
    }

    // Проверка сброса птицы в начальное состояние
    [Test]
    public void Bird_Reset_ResetsAllProperties()
    {
        var bird = new Bird(100, 200, isPlayer: true);
        bird.Velocity = new PointF(10, -5);
        bird.OnGround = false;
        bird.IsSmashing = true;
        bird.SmashTriggered = true;
        bird.ActionCooldown = 2.5f;

        bird.Reset(300, 400);

        Assert.That(bird.Position.X, Is.EqualTo(400));
        Assert.That(bird.Position.Y, Is.EqualTo(300));
        Assert.That(bird.Velocity, Is.EqualTo(PointF.Empty));
        Assert.That(bird.OnGround, Is.True);
        Assert.That(bird.IsSmashing, Is.False);
        Assert.That(bird.SmashTriggered, Is.False);
        Assert.That(bird.ActionCooldown, Is.EqualTo(0));
    }

    // Проверка: IsPlayer доступен только для чтения
    [Test]
    public void Bird_IsPlayer_ReadOnly_AfterCreation()
    {
        var bird = new Bird(0, 0, isPlayer: true);

        Assert.That(bird.IsPlayer, Is.True);
    }
}