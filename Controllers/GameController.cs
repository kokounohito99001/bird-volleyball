using BirdVolleyball.Models;

namespace BirdVolleyball.Controllers;

public sealed class GameController
{
    private readonly GameState gameState;
    private readonly Random random = new();

    public GameController(GameState gameState)
    {
        this.gameState = gameState;
    }

    // Главный метод обновления игры
    public void Update(float deltaTime, HashSet<Keys> pressedKeys, ControlBindings controls)
    {
        if (gameState.ScreenState != ScreenState.Playing || gameState.GameFinished)
            return;

        UpdatePlayer(deltaTime, pressedKeys, controls);

        if (gameState.GameMode == GameMode.SinglePlayer)
            UpdateBot(deltaTime);
        else
            UpdateFriendPlayer(deltaTime, pressedKeys);

        UpdateBirdPhysics(gameState.Player, deltaTime, gameState.ArenaBounds.Left, gameState.NetX - GameSettings.NetWidth / 2f);
        UpdateBirdPhysics(gameState.Bot, deltaTime, gameState.NetX + GameSettings.NetWidth / 2f, gameState.ArenaBounds.Right);

        if (gameState.CountdownActive)
            UpdateCountdown(deltaTime);
        else
            UpdateBall(deltaTime);
    }

    // Обновление управления игроком
    private void UpdatePlayer(float deltaTime, HashSet<Keys> pressedKeys, ControlBindings controls)
    {
        var move = 0f;
        if (pressedKeys.Contains(Keys.A)) move -= 1f;
        if (pressedKeys.Contains(Keys.D)) move += 1f;

        gameState.Player.Velocity = new PointF(move * GameSettings.BirdMoveSpeed, gameState.Player.Velocity.Y);

        if (pressedKeys.Contains(Keys.ShiftKey))
            TryJump(gameState.Player);

        if (pressedKeys.Contains(Keys.Space))
        {
            if (!gameState.Player.SmashTriggered && gameState.Player.OnGround)
            {
                TryJump(gameState.Player);
                gameState.Player.SmashTriggered = true;
            }
            else if (gameState.Player.SmashTriggered && !gameState.Player.OnGround && gameState.Player.Velocity.Y >= 0)
            {
                gameState.Player.IsSmashing = true;
            }
        }
        else
        {
            gameState.Player.SmashTriggered = false;
            gameState.Player.IsSmashing = false;
        }

        if (controls.IsPressed(ControlAction.ResetRound, pressedKeys))
            OnResetRoundRequested?.Invoke();
    }

    // Обновление второго игрока
    private void UpdateFriendPlayer(float deltaTime, HashSet<Keys> pressedKeys)
    {
        var move = 0f;
        if (pressedKeys.Contains(Keys.Left)) move -= 1f;
        if (pressedKeys.Contains(Keys.Right)) move += 1f;

        var rightBirdSpeed = gameState.IsBotControlledRightBird ? GameSettings.BotMoveSpeed : GameSettings.BirdMoveSpeed;
        gameState.Bot.Velocity = new PointF(move * rightBirdSpeed, gameState.Bot.Velocity.Y);

        var rightBirdJumpSpeed = gameState.IsBotControlledRightBird ? -420f : GameSettings.BirdJumpSpeed;
        if (pressedKeys.Contains(Keys.Up))
            TryJump(gameState.Bot, rightBirdJumpSpeed);

        gameState.Bot.IsSmashing = pressedKeys.Contains(Keys.Return) || pressedKeys.Contains(Keys.Enter);
    }

    // ИИ бота
    private void UpdateBot(float deltaTime)
    {
        var targetX = gameState.Bot.Center.X;
        var ballComing = gameState.Ball.Center.X >= gameState.NetX || gameState.Ball.Velocity.X > 0;

        if (ballComing)
            targetX = Math.Clamp(gameState.Ball.Center.X, gameState.NetX + 70, gameState.ArenaBounds.Right - 80);
        else
            targetX = gameState.ArenaBounds.Right - 180;

        var diff = targetX - gameState.Bot.Center.X;
        if (Math.Abs(diff) > 10)
            gameState.Bot.Velocity = new PointF(Math.Sign(diff) * GameSettings.BotMoveSpeed, gameState.Bot.Velocity.Y);
        else
            gameState.Bot.Velocity = new PointF(0, gameState.Bot.Velocity.Y);

        var closeEnoughX = Math.Abs(gameState.Ball.Center.X - gameState.Bot.Center.X) < 92;
        var ballAbove = gameState.Ball.Center.Y < gameState.Bot.Center.Y + 8;

        if (gameState.Ball.Center.X > gameState.NetX && closeEnoughX && gameState.Ball.DistanceTo(gameState.Bot.Center) < 122)
        {
            if (ballAbove && gameState.Ball.Velocity.Y > -40 && random.NextDouble() > 0.5)
            {
                gameState.Bot.IsSmashing = true;
                if (gameState.Bot.OnGround)
                {
                    var botJumpSpeed = gameState.IsBotControlledRightBird ? -420f : GameSettings.BirdJumpSpeed;
                    gameState.Bot.Velocity = new PointF(gameState.Bot.Velocity.X, botJumpSpeed);
                    gameState.Bot.OnGround = false;
                }
            }
            else
            {
                gameState.Bot.IsSmashing = false;
            }
        }
        else
        {
            gameState.Bot.IsSmashing = false;
        }
    }

