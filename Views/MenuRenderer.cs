using BirdVolleyball.Models;

namespace BirdVolleyball.Views;

public static class MenuRenderer
{
    // Отрисовка меню
    public static void DrawMenu(Graphics g, GameState state, IGameView view)
    {
        using var panelBrush = new SolidBrush(Color.FromArgb(188, 255, 255, 255));
        using var shadowBrush = new SolidBrush(Color.FromArgb(45, 33, 62, 88));
        using var titleFont = new Font("Segoe UI", 40, FontStyle.Bold);
        using var buttonFont = new Font("Segoe UI", 20, FontStyle.Bold);
        using var smallFont = new Font("Segoe UI", 14, FontStyle.Regular);
        using var textBrush = new SolidBrush(Color.FromArgb(24, 51, 82));
        using var accentBrush = new SolidBrush(Color.FromArgb(255, 204, 87));
        using var secondaryBrush = new SolidBrush(Color.FromArgb(129, 178, 154));

        var panelRect = new RectangleF(view.ClientSize.Width / 2f - 300, view.ClientSize.Height / 2f - 170, 600, 320);
        g.FillRoundedRectangle(shadowBrush, panelRect.X + 8, panelRect.Y + 10, panelRect.Width, panelRect.Height, 28);
        g.FillRoundedRectangle(panelBrush, panelRect.X, panelRect.Y, panelRect.Width, panelRect.Height, 28);

        var guideButtonBounds = new RectangleF(view.ClientSize.Width - 230, 28, 170, 48);
        DrawMenuButton(g, guideButtonBounds, "Управление", secondaryBrush, textBrush, buttonFont);
        g.DrawString("Bird Volleyball", titleFont, textBrush, panelRect.X + 88, panelRect.Y + 46);

        if (state.ScreenState == ScreenState.MainMenu)
        {
            var startButtonBounds = new RectangleF(view.ClientSize.Width / 2f - 150, view.ClientSize.Height / 2f + 40, 300, 72);
            DrawMenuButton(g, startButtonBounds, "Начать игру", accentBrush, textBrush, buttonFont);
        }
        else
        {
            var singleModeButtonBounds = new RectangleF(view.ClientSize.Width / 2f - 180, view.ClientSize.Height / 2f + 10, 360, 68);
            var friendModeButtonBounds = new RectangleF(view.ClientSize.Width / 2f - 180, view.ClientSize.Height / 2f + 94, 360, 68);
            var backButtonBounds = new RectangleF(view.ClientSize.Width / 2f - 120, view.ClientSize.Height / 2f + 188, 240, 52);

            DrawMenuButton(g, singleModeButtonBounds, "Одиночная игра", accentBrush, textBrush, buttonFont);
            DrawMenuButton(g, friendModeButtonBounds, "Игра с другом", secondaryBrush, textBrush, buttonFont);
            DrawMenuButton(g, backButtonBounds, "Назад", panelBrush, textBrush, smallFont);
        }

        if (state.ScreenState == ScreenState.MainMenu)
        {
            g.FillRectangle(panelBrush, panelRect.X + 60, panelRect.Y + 112, 500, 40);
        }

        if (state.GuideVisible)
        {
            DrawGuideOverlay(g, view);
        }
    }

    // Отрисовка кнопки меню
    private static void DrawMenuButton(Graphics g, RectangleF rect, string text, Brush buttonBrush, Brush textBrush, Font font)
    {
        using var shadowBrush = new SolidBrush(Color.FromArgb(36, 23, 43, 61));
        g.FillRoundedRectangle(shadowBrush, rect.X + 4, rect.Y + 6, rect.Width, rect.Height, 24);
        g.FillRoundedRectangle(buttonBrush, rect.X, rect.Y, rect.Width, rect.Height, 24);
        var size = g.MeasureString(text, font);
        g.DrawString(text, font, textBrush, rect.X + (rect.Width - size.Width) / 2f, rect.Y + (rect.Height - size.Height) / 2f - 1f);
    }

    // Отрисовка подсказок управления
    private static void DrawGuideOverlay(Graphics g, IGameView view)
    {
        using var overlayBrush = new SolidBrush(Color.FromArgb(150, 17, 28, 38));
        using var panelBrush = new SolidBrush(Color.FromArgb(242, 255, 255, 255));
        using var textBrush = new SolidBrush(Color.FromArgb(24, 51, 82));
        using var accentBrush = new SolidBrush(Color.FromArgb(255, 204, 87));
        using var titleFont = new Font("Segoe UI", 24, FontStyle.Bold);
        using var lineFont = new Font("Segoe UI", 13, FontStyle.Regular);
        using var closeFont = new Font("Segoe UI", 12, FontStyle.Bold);

        g.FillRectangle(overlayBrush, view.ClientRectangle);
        var panelRect = new RectangleF(view.ClientSize.Width / 2f - 280, view.ClientSize.Height / 2f - 180, 560, 360);
        g.FillRoundedRectangle(panelBrush, panelRect.X, panelRect.Y, panelRect.Width, panelRect.Height, 26);

        var guideCloseButtonBounds = new RectangleF(view.ClientSize.Width / 2f + 180, view.ClientSize.Height / 2f - 165, 84, 40);
        g.FillRoundedRectangle(accentBrush, guideCloseButtonBounds.X, guideCloseButtonBounds.Y, guideCloseButtonBounds.Width, guideCloseButtonBounds.Height, 18);
        g.DrawString("Закрыть", closeFont, textBrush, guideCloseButtonBounds.X + 10, guideCloseButtonBounds.Y + 9);

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

    // Границы кнопок меню (для обработки кликов)
    public static RectangleF GetGuideButtonBounds(IGameView view) => new(view.ClientSize.Width - 230, 28, 170, 48);
    public static RectangleF GetGuideCloseButtonBounds(IGameView view) => new(view.ClientSize.Width / 2f + 180, view.ClientSize.Height / 2f - 165, 84, 40);
    public static RectangleF GetStartButtonBounds(IGameView view) => new(view.ClientSize.Width / 2f - 150, view.ClientSize.Height / 2f + 40, 300, 72);
    public static RectangleF GetSingleModeButtonBounds(IGameView view) => new(view.ClientSize.Width / 2f - 180, view.ClientSize.Height / 2f + 10, 360, 68);
    public static RectangleF GetFriendModeButtonBounds(IGameView view) => new(view.ClientSize.Width / 2f - 180, view.ClientSize.Height / 2f + 94, 360, 68);
    public static RectangleF GetBackButtonBounds(IGameView view) => new(view.ClientSize.Width / 2f - 120, view.ClientSize.Height / 2f + 188, 240, 52);
}