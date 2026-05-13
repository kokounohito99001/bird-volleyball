using System.Drawing.Drawing2D;
using BirdVolleyball.Models;

namespace BirdVolleyball.Views;

public sealed class GameRenderer
{
    private readonly GameState gameState;

    public GameRenderer(GameState gameState)
    {
        this.gameState = gameState;
    }

    // Главный метод рендеринга
    public void Render(Graphics g, IGameView view)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        DrawBackground(g, view.ClientSize);

        if (gameState.ScreenState != ScreenState.Playing)
        {
            MenuRenderer.DrawMenu(g, gameState, view);
            return;
        }

        DrawArena(g, view);
        DrawBird(g, gameState.Player, Color.FromArgb(255, 204, 87), Color.FromArgb(103, 65, 34), facingRight: true);
        DrawBird(g, gameState.Bot, Color.FromArgb(129, 178, 154), Color.FromArgb(46, 77, 66), facingRight: false);
        DrawBall(g);
        DrawHud(g, view);
    }

    // Отрисовка фона: небо и облака
    private void DrawBackground(Graphics g, Size clientSize)
    {
        var clientRect = new Rectangle(0, 0, clientSize.Width, clientSize.Height);
        using var skyBrush = new LinearGradientBrush(clientRect, Color.FromArgb(134, 210, 255), Color.FromArgb(247, 253, 255), LinearGradientMode.Vertical);
        g.FillRectangle(skyBrush, clientRect);

        using var cloudBrush = new SolidBrush(Color.FromArgb(180, 255, 255, 255));
        g.FillEllipse(cloudBrush, 90, 70, 180, 60);
        g.FillEllipse(cloudBrush, 230, 100, 150, 50);
        g.FillEllipse(cloudBrush, clientSize.Width - 320, 80, 210, 65);
    }

    // Отрисовка арены: площадка, сетка, линии
    private void DrawArena(Graphics g, IGameView view)
    {
        var arena = gameState.ArenaBounds;
        using var courtBrush = new SolidBrush(Color.FromArgb(255, 235, 188));
        using var grassBrush = new SolidBrush(Color.FromArgb(132, 202, 115));
        using var netBrush = new SolidBrush(Color.FromArgb(239, 244, 255));
        using var linePen = new Pen(Color.FromArgb(255, 255, 255), 4);
        using var borderPen = new Pen(Color.FromArgb(47, 95, 64), 3);

        g.FillRectangle(grassBrush, 0, (int)gameState.GroundY, view.ClientSize.Width, view.ClientSize.Height - (int)gameState.GroundY);
        g.FillRectangle(courtBrush, arena.Left, gameState.GroundY, arena.Width, 16);
        g.DrawLine(linePen, arena.Left, gameState.GroundY + 8, arena.Right, gameState.GroundY + 8);
        g.DrawLine(borderPen, gameState.NetX, gameState.GroundY, gameState.NetX, gameState.GroundY - GameSettings.NetHeight);
        g.FillRectangle(netBrush, gameState.NetX - GameSettings.NetWidth / 2f, gameState.GroundY - GameSettings.NetHeight, GameSettings.NetWidth, GameSettings.NetHeight);
        g.DrawLine(linePen, gameState.NetX - 28, gameState.GroundY - GameSettings.NetHeight, gameState.NetX + 28, gameState.GroundY - GameSettings.NetHeight);
    }

    // Отрисовка птицы
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

    // Отрисовка мяча
    private void DrawBall(Graphics g)
    {
        using var ballBrush = new SolidBrush(Color.FromArgb(255, 251, 205));
        using var seamPen = new Pen(Color.FromArgb(207, 139, 36), 2);

        g.FillEllipse(ballBrush, gameState.Ball.Bounds);
        g.DrawArc(seamPen, gameState.Ball.Bounds, 20, 140);
        g.DrawArc(seamPen, gameState.Ball.Bounds, 200, 140);
    }

    // Отрисовка интерфейса: счёт, подсказки, баннеры
    private void DrawHud(Graphics g, IGameView view)
    {
        using var titleFont = new Font("Segoe UI Semibold", 17, FontStyle.Bold);
        using var scoreFont = new Font("Segoe UI", 26, FontStyle.Bold);
        using var uiFont = new Font("Segoe UI", 12, FontStyle.Regular);
        using var bigFont = new Font("Segoe UI", 30, FontStyle.Bold);
        using var textBrush = new SolidBrush(Color.FromArgb(27, 53, 86));
        using var panelBrush = new SolidBrush(Color.FromArgb(170, 255, 255, 255));

        g.FillRoundedRectangle(panelBrush, 28, 18, 390, 100, 18);
        g.DrawString("Bird Volleyball", titleFont, textBrush, 46, 30);
        g.DrawString($"{gameState.PlayerScore} : {gameState.BotScore}", scoreFont, textBrush, 48, 56);

        if (gameState.GameMode == GameMode.SinglePlayer)
        {
            g.DrawString("Left bird: A/D  Jump: Shift  Smash: Space  Reset: R", uiFont, textBrush, 430, 34);
            g.DrawString("Mode: Single Player vs Bot. Serve stays with point winner.", uiFont, textBrush, 430, 64);
        }
        else
        {
            g.DrawString("Left bird: A/D  Jump: Shift  Smash: Space", uiFont, textBrush, 430, 34);
            g.DrawString("Right bird: Left/Right  Jump: Up  Smash: Enter  Reset: R", uiFont, textBrush, 430, 64);
        }

        if (!string.IsNullOrWhiteSpace(gameState.BannerText))
        {
            var size = g.MeasureString(gameState.BannerText, bigFont);
            g.FillRoundedRectangle(panelBrush, view.ClientSize.Width / 2f - size.Width / 2f - 30, 128, size.Width + 60, size.Height + 18, 20);
            g.DrawString(gameState.BannerText, bigFont, textBrush, view.ClientSize.Width / 2f - size.Width / 2f, 136);
        }

        if (gameState.GameFinished)
        {
            using var overlay = new SolidBrush(Color.FromArgb(120, 17, 28, 38));
            g.FillRectangle(overlay, view.ClientRectangle);
            var text = gameState.PlayerScore > gameState.BotScore ? "Victory! Press R to replay." : "Defeat! Press R to rematch.";
            var size = g.MeasureString(text, bigFont);
            g.FillRoundedRectangle(panelBrush, view.ClientSize.Width / 2f - size.Width / 2f - 35, view.ClientSize.Height / 2f - 52, size.Width + 70, size.Height + 30, 24);
            g.DrawString(text, bigFont, textBrush, view.ClientSize.Width / 2f - size.Width / 2f, view.ClientSize.Height / 2f - 40);
        }
    }
}