    // Физика птицы
    private void UpdateBirdPhysics(Bird bird, float deltaTime, float minX, float maxX)
    {
        bird.Velocity = new PointF(bird.Velocity.X, bird.Velocity.Y + GameSettings.Gravity * deltaTime);
        bird.Position = new PointF(
            bird.Position.X + bird.Velocity.X * deltaTime,
            bird.Position.Y + bird.Velocity.Y * deltaTime);

        var clampedX = Math.Clamp(bird.Position.X, minX, maxX - bird.Width);
        var clampedY = Math.Min(bird.Position.Y, gameState.GroundY - bird.Height);
        bird.Position = new PointF(clampedX, clampedY);

        if (bird.Position.Y >= gameState.GroundY - bird.Height)
        {
            bird.Position = new PointF(bird.Position.X, gameState.GroundY - bird.Height);
            bird.Velocity = new PointF(bird.Velocity.X, 0);
            bird.OnGround = true;
            bird.IsSmashing = false;
        }
        else
        {
            bird.OnGround = false;
        }

        bird.ActionCooldown = Math.Max(0, bird.ActionCooldown - deltaTime);
    }

    // Обновление таймера обратного отсчёта
    private void UpdateCountdown(float deltaTime)
    {
        gameState.Countdown -= deltaTime;
        var shown = Math.Max(1, (int)Math.Ceiling(gameState.Countdown));
        gameState.BannerText = gameState.Countdown > 0 ? shown.ToString() : "Play!";

        var server = gameState.PlayerServe ? gameState.Player : gameState.Bot;
        gameState.Ball.Reset(server.Center.X + (gameState.PlayerServe ? 12f : -12f), server.Bounds.Top - GameSettings.ServeHoverOffsetY);

        if (gameState.Countdown <= 0f)
        {
            gameState.CountdownActive = false;
            gameState.BannerText = "Play!";
            LaunchServe(gameState.PlayerServe);
        }
    }

    // Обновление физики мяча
    private void UpdateBall(float deltaTime)
    {
        gameState.Ball.PreviousPosition = gameState.Ball.Position;

        var velocityX = gameState.Ball.Velocity.X * GameSettings.BallAirDrag;
        var velocityY = gameState.Ball.Velocity.Y + GameSettings.Gravity * deltaTime;
        gameState.Ball.Velocity = new PointF(
            Math.Clamp(velocityX, -GameSettings.BallMaxSpeedX, GameSettings.BallMaxSpeedX),
            Math.Clamp(velocityY, -GameSettings.BallMaxSpeedY, GameSettings.BallMaxSpeedY));
        gameState.Ball.Position = new PointF(
            gameState.Ball.Position.X + gameState.Ball.Velocity.X * deltaTime,
            gameState.Ball.Position.Y + gameState.Ball.Velocity.Y * deltaTime);

        HandleSideCrossing();
        ResolveBirdCollision(gameState.Player, fromLeftSide: true);
        ResolveBirdCollision(gameState.Bot, fromLeftSide: false);
        RefreshContactLocks();

        if (gameState.Ball.Bounds.Left <= gameState.ArenaBounds.Left)
        {
            gameState.Ball.Position = new PointF(gameState.ArenaBounds.Left + GameSettings.BallRadius, gameState.Ball.Position.Y);
            gameState.Ball.Velocity = new PointF(
                Math.Max(Math.Abs(gameState.Ball.Velocity.X) * 0.92f, 260f),
                Math.Min(gameState.Ball.Velocity.Y * 1.02f, GameSettings.BallMaxSpeedY));
        }

        if (gameState.Ball.Bounds.Right >= gameState.ArenaBounds.Right)
        {
            gameState.Ball.Position = new PointF(gameState.ArenaBounds.Right - GameSettings.BallRadius, gameState.Ball.Position.Y);
            gameState.Ball.Velocity = new PointF(
                -Math.Max(Math.Abs(gameState.Ball.Velocity.X) * 0.92f, 260f),
                Math.Min(gameState.Ball.Velocity.Y * 1.02f, GameSettings.BallMaxSpeedY));
        }

        var netRect = new RectangleF(gameState.NetX - GameSettings.NetWidth / 2f, gameState.GroundY - GameSettings.NetHeight, GameSettings.NetWidth, GameSettings.NetHeight);
        if (gameState.Ball.Bounds.IntersectsWith(netRect))
            ResolveNetCollision(netRect);

        if (gameState.Ball.Bounds.Top <= gameState.ArenaBounds.Top)
        {
            gameState.Ball.Position = new PointF(gameState.Ball.Position.X, gameState.ArenaBounds.Top + GameSettings.BallRadius);
            gameState.Ball.Velocity = new PointF(gameState.Ball.Velocity.X * 0.98f, Math.Abs(gameState.Ball.Velocity.Y) * 0.7f);
        }

        if (gameState.Ball.Bounds.Bottom >= gameState.GroundY)
        {
            var ballOnLeft = gameState.Ball.Center.X < gameState.NetX;
            AwardPoint(!ballOnLeft);
        }
    }

