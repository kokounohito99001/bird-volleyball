using System.Drawing.Drawing2D;

namespace BirdVolleyball;

public partial class Form1 : Form
{
    private enum ScreenState
    {
        MainMenu,
        ModeSelect,
        Playing
    }

    private enum GameMode
    {
        SinglePlayer,
        Versus
    }

    private const float Gravity = 980f;
    private const float BirdMoveSpeed = 390f;
    private const float BirdJumpSpeed = -690f;
    private const float BotMoveSpeed = 455f;
    private const float GroundHeight = 92f;
    private const float NetHeight = 120f;
    private const float NetWidth = 18f;
    private const float BallRadius = 18f;
    private const float BallAirDrag = 0.994f;
    private const float BallBounce = 0.9f;
    private const float BallMaxSpeedX = 640f;
    private const float BallMaxSpeedY = 820f;
    private const float ServeHoverOffsetY = 78f;
    private const float ServeLaunchSpeedX = 280f;
    private const float ServeLaunchSpeedY = -360f;
    private const float AutoReturnHorizontalSpeed = 300f;
    private const float AutoReturnVerticalSpeed = -420f;
    private const float AutoSmashHorizontalSpeed = 580f;
    private const float AutoSmashVerticalSpeed = -650f;
    private const int WinningScore = 7;

    private readonly System.Windows.Forms.Timer gameTimer;
    private readonly HashSet<Keys> pressedKeys = [];
    private readonly ControlBindings controls;
    private readonly Random random = new();

    private Bird player = null!;
    private Bird bot = null!;
    private Ball ball = null!;

    private float countdown = 3f;
    private bool countdownActive = true;
    private bool playerServe = true;
    private bool gameFinished;
    private int playerScore;
    private int botScore;
    private int leftTouches;
    private int rightTouches;
    private int lastBallSide = -1;
    private bool playerContactLocked;
    private bool botContactLocked;
    private bool guideVisible;
    private GameMode gameMode = GameMode.SinglePlayer;
    private ScreenState screenState = ScreenState.MainMenu;
    private string bannerText = "Get Ready!";
    private bool isBotControlledRightBird = true;

    public Form1()
    {
        InitializeComponent();

        controls = ControlBindings.Load(Path.Combine(AppContext.BaseDirectory, "Settings", "Controls.json"));

        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        UpdateStyles();

        gameTimer = new System.Windows.Forms.Timer { Interval = 16 };
        gameTimer.Tick += (_, _) => TickGame(gameTimer.Interval / 1000f);

        KeyDown += HandleKeyDown;
        KeyUp += HandleKeyUp;
        MouseDown += HandleMouseDown;
        Resize += (_, _) => ResetArena();

        ResetArena();
        gameTimer.Start();
    }

    private RectangleF ArenaBounds => new(60, 80, ClientSize.Width - 120, ClientSize.Height - 140);

    private RectangleF StartButtonBounds => new(ClientSize.Width / 2f - 150, ClientSize.Height / 2f + 40, 300, 72);

    private RectangleF SingleModeButtonBounds => new(ClientSize.Width / 2f - 180, ClientSize.Height / 2f + 10, 360, 68);

    private RectangleF FriendModeButtonBounds => new(ClientSize.Width / 2f - 180, ClientSize.Height / 2f + 94, 360, 68);

    private RectangleF BackButtonBounds => new(ClientSize.Width / 2f - 120, ClientSize.Height / 2f + 188, 240, 52);

    private RectangleF GuideButtonBounds => new(ClientSize.Width - 230, 28, 170, 48);

    private RectangleF GuideCloseButtonBounds => new(ClientSize.Width / 2f + 180, ClientSize.Height / 2f - 165, 84, 40);

    private float GroundY => ArenaBounds.Bottom - GroundHeight;

    private float NetX => ArenaBounds.Left + ArenaBounds.Width / 2f;

    private void ResetArena()
    {
        var arena = ArenaBounds;
        player = new Bird(arena.Left + 160, GroundY - 72, isPlayer: true);
        bot = new Bird(arena.Right - 230, GroundY - 72, isPlayer: false);
        ball = new Ball(0, 0);
    }

    private void StartRound(bool serveOnPlayerSide, bool resetScore = false)
    {
        if (resetScore)
        {
            playerScore = 0;
            botScore = 0;
            gameFinished = false;
        }

        playerServe = serveOnPlayerSide;
        countdownActive = true;
        countdown = 3f;
        bannerText = "3";
        leftTouches = 0;
        rightTouches = 0;
        playerContactLocked = false;
        botContactLocked = false;

        player.Reset(GroundY - player.Height, ArenaBounds.Left + 160);
        bot.Reset(GroundY - bot.Height, ArenaBounds.Right - 230);

        var server = playerServe ? player : bot;
        ball.Reset(server.Center.X + (playerServe ? 12f : -12f), server.Bounds.Top - ServeHoverOffsetY);
        ball.Velocity = PointF.Empty;
        lastBallSide = playerServe ? 0 : 1;
        Invalidate();
    }

    private void TickGame(float deltaTime)
    {
        if (screenState != ScreenState.Playing)
        {
            Invalidate();
            return;
        }

        if (gameFinished)
        {
            Invalidate();
            return;
        }

        UpdatePlayer(deltaTime);
        if (gameMode == GameMode.SinglePlayer)
        {
            UpdateBot(deltaTime);
        }
        else
        {
            UpdateFriendPlayer(deltaTime);
        }
        UpdateBirdPhysics(player, deltaTime, ArenaBounds.Left, NetX - NetWidth / 2f);
        UpdateBirdPhysics(bot, deltaTime, NetX + NetWidth / 2f, ArenaBounds.Right);

        if (countdownActive)
        {
            countdown -= deltaTime;
            var shown = Math.Max(1, (int)Math.Ceiling(countdown));
            bannerText = countdown > 0 ? shown.ToString() : "Play!";
            var server = playerServe ? player : bot;
            ball.Reset(server.Center.X + (playerServe ? 12f : -12f), server.Bounds.Top - ServeHoverOffsetY);

            if (countdown <= 0f)
            {
                countdownActive = false;
                bannerText = "Play!";
                LaunchServe(playerServe);
            }

            Invalidate();
            return;
        }

        UpdateBall(deltaTime);
        bannerText = string.Empty;
        Invalidate();
    }

    private void UpdatePlayer(float deltaTime)
    {
        var move = 0f;
        if (pressedKeys.Contains(Keys.A))
        {
            move -= 1f;
        }

        if (pressedKeys.Contains(Keys.D))
        {
            move += 1f;
        }

        player.Velocity = new PointF(move * BirdMoveSpeed, player.Velocity.Y);

        if (pressedKeys.Contains(Keys.ShiftKey))
        {
            TryJump(player);
        }

        player.IsSmashing = pressedKeys.Contains(Keys.Space);

        if (controls.IsPressed(ControlAction.ResetRound, pressedKeys))
        {
            StartRound(playerServe, resetScore: false);
        }
    }

    private void UpdateFriendPlayer(float deltaTime)
    {
        var move = 0f;
        if (pressedKeys.Contains(Keys.Left))
        {
            move -= 1f;
        }

        if (pressedKeys.Contains(Keys.Right))
        {
            move += 1f;
        }

        var rightBirdSpeed = isBotControlledRightBird ? BotMoveSpeed : BirdMoveSpeed;
        bot.Velocity = new PointF(move * rightBirdSpeed, bot.Velocity.Y);

        var rightBirdJumpSpeed = isBotControlledRightBird ? -420f : BirdJumpSpeed;
        if (pressedKeys.Contains(Keys.Up))
        {
            TryJump(bot, rightBirdJumpSpeed);
        }

        bot.IsSmashing = pressedKeys.Contains(Keys.Return) || pressedKeys.Contains(Keys.Enter);
    }

    private void UpdateBot(float deltaTime)
    {
        var targetX = bot.Center.X;
        var ballComing = ball.Center.X >= NetX || ball.Velocity.X > 0;

        if (ballComing)
        {
            targetX = Math.Clamp(ball.Center.X, NetX + 70, ArenaBounds.Right - 80);
        }
        else
        {
            targetX = ArenaBounds.Right - 180;
        }

        var diff = targetX - bot.Center.X;
        if (Math.Abs(diff) > 10)
        {
            bot.Velocity = new PointF(Math.Sign(diff) * BotMoveSpeed, bot.Velocity.Y);
        }
        else
        {
            bot.Velocity = new PointF(0, bot.Velocity.Y);
        }

        var closeEnoughX = Math.Abs(ball.Center.X - bot.Center.X) < 92;
        var ballAbove = ball.Center.Y < bot.Center.Y + 8;

        if (ball.Center.X > NetX && closeEnoughX && ball.DistanceTo(bot.Center) < 122)
        {
            if (ballAbove && ball.Velocity.Y > -40 && random.NextDouble() > 0.5)
            {
                bot.IsSmashing = true;
                if (bot.OnGround)
                {
                    var botJumpSpeed = isBotControlledRightBird ? -420f : BirdJumpSpeed;
                    bot.Velocity = new PointF(bot.Velocity.X, botJumpSpeed);
                    bot.OnGround = false;
                }
            }
            else
            {
                bot.IsSmashing = false;
            }
        }
        else
        {
            bot.IsSmashing = false;
        }
    }

    private void UpdateBirdPhysics(Bird bird, float deltaTime, float minX, float maxX)
    {
        bird.Velocity = new PointF(bird.Velocity.X, bird.Velocity.Y + Gravity * deltaTime);
        bird.Position = new PointF(
            bird.Position.X + bird.Velocity.X * deltaTime,
            bird.Position.Y + bird.Velocity.Y * deltaTime);

        var clampedX = Math.Clamp(bird.Position.X, minX, maxX - bird.Width);
        var clampedY = Math.Min(bird.Position.Y, GroundY - bird.Height);
        bird.Position = new PointF(clampedX, clampedY);

        if (bird.Position.Y >= GroundY - bird.Height)
        {
            bird.Position = new PointF(bird.Position.X, GroundY - bird.Height);
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

    private void UpdateBall(float deltaTime)
    {
        ball.PreviousPosition = ball.Position;

        var velocityX = ball.Velocity.X * BallAirDrag;
        var velocityY = ball.Velocity.Y + Gravity * deltaTime;
        ball.Velocity = new PointF(
            Math.Clamp(velocityX, -BallMaxSpeedX, BallMaxSpeedX),
            Math.Clamp(velocityY, -BallMaxSpeedY, BallMaxSpeedY));
        ball.Position = new PointF(ball.Position.X + ball.Velocity.X * deltaTime, ball.Position.Y + ball.Velocity.Y * deltaTime);

        HandleSideCrossing();
        ResolveBirdCollision(player, fromLeftSide: true);
        ResolveBirdCollision(bot, fromLeftSide: false);
        RefreshContactLocks();

        if (ball.Bounds.Left <= ArenaBounds.Left)
        {
            ball.Position = new PointF(ArenaBounds.Left + BallRadius, ball.Position.Y);
            ball.Velocity = new PointF(
                Math.Max(Math.Abs(ball.Velocity.X) * 0.92f, 260f),
                Math.Min(ball.Velocity.Y * 1.02f, BallMaxSpeedY));
        }

        if (ball.Bounds.Right >= ArenaBounds.Right)
        {
            ball.Position = new PointF(ArenaBounds.Right - BallRadius, ball.Position.Y);
            ball.Velocity = new PointF(
                -Math.Max(Math.Abs(ball.Velocity.X) * 0.92f, 260f),
                Math.Min(ball.Velocity.Y * 1.02f, BallMaxSpeedY));
        }

        var netRect = new RectangleF(NetX - NetWidth / 2f, GroundY - NetHeight, NetWidth, NetHeight);
        if (ball.Bounds.IntersectsWith(netRect))
        {
            ResolveNetCollision(netRect);
        }

        if (ball.Bounds.Top <= ArenaBounds.Top)
        {
            ball.Position = new PointF(ball.Position.X, ArenaBounds.Top + BallRadius);
            ball.Velocity = new PointF(ball.Velocity.X * 0.98f, Math.Abs(ball.Velocity.Y) * 0.7f);
        }

        if (ball.Bounds.Bottom >= GroundY)
        {
            var ballOnLeft = ball.Center.X < NetX;
            AwardPoint(playerWon: !ballOnLeft);
        }
    }

    private void HandleSideCrossing()
    {
        var currentSide = ball.Center.X < NetX ? 0 : 1;
        if (currentSide == lastBallSide)
        {
            return;
        }

        if (currentSide == 0)
        {
            leftTouches = 0;
        }
        else
        {
            rightTouches = 0;
        }

        lastBallSide = currentSide;
    }

    private void ResolveBirdCollision(Bird bird, bool fromLeftSide)
    {
        if (bird.ActionCooldown > 0f)
        {
            return;
        }

        var closestX = Math.Clamp(ball.Center.X, bird.Bounds.Left, bird.Bounds.Right);
        var closestY = Math.Clamp(ball.Center.Y, bird.Bounds.Top, bird.Bounds.Bottom);
        var dx = ball.Center.X - closestX;
        var dy = ball.Center.Y - closestY;
        var distanceSquared = dx * dx + dy * dy;

        if (distanceSquared > ball.Radius * ball.Radius)
        {
            return;
        }

        var contactLocked = fromLeftSide ? playerContactLocked : botContactLocked;
        if (contactLocked)
        {
            return;
        }

        if (!RegisterTouch(fromLeftSide))
        {
            return;
        }

        if (fromLeftSide)
        {
            playerContactLocked = true;
        }
        else
        {
            botContactLocked = true;
        }

        var collisionNormal = GetCollisionNormal(bird, closestX, closestY, dx, dy);
        var separation = ball.Radius - MathF.Sqrt(Math.Max(distanceSquared, 0.0001f));
        var targetBallX = fromLeftSide
            ? bird.Bounds.Right + ball.Radius + 2f
            : bird.Bounds.Left - ball.Radius - 2f;
        ball.Position = new PointF(
            targetBallX,
            ball.Position.Y + collisionNormal.Y * (separation + 3f));

        var wantsSmash = bird.IsSmashing;
        var outgoingX = wantsSmash ? AutoSmashHorizontalSpeed : AutoReturnHorizontalSpeed;
        var outgoingY = wantsSmash ? AutoSmashVerticalSpeed : AutoReturnVerticalSpeed;
        var lateralBias = Math.Clamp((ball.Center.X - bird.Center.X) * 2.1f, -110f, 110f);
        var verticalBias = Math.Clamp((ball.Center.Y - bird.Center.Y) * 1.15f, -70f, 85f);
        var inheritedX = bird.Velocity.X * 0.32f;
        var inheritedY = bird.Velocity.Y < 0 ? bird.Velocity.Y * 0.14f : 0f;

        ball.Velocity = new PointF(
            (fromLeftSide ? outgoingX : -outgoingX) + inheritedX + lateralBias,
            outgoingY + inheritedY + verticalBias);

        if (ball.Center.Y > bird.Center.Y + 12)
        {
            ball.Velocity = new PointF(ball.Velocity.X * 0.86f, -Math.Abs(ball.Velocity.Y) * BallBounce);
        }

        bird.ActionCooldown = Math.Max(bird.ActionCooldown, 0.18f);
        bird.IsSmashing = false;
    }

    private void RefreshContactLocks()
    {
        playerContactLocked = BallIntersectsBird(player);
        botContactLocked = BallIntersectsBird(bot);
    }

    private bool BallIntersectsBird(Bird bird)
    {
        var closestX = Math.Clamp(ball.Center.X, bird.Bounds.Left, bird.Bounds.Right);
        var closestY = Math.Clamp(ball.Center.Y, bird.Bounds.Top, bird.Bounds.Bottom);
        var dx = ball.Center.X - closestX;
        var dy = ball.Center.Y - closestY;
        return dx * dx + dy * dy <= ball.Radius * ball.Radius;
    }

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

    private void LaunchServe(bool fromLeftSide)
    {
        lastBallSide = fromLeftSide ? 0 : 1;
        var variance = (float)(random.NextDouble() * 24f - 12f);
        ball.Velocity = new PointF(fromLeftSide ? ServeLaunchSpeedX + variance : -ServeLaunchSpeedX + variance, ServeLaunchSpeedY);
        ball.Position = new PointF(ball.Position.X, ball.Position.Y - 6f);
        bannerText = "Play!";
    }

    private void ResolveNetCollision(RectangleF netRect)
    {
        var ballBottom = ball.Bounds.Bottom;
        var hitsTopTape = ballBottom >= netRect.Top && ball.Center.Y < netRect.Top;

        if (hitsTopTape && Math.Abs(ball.Center.X - NetX) <= NetWidth / 2f + BallRadius)
        {
            ball.Position = new PointF(ball.Position.X, netRect.Top - BallRadius);
            ball.Velocity = new PointF(ball.Velocity.X * 0.78f, -Math.Abs(ball.Velocity.Y) * 0.72f);
            return;
        }

        var pushLeft = ball.Center.X < NetX;
        ball.Position = new PointF(pushLeft ? netRect.Left - BallRadius : netRect.Right + BallRadius, ball.Position.Y);
        ball.Velocity = new PointF(pushLeft ? -Math.Abs(ball.Velocity.X) * 0.66f : Math.Abs(ball.Velocity.X) * 0.66f, ball.Velocity.Y * 0.92f);
    }

    private void TryJump(Bird bird, float? jumpSpeed = null)
    {
        if (!bird.OnGround)
        {
            return;
        }

        bird.Velocity = new PointF(bird.Velocity.X, jumpSpeed ?? BirdJumpSpeed);
        bird.OnGround = false;
    }

    private bool RegisterTouch(bool leftSide)
    {
        if (leftSide)
        {
            if (leftTouches >= 1)
            {
                AwardPoint(playerWon: false);
                return false;
            }

            leftTouches++;
        }
        else
        {
            if (rightTouches >= 1)
            {
                AwardPoint(playerWon: true);
                return false;
            }

            rightTouches++;
        }

        return true;
    }

    private void AwardPoint(bool playerWon)
    {
        if (playerWon)
        {
            playerScore++;
            bannerText = "Point to You!";
        }
        else
        {
            botScore++;
            bannerText = gameMode == GameMode.SinglePlayer ? "Point to Bot!" : "Point to Right Bird!";
        }

        var playerLeadsEnough = playerScore >= WinningScore && playerScore - botScore >= 2;
        var botLeadsEnough = botScore >= WinningScore && botScore - playerScore >= 2;

        if (playerLeadsEnough || botLeadsEnough)
        {
            gameFinished = true;
            countdownActive = false;
            bannerText = playerLeadsEnough ? "You Win!" : gameMode == GameMode.SinglePlayer ? "Bot Wins!" : "Right Bird Wins!";
            return;
        }

        StartRound(serveOnPlayerSide: playerWon, resetScore: false);
    }

    private void HandleKeyDown(object? sender, KeyEventArgs e)
    {
        pressedKeys.Add(e.KeyCode);

        if (screenState != ScreenState.Playing)
        {
            if (guideVisible && e.KeyCode == Keys.Escape)
            {
                guideVisible = false;
                return;
            }

            if (e.KeyCode == Keys.G)
            {
                guideVisible = !guideVisible;
                return;
            }

            if (screenState == ScreenState.MainMenu && (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space))
            {
                screenState = ScreenState.ModeSelect;
                return;
            }

            if (screenState == ScreenState.ModeSelect && e.KeyCode == Keys.Escape)
            {
                screenState = ScreenState.MainMenu;
                guideVisible = false;
                return;
            }

            return;
        }

        if (gameFinished && controls.IsPressed(ControlAction.ResetRound, pressedKeys))
        {
            StartRound(serveOnPlayerSide: true, resetScore: true);
        }
    }

    private void HandleKeyUp(object? sender, KeyEventArgs e)
    {
        pressedKeys.Remove(e.KeyCode);
    }

    private void HandleMouseDown(object? sender, MouseEventArgs e)
    {
        if (screenState != ScreenState.Playing)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            if (guideVisible)
            {
                if (GuideCloseButtonBounds.Contains(e.Location))
                {
                    guideVisible = false;
                }

                return;
            }

            if (GuideButtonBounds.Contains(e.Location))
            {
                guideVisible = true;
                return;
            }

            if (screenState == ScreenState.MainMenu && StartButtonBounds.Contains(e.Location))
            {
                screenState = ScreenState.ModeSelect;
                return;
            }

            if (screenState == ScreenState.ModeSelect && SingleModeButtonBounds.Contains(e.Location))
            {
                isBotControlledRightBird = true;
                BeginGame(GameMode.SinglePlayer);
                return;
            }

            if (screenState == ScreenState.ModeSelect && FriendModeButtonBounds.Contains(e.Location))
            {
                isBotControlledRightBird = false;
                BeginGame(GameMode.Versus);
                return;
            }

            if (screenState == ScreenState.ModeSelect && BackButtonBounds.Contains(e.Location))
            {
                screenState = ScreenState.MainMenu;
            }

            return;
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        DrawBackground(g);

        if (screenState != ScreenState.Playing)
        {
            DrawMenu(g);
            return;
        }

        DrawArena(g);
        DrawBird(g, player, Color.FromArgb(255, 204, 87), Color.FromArgb(103, 65, 34), facingRight: true);
        DrawBird(g, bot, Color.FromArgb(129, 178, 154), Color.FromArgb(46, 77, 66), facingRight: false);
        DrawBall(g);
        DrawHud(g);
    }

    private void DrawBackground(Graphics g)
    {
        using var skyBrush = new LinearGradientBrush(ClientRectangle, Color.FromArgb(134, 210, 255), Color.FromArgb(247, 253, 255), LinearGradientMode.Vertical);
        g.FillRectangle(skyBrush, ClientRectangle);

        using var cloudBrush = new SolidBrush(Color.FromArgb(180, 255, 255, 255));
        g.FillEllipse(cloudBrush, 90, 70, 180, 60);
        g.FillEllipse(cloudBrush, 230, 100, 150, 50);
        g.FillEllipse(cloudBrush, ClientSize.Width - 320, 80, 210, 65);
    }

    private void DrawArena(Graphics g)
    {
        var arena = ArenaBounds;
        using var courtBrush = new SolidBrush(Color.FromArgb(255, 235, 188));
        using var grassBrush = new SolidBrush(Color.FromArgb(132, 202, 115));
        using var netBrush = new SolidBrush(Color.FromArgb(239, 244, 255));
        using var linePen = new Pen(Color.FromArgb(255, 255, 255), 4);
        using var borderPen = new Pen(Color.FromArgb(47, 95, 64), 3);

        g.FillRectangle(grassBrush, 0, (int)GroundY, ClientSize.Width, ClientSize.Height - (int)GroundY);
        g.FillRectangle(courtBrush, arena.Left, GroundY, arena.Width, 16);
        g.DrawLine(linePen, arena.Left, GroundY + 8, arena.Right, GroundY + 8);
        g.DrawLine(borderPen, NetX, GroundY, NetX, GroundY - NetHeight);
        g.FillRectangle(netBrush, NetX - NetWidth / 2f, GroundY - NetHeight, NetWidth, NetHeight);
        g.DrawLine(linePen, NetX - 28, GroundY - NetHeight, NetX + 28, GroundY - NetHeight);
    }

    private void DrawMenu(Graphics g)
    {
        using var panelBrush = new SolidBrush(Color.FromArgb(188, 255, 255, 255));
        using var shadowBrush = new SolidBrush(Color.FromArgb(45, 33, 62, 88));
        using var titleFont = new Font("Segoe UI", 40, FontStyle.Bold);
        using var buttonFont = new Font("Segoe UI", 20, FontStyle.Bold);
        using var smallFont = new Font("Segoe UI", 14, FontStyle.Regular);
        using var textBrush = new SolidBrush(Color.FromArgb(24, 51, 82));
        using var accentBrush = new SolidBrush(Color.FromArgb(255, 204, 87));
        using var secondaryBrush = new SolidBrush(Color.FromArgb(129, 178, 154));

        var panelRect = new RectangleF(ClientSize.Width / 2f - 300, ClientSize.Height / 2f - 170, 600, 320);
        g.FillRoundedRectangle(shadowBrush, panelRect.X + 8, panelRect.Y + 10, panelRect.Width, panelRect.Height, 28);
        g.FillRoundedRectangle(panelBrush, panelRect.X, panelRect.Y, panelRect.Width, panelRect.Height, 28);

        DrawMenuButton(g, GuideButtonBounds, "Управление", secondaryBrush, textBrush, buttonFont);
        g.DrawString("Bird Volleyball", titleFont, textBrush, panelRect.X + 88, panelRect.Y + 46);

        if (screenState == ScreenState.MainMenu)
        {
            DrawMenuButton(g, StartButtonBounds, "Начать игру", accentBrush, textBrush, buttonFont);
        }
        else
        {
            DrawMenuButton(g, SingleModeButtonBounds, "Одиночная игра", accentBrush, textBrush, buttonFont);
            DrawMenuButton(g, FriendModeButtonBounds, "Игра с другом", secondaryBrush, textBrush, buttonFont);
            DrawMenuButton(g, BackButtonBounds, "Назад", panelBrush, textBrush, smallFont);
        }

        if (screenState == ScreenState.MainMenu)
        {
            g.FillRectangle(panelBrush, panelRect.X + 60, panelRect.Y + 112, 500, 40);
        }

        if (guideVisible)
        {
            DrawGuideOverlay(g);
        }
    }

    private void DrawMenuButton(Graphics g, RectangleF rect, string text, Brush buttonBrush, Brush textBrush, Font font)
    {
        using var shadowBrush = new SolidBrush(Color.FromArgb(36, 23, 43, 61));
        g.FillRoundedRectangle(shadowBrush, rect.X + 4, rect.Y + 6, rect.Width, rect.Height, 24);
        g.FillRoundedRectangle(buttonBrush, rect.X, rect.Y, rect.Width, rect.Height, 24);
        var size = g.MeasureString(text, font);
        g.DrawString(text, font, textBrush, rect.X + (rect.Width - size.Width) / 2f, rect.Y + (rect.Height - size.Height) / 2f - 1f);
    }

    private void DrawGuideOverlay(Graphics g)
    {
        using var overlayBrush = new SolidBrush(Color.FromArgb(150, 17, 28, 38));
        using var panelBrush = new SolidBrush(Color.FromArgb(242, 255, 255, 255));
        using var textBrush = new SolidBrush(Color.FromArgb(24, 51, 82));
        using var accentBrush = new SolidBrush(Color.FromArgb(255, 204, 87));
        using var titleFont = new Font("Segoe UI", 24, FontStyle.Bold);
        using var lineFont = new Font("Segoe UI", 13, FontStyle.Regular);
        using var closeFont = new Font("Segoe UI", 12, FontStyle.Bold);

        g.FillRectangle(overlayBrush, ClientRectangle);
        var panelRect = new RectangleF(ClientSize.Width / 2f - 280, ClientSize.Height / 2f - 180, 560, 360);
        g.FillRoundedRectangle(panelBrush, panelRect.X, panelRect.Y, panelRect.Width, panelRect.Height, 26);
        g.FillRoundedRectangle(accentBrush, GuideCloseButtonBounds.X, GuideCloseButtonBounds.Y, GuideCloseButtonBounds.Width, GuideCloseButtonBounds.Height, 18);
        g.DrawString("Закрыть", closeFont, textBrush, GuideCloseButtonBounds.X + 10, GuideCloseButtonBounds.Y + 9);

        g.DrawString("Управление", titleFont, textBrush, panelRect.X + 32, panelRect.Y + 24);
        g.DrawString("Одиночная игра", lineFont, textBrush, panelRect.X + 36, panelRect.Y + 80);
        g.DrawString("Левая птица: A / D", lineFont, textBrush, panelRect.X + 36, panelRect.Y + 110);
        g.DrawString("Прыжок: Shift", lineFont, textBrush, panelRect.X + 36, panelRect.Y + 138);
        g.DrawString("Смэш: Space", lineFont, textBrush, panelRect.X + 36, panelRect.Y + 166);
        g.DrawString("Сброс раунда: R", lineFont, textBrush, panelRect.X + 36, panelRect.Y + 194);
        g.DrawString("Правая птица: бот", lineFont, textBrush, panelRect.X + 36, panelRect.Y + 222);

        g.DrawString("Игра с другом", lineFont, textBrush, panelRect.X + 320, panelRect.Y + 80);
        g.DrawString("Левая птица: A / D", lineFont, textBrush, panelRect.X + 320, panelRect.Y + 110);
        g.DrawString("Прыжок: Shift", lineFont, textBrush, panelRect.X + 320, panelRect.Y + 138);
        g.DrawString("Смэш: Space", lineFont, textBrush, panelRect.X + 320, panelRect.Y + 166);
        g.DrawString("Правая птица: ← / →", lineFont, textBrush, panelRect.X + 320, panelRect.Y + 194);
        g.DrawString("Прыжок: ↑", lineFont, textBrush, panelRect.X + 320, panelRect.Y + 222);
        g.DrawString("Смэш: Enter", lineFont, textBrush, panelRect.X + 320, panelRect.Y + 250);
    }

    private void DrawBird(Graphics g, Bird bird, Color bodyColor, Color accentColor, bool facingRight)
    {
        using var bodyBrush = new SolidBrush(bodyColor);
        using var wingBrush = new SolidBrush(Color.FromArgb(230, accentColor));
        using var eyeBrush = new SolidBrush(Color.White);
        using var pupilBrush = new SolidBrush(Color.Black);
        using var beakBrush = new SolidBrush(Color.FromArgb(255, 148, 66));
        var dir = facingRight ? 1f : -1f;
        var eyeX = facingRight ? bird.Bounds.X + 46 : bird.Bounds.X + 15;
        var pupilX = facingRight ? bird.Bounds.X + 51 : bird.Bounds.X + 19;
        var beakBaseX = facingRight ? bird.Bounds.Right - 8 : bird.Bounds.Left + 8;
        var beakTipX = facingRight ? bird.Bounds.Right + 12 : bird.Bounds.Left - 12;
        var wingX = facingRight ? bird.Bounds.X + 12 : bird.Bounds.X + 30;

        g.FillEllipse(bodyBrush, bird.Bounds);
        g.FillEllipse(wingBrush, wingX, bird.Bounds.Y + 24, 32, 24);
        g.FillEllipse(eyeBrush, eyeX, bird.Bounds.Y + 18, 13, 13);
        g.FillEllipse(pupilBrush, pupilX, bird.Bounds.Y + 22, 5, 5);
        g.FillPolygon(beakBrush,
        [
            new PointF(beakBaseX, bird.Bounds.Y + 30),
            new PointF(beakTipX, bird.Bounds.Y + 36),
            new PointF(beakBaseX, bird.Bounds.Y + 42)
        ]);

        var legY = bird.Bounds.Bottom - 4;
        using var legPen = new Pen(accentColor, 3);
        g.DrawLine(legPen, bird.Bounds.X + 24, legY, bird.Bounds.X + 24 - 2 * dir, legY + 16);
        g.DrawLine(legPen, bird.Bounds.X + 46, legY, bird.Bounds.X + 46 + 2 * dir, legY + 16);
    }

    private void DrawBall(Graphics g)
    {
        using var ballBrush = new SolidBrush(Color.FromArgb(255, 251, 205));
        using var seamPen = new Pen(Color.FromArgb(207, 139, 36), 2);

        g.FillEllipse(ballBrush, ball.Bounds);
        g.DrawArc(seamPen, ball.Bounds, 20, 140);
        g.DrawArc(seamPen, ball.Bounds, 200, 140);
    }

    private void DrawHud(Graphics g)
    {
        using var titleFont = new Font("Segoe UI Semibold", 17, FontStyle.Bold);
        using var scoreFont = new Font("Segoe UI", 26, FontStyle.Bold);
        using var uiFont = new Font("Segoe UI", 12, FontStyle.Regular);
        using var bigFont = new Font("Segoe UI", 30, FontStyle.Bold);
        using var textBrush = new SolidBrush(Color.FromArgb(27, 53, 86));
        using var panelBrush = new SolidBrush(Color.FromArgb(170, 255, 255, 255));

        g.FillRoundedRectangle(panelBrush, 28, 18, 390, 100, 18);
        g.DrawString("Bird Volleyball", titleFont, textBrush, 46, 30);
        g.DrawString($"{playerScore} : {botScore}", scoreFont, textBrush, 48, 56);
        if (gameMode == GameMode.SinglePlayer)
        {
            g.DrawString("Left bird: A/D  Jump: Shift  Smash: Space  Reset: R", uiFont, textBrush, 430, 34);
            g.DrawString("Mode: Single Player vs Bot. Serve stays with point winner.", uiFont, textBrush, 430, 64);
        }
        else
        {
            g.DrawString("Left bird: A/D  Jump: Shift  Smash: Space", uiFont, textBrush, 430, 34);
            g.DrawString("Right bird: Left/Right  Jump: Up  Smash: Enter  Reset: R", uiFont, textBrush, 430, 64);
        }

        if (!string.IsNullOrWhiteSpace(bannerText))
        {
            var size = g.MeasureString(bannerText, bigFont);
            g.FillRoundedRectangle(panelBrush, ClientSize.Width / 2f - size.Width / 2f - 30, 128, size.Width + 60, size.Height + 18, 20);
            g.DrawString(bannerText, bigFont, textBrush, ClientSize.Width / 2f - size.Width / 2f, 136);
        }

        if (gameFinished)
        {
            using var overlay = new SolidBrush(Color.FromArgb(120, 17, 28, 38));
            g.FillRectangle(overlay, ClientRectangle);
            var text = playerScore > botScore ? "Victory! Press R to replay." : "Defeat! Press R to rematch.";
            var size = g.MeasureString(text, bigFont);
            g.FillRoundedRectangle(panelBrush, ClientSize.Width / 2f - size.Width / 2f - 35, ClientSize.Height / 2f - 52, size.Width + 70, size.Height + 30, 24);
            g.DrawString(text, bigFont, textBrush, ClientSize.Width / 2f - size.Width / 2f, ClientSize.Height / 2f - 40);
        }
    }

    private void BeginGame(GameMode selectedMode)
    {
        gameMode = selectedMode;
        screenState = ScreenState.Playing;
        playerServe = true;
        guideVisible = false;
        StartRound(playerServe, resetScore: true);
    }
}
