namespace BirdVolleyball.Models;

public enum ScreenState { MainMenu, ModeSelect, Playing }
public enum GameMode { SinglePlayer, Versus }

public sealed class GameState
{
    public Bird Player { get; set; } = null!;
    public Bird Bot { get; set; } = null!;
    public Ball Ball { get; set; } = null!;

    public ScreenState ScreenState { get; set; } = ScreenState.MainMenu;
    public GameMode GameMode { get; set; } = GameMode.SinglePlayer;

    public float Countdown { get; set; } = 3f;
    public bool CountdownActive { get; set; } = true;
    public bool PlayerServe { get; set; } = true;
    public bool GameFinished { get; set; }
    public int PlayerScore { get; set; }
    public int BotScore { get; set; }
    public int LeftTouches { get; set; }
    public int RightTouches { get; set; }
    public int LastBallSide { get; set; } = -1;
    public bool PlayerContactLocked { get; set; }
    public bool BotContactLocked { get; set; }
    public bool GuideVisible { get; set; }
    public string BannerText { get; set; } = "Get Ready!";
    public bool IsBotControlledRightBird { get; set; } = true;

    public RectangleF ArenaBounds { get; private set; }
    public float GroundY => ArenaBounds.Bottom - GameSettings.GroundHeight;
    public float NetX => ArenaBounds.Left + ArenaBounds.Width / 2f;

    // Обновление границ арены
    public void UpdateArenaBounds(RectangleF bounds)
    {
        ArenaBounds = bounds;
    }

    // Создание птиц и мяча на стартовых позициях
    public void ResetPositions()
    {
        var arena = ArenaBounds;
        Player = new Bird(arena.Left + 160, GroundY - GameSettings.BirdHeight, isPlayer: true);
        Bot = new Bird(arena.Right - 230, GroundY - GameSettings.BirdHeight, isPlayer: false);
        Ball = new Ball(0, 0);
    }

    // Начало нового раунда
    public void StartRound(bool serveOnPlayerSide, bool resetScore = false)
    {
        if (resetScore)
        {
            PlayerScore = 0;
            BotScore = 0;
            GameFinished = false;
        }

        PlayerServe = serveOnPlayerSide;
        CountdownActive = true;
        Countdown = 3f;
        BannerText = "3";
        LeftTouches = 0;
        RightTouches = 0;
        PlayerContactLocked = false;
        BotContactLocked = false;

        Player.Reset(GroundY - Player.Height, ArenaBounds.Left + 160);
        Bot.Reset(GroundY - Bot.Height, ArenaBounds.Right - 230);

        var server = PlayerServe ? Player : Bot;
        Ball.Reset(server.Center.X + (PlayerServe ? 12f : -12f), server.Bounds.Top - GameSettings.ServeHoverOffsetY);
        Ball.Velocity = PointF.Empty;
        LastBallSide = PlayerServe ? 0 : 1;
    }
}