    // Сброс касаний при перелёте мяча
    private void HandleSideCrossing()
    {
        var currentSide = gameState.Ball.Center.X < gameState.NetX ? 0 : 1;
        if (currentSide == gameState.LastBallSide) return;

        if (currentSide == 0) gameState.LeftTouches = 0;
        else gameState.RightTouches = 0;

        gameState.LastBallSide = currentSide;
    }

    // Столкновение мяча с птицей
    private void ResolveBirdCollision(Bird bird, bool fromLeftSide)
    {
        if (bird.ActionCooldown > 0f) return;

        var closestX = Math.Clamp(gameState.Ball.Center.X, bird.Bounds.Left, bird.Bounds.Right);
        var closestY = Math.Clamp(gameState.Ball.Center.Y, bird.Bounds.Top, bird.Bounds.Bottom);
        var dx = gameState.Ball.Center.X - closestX;
        var dy = gameState.Ball.Center.Y - closestY;
        var distanceSquared = dx * dx + dy * dy;

        if (distanceSquared > gameState.Ball.Radius * gameState.Ball.Radius) return;

        var contactLocked = fromLeftSide ? gameState.PlayerContactLocked : gameState.BotContactLocked;
        if (contactLocked) return;

        if (!RegisterTouch(fromLeftSide)) return;

        if (fromLeftSide) gameState.PlayerContactLocked = true;
        else gameState.BotContactLocked = true;

        var collisionNormal = GetCollisionNormal(bird, closestX, closestY, dx, dy);
        var separation = gameState.Ball.Radius - MathF.Sqrt(Math.Max(distanceSquared, 0.0001f));
        var targetBallX = fromLeftSide ? bird.Bounds.Right + gameState.Ball.Radius + 2f : bird.Bounds.Left - gameState.Ball.Radius - 2f;
        gameState.Ball.Position = new PointF(targetBallX, gameState.Ball.Position.Y + collisionNormal.Y * (separation + 3f));

        var wantsSmash = bird.IsSmashing;
        var outgoingX = wantsSmash ? GameSettings.AutoSmashHorizontalSpeed : GameSettings.AutoReturnHorizontalSpeed;
        var outgoingY = wantsSmash ? GameSettings.AutoSmashVerticalSpeed : GameSettings.AutoReturnVerticalSpeed;
        var lateralBias = Math.Clamp((gameState.Ball.Center.X - bird.Center.X) * 2.1f, -110f, 110f);
        var verticalBias = Math.Clamp((gameState.Ball.Center.Y - bird.Center.Y) * 1.15f, -70f, 85f);
        var inheritedX = bird.Velocity.X * 0.32f;
        var inheritedY = bird.Velocity.Y < 0 ? bird.Velocity.Y * 0.14f : 0f;

        gameState.Ball.Velocity = new PointF(
            (fromLeftSide ? outgoingX : -outgoingX) + inheritedX + lateralBias,
            outgoingY + inheritedY + verticalBias);

        if (gameState.Ball.Center.Y > bird.Center.Y + 12)
            gameState.Ball.Velocity = new PointF(gameState.Ball.Velocity.X * 0.86f, -Math.Abs(gameState.Ball.Velocity.Y) * GameSettings.BallBounce);

        bird.ActionCooldown = Math.Max(bird.ActionCooldown, 0.18f);
        bird.IsSmashing = false;
    }

    // Обновление блокировок контактов
    private void RefreshContactLocks()
    {
        gameState.PlayerContactLocked = BallIntersectsBird(gameState.Player);
        gameState.BotContactLocked = BallIntersectsBird(gameState.Bot);
    }

    // Проверка пересечения мяча и птицы
    private bool BallIntersectsBird(Bird bird)
    {
        var closestX = Math.Clamp(gameState.Ball.Center.X, bird.Bounds.Left, bird.Bounds.Right);
        var closestY = Math.Clamp(gameState.Ball.Center.Y, bird.Bounds.Top, bird.Bounds.Bottom);
        var dx = gameState.Ball.Center.X - closestX;
        var dy = gameState.Ball.Center.Y - closestY;
        return dx * dx + dy * dy <= gameState.Ball.Radius * gameState.Ball.Radius;
    }

    // Расчёт нормали столкновения
    private static PointF GetCollisionNormal(Bird bird, float closestX, float closestY, float dx, float dy)
    {
        if (MathF.Abs(dx) > 0.01f || MathF.Abs(dy) > 0.01f)
        {
            var length = MathF.Sqrt(dx * dx + dy * dy);
            return new PointF(dx / length, dy / length);
        }

        var fallbackX = bird.Center.X <= closestX ? 1f : -1f;
        return new PointF(fallbackX, -0.35f);
    }

    // Запуск подачи
    private void LaunchServe(bool fromLeftSide)
    {
        gameState.LastBallSide = fromLeftSide ? 0 : 1;
        var variance = (float)(new Random().NextDouble() * 24f - 12f);
        gameState.Ball.Velocity = new PointF(
            fromLeftSide ? GameSettings.ServeLaunchSpeedX + variance : -GameSettings.ServeLaunchSpeedX + variance,
            GameSettings.ServeLaunchSpeedY);
        gameState.Ball.Position = new PointF(gameState.Ball.Position.X, gameState.Ball.Position.Y - 6f);
        gameState.BannerText = "Play!";
    }

    // Столкновение с сеткой
    private void ResolveNetCollision(RectangleF netRect)
    {
        var ballBottom = gameState.Ball.Bounds.Bottom;
        var hitsTopTape = ballBottom >= netRect.Top && gameState.Ball.Center.Y < netRect.Top;

        if (hitsTopTape && Math.Abs(gameState.Ball.Center.X - gameState.NetX) <= GameSettings.NetWidth / 2f + GameSettings.BallRadius)
        {
            gameState.Ball.Position = new PointF(gameState.Ball.Position.X, netRect.Top - GameSettings.BallRadius);
            gameState.Ball.Velocity = new PointF(gameState.Ball.Velocity.X * 0.78f, -Math.Abs(gameState.Ball.Velocity.Y) * 0.72f);
            return;
        }

        var pushLeft = gameState.Ball.Center.X < gameState.NetX;
        gameState.Ball.Position = new PointF(
            pushLeft ? netRect.Left - GameSettings.BallRadius : netRect.Right + GameSettings.BallRadius,
            gameState.Ball.Position.Y);
        gameState.Ball.Velocity = new PointF(
            pushLeft ? -Math.Abs(gameState.Ball.Velocity.X) * 0.66f : Math.Abs(gameState.Ball.Velocity.X) * 0.66f,
            gameState.Ball.Velocity.Y * 0.92f);
    }

    // Прыжок птицы
    private void TryJump(Bird bird, float? jumpSpeed = null)
    {
        if (!bird.OnGround) return;
        bird.Velocity = new PointF(bird.Velocity.X, jumpSpeed ?? GameSettings.BirdJumpSpeed);
        bird.OnGround = false;
    }

    // Регистрация касания мяча
    private bool RegisterTouch(bool leftSide)
    {
        if (leftSide)
        {
            if (gameState.LeftTouches >= 1)
            {
                AwardPoint(false);
                return false;
            }
            gameState.LeftTouches++;
        }
        else
        {
            if (gameState.RightTouches >= 1)
            {
                AwardPoint(true);
                return false;
            }
            gameState.RightTouches++;
        }
        return true;
    }

    // Начисление очка
    private void AwardPoint(bool playerWon)
    {
        if (playerWon)
        {
            gameState.PlayerScore++;
            gameState.BannerText = "Point to You!";
        }
        else
        {
            gameState.BotScore++;
            gameState.BannerText = gameState.GameMode == GameMode.SinglePlayer ? "Point to Bot!" : "Point to Right Bird!";
        }

        var playerLeadsEnough = gameState.PlayerScore >= GameSettings.WinningScore && gameState.PlayerScore - gameState.BotScore >= 2;
        var botLeadsEnough = gameState.BotScore >= GameSettings.WinningScore && gameState.BotScore - gameState.PlayerScore >= 2;

        if (playerLeadsEnough || botLeadsEnough)
        {
            gameState.GameFinished = true;
            gameState.CountdownActive = false;
            gameState.BannerText = playerLeadsEnough ? "You Win!" : gameState.GameMode == GameMode.SinglePlayer ? "Bot Wins!" : "Right Bird Wins!";
            return;
        }

        OnRoundEnded?.Invoke(playerWon);
    }

    public event Action<bool>? OnRoundEnded;
    public event Action? OnResetRoundRequested;
}