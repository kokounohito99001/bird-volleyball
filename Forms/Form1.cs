using BirdVolleyball.Controllers;
using BirdVolleyball.Models;
using BirdVolleyball.Views;

namespace BirdVolleyball;

public partial class Form1 : Form, IGameView
{
    private readonly System.Windows.Forms.Timer gameTimer;
    private readonly GameState gameState;
    private readonly GameRenderer renderer;
    private readonly InputController inputController;
    private readonly GameController gameController;
    private readonly ControlBindings controls;

    public Form1()
    {
        InitializeComponent();

        // Initialize bindings
        controls = ControlBindings.Load(Path.Combine(AppContext.BaseDirectory, "Settings", "Controls.json"));

        // Initialize game state
        gameState = new GameState();

        // Initialize controllers
        inputController = new InputController(gameState, controls);
        gameController = new GameController(gameState);

        // Initialize renderer
        renderer = new GameRenderer(gameState);

        // Setup form
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        UpdateStyles();

        // Setup timer
        gameTimer = new System.Windows.Forms.Timer { Interval = 16 };
        gameTimer.Tick += (_, _) => TickGame(gameTimer.Interval / 1000f);

        // Setup events
        KeyDown += HandleKeyDown;
        KeyUp += HandleKeyUp;
        MouseDown += HandleMouseDown;
        Resize += (_, _) => ResetArena();

        gameController.OnRoundEnded += (playerWon) => gameState.StartRound(playerWon, false);
        inputController.OnResetGameRequested += () => gameState.StartRound(true, true);
        gameController.OnResetRoundRequested += () => gameState.StartRound(gameState.PlayerServe, false);

        ResetArena();
        gameTimer.Start();
    }

    private void ResetArena()
    {
        var arena = new RectangleF(60, 80, ClientSize.Width - 120, ClientSize.Height - 140);
        gameState.UpdateArenaBounds(arena);
        gameState.ResetPositions();
    }

    private void TickGame(float deltaTime)
    {
        gameController.Update(deltaTime, inputController.GetPressedKeys(), controls);
        Invalidate();
    }

    private void HandleKeyDown(object? sender, KeyEventArgs e)
    {
        inputController.HandleKeyDown(e.KeyCode);
    }

    private void HandleKeyUp(object? sender, KeyEventArgs e)
    {
        inputController.HandleKeyUp(e.KeyCode);
    }

    private void HandleMouseDown(object? sender, MouseEventArgs e)
    {
        if (gameState.ScreenState != ScreenState.Playing)
        {
            if (e.Button != MouseButtons.Left) return;

            if (gameState.GuideVisible)
            {
                if (MenuRenderer.GetGuideCloseButtonBounds(this).Contains(e.Location))
                    gameState.GuideVisible = false;
                return;
            }

            if (MenuRenderer.GetGuideButtonBounds(this).Contains(e.Location))
            {
                gameState.GuideVisible = true;
                return;
            }

            if (gameState.ScreenState == ScreenState.MainMenu && MenuRenderer.GetStartButtonBounds(this).Contains(e.Location))
            {
                gameState.ScreenState = ScreenState.ModeSelect;
                return;
            }

            if (gameState.ScreenState == ScreenState.ModeSelect)
            {
                if (MenuRenderer.GetSingleModeButtonBounds(this).Contains(e.Location))
                {
                    gameState.IsBotControlledRightBird = true;
                    BeginGame(GameMode.SinglePlayer);
                    return;
                }

                if (MenuRenderer.GetFriendModeButtonBounds(this).Contains(e.Location))
                {
                    gameState.IsBotControlledRightBird = false;
                    BeginGame(GameMode.Versus);
                    return;
                }

                if (MenuRenderer.GetBackButtonBounds(this).Contains(e.Location))
                {
                    gameState.ScreenState = ScreenState.MainMenu;
                }
            }
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        renderer.Render(e.Graphics, this);
    }

    private void BeginGame(GameMode selectedMode)
    {
        gameState.GameMode = selectedMode;
        gameState.ScreenState = ScreenState.Playing;
        gameState.PlayerServe = true;
        gameState.GuideVisible = false;
        gameState.StartRound(true, true);
    }
}