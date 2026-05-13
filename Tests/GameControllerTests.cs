using NUnit.Framework;
using BirdVolleyball.Controllers;
using BirdVolleyball.Models;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BirdVolleyball.Tests;

[TestFixture]
public class GameControllerTests
{
    private GameState _state;
    private GameController _controller;
    private ControlBindings _bindings;

    // Подготовка перед каждым тестом
    [SetUp]
    public void SetUp()
    {
        _state = new GameState();
        var arena = new RectangleF(0, 0, 1280, 720);
        _state.UpdateArenaBounds(arena);
        _state.ResetPositions();
        _controller = new GameController(_state);
        _bindings = ControlBindings.Load("Settings/Controls.json");
    }

    // Проверка: вне игры состояние не меняется
    [Test]
    public void Update_WhenNotPlaying_DoesNotModifyState()
    {
        _state.ScreenState = ScreenState.MainMenu;
        var initialScore = _state.PlayerScore;

        _controller.Update(0.016f, new HashSet<Keys>(), _bindings);

        Assert.That(_state.PlayerScore, Is.EqualTo(initialScore));
    }

    // Проверка движения игрока
    [Test]
    public void Update_PlayerMovement_AffectsBirdVelocity()
    {
        _state.ScreenState = ScreenState.Playing;
        var keys = new HashSet<Keys> { Keys.D };  // Вправо

        _controller.Update(0.016f, keys, _bindings);

        Assert.That(_state.Player.Velocity.X, Is.GreaterThan(0));
    }

    // Проверка прыжка игрока на земле
    [Test]
    public void Update_PlayerJump_WhenOnGround_SetsVerticalVelocity()
    {
        _state.ScreenState = ScreenState.Playing;
        _state.Player.OnGround = true;
        var keys = new HashSet<Keys> { Keys.ShiftKey };

        _controller.Update(0.016f, keys, _bindings);

        Assert.That(_state.Player.Velocity.Y, Is.LessThan(0));  // Вверх
        Assert.That(_state.Player.OnGround, Is.False);
    }

    // Проверка: нельзя прыгнуть в воздухе
    [Test]
    public void Update_PlayerCannotJump_WhenInAir()
    {
        _state.ScreenState = ScreenState.Playing;
        _state.Player.OnGround = false;
        var initialYVelocity = _state.Player.Velocity.Y;
        var keys = new HashSet<Keys> { Keys.ShiftKey };

        _controller.Update(0.016f, keys, _bindings);

        Assert.That(_state.Player.Velocity.Y, Is.EqualTo(initialYVelocity));
    }

    // Проверка уменьшения таймера обратного отсчёта
    [Test]
    public void Update_Countdown_DecreasesOverTime()
    {
        _state.ScreenState = ScreenState.Playing;
        _state.CountdownActive = true;
        _state.Countdown = 2.5f;

        _controller.Update(0.5f, new HashSet<Keys>(), _bindings);

        Assert.That(_state.Countdown, Is.EqualTo(2.0f).Within(0.01f));
    }

    // Проверка завершения обратного отсчёта и запуска подачи
    [Test]
    public void Update_CountdownFinished_DeactivatesAndLaunchesServe()
    {
        _state.ScreenState = ScreenState.Playing;
        _state.CountdownActive = true;
        _state.Countdown = 0.01f;
        var initialBallVelocity = _state.Ball.Velocity;

        _controller.Update(0.02f, new HashSet<Keys>(), _bindings);

        Assert.That(_state.CountdownActive, Is.False);
        Assert.That(_state.Ball.Velocity, Is.Not.EqualTo(initialBallVelocity));
    }

    // Проверка гравитации мяча
    [Test]
    public void Update_BallGravity_AffectsVerticalVelocity()
    {
        _state.ScreenState = ScreenState.Playing;
        _state.CountdownActive = false;
        var initialVelocityY = _state.Ball.Velocity.Y;

        _controller.Update(0.016f, new HashSet<Keys>(), _bindings);

        Assert.That(_state.Ball.Velocity.Y, Is.GreaterThan(initialVelocityY));
    }

    // Проверка движения бота к мячу
    [Test]
    public void Update_BotAI_MovesTowardBall_WhenBallComing()
    {
        _state.ScreenState = ScreenState.Playing;
        _state.GameMode = GameMode.SinglePlayer;
        _state.Ball.Position = new PointF(_state.NetX + 200, 300);
        _state.Ball.Velocity = new PointF(100, 0);
        var initialBotX = _state.Bot.Center.X;

        _controller.Update(0.016f, new HashSet<Keys>(), _bindings);

        Assert.That(_state.Bot.Velocity.X, Is.GreaterThan(0));
    }

    // Проверка логики смэша
    [Test]
    public void Update_SmashLogic_SetsIsSmashing_WhenConditionsMet()
    {
        _state.ScreenState = ScreenState.Playing;
        _state.Player.OnGround = true;
        var keys = new HashSet<Keys> { Keys.Space };

        _controller.Update(0.016f, keys, _bindings);
        Assert.That(_state.Player.SmashTriggered, Is.True);

        _state.Player.OnGround = false;
        _state.Player.Velocity = new PointF(0, 10);
        _controller.Update(0.016f, keys, _bindings);

        Assert.That(_state.Player.IsSmashing, Is.True);
    }
}