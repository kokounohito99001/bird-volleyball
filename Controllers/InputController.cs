using BirdVolleyball.Models;

namespace BirdVolleyball.Controllers;

public sealed class InputController
{
    private readonly GameState gameState;
    private readonly ControlBindings controls;
    private readonly HashSet<Keys> pressedKeys = [];

    public InputController(GameState gameState, ControlBindings controls)
    {
        this.gameState = gameState;
        this.controls = controls;
    }

    // Обработка нажатия клавиши
    public void HandleKeyDown(Keys keyCode)
    {
        pressedKeys.Add(keyCode);

        if (gameState.ScreenState != ScreenState.Playing)
        {
            HandleMenuInput(keyCode);
            return;
        }

        if (gameState.GameFinished && controls.IsPressed(ControlAction.ResetRound, pressedKeys))
        {
            OnResetGameRequested?.Invoke();
        }
    }

    // Обработка отпускания клавиши
    public void HandleKeyUp(Keys keyCode)
    {
        pressedKeys.Remove(keyCode);
    }

    // Получение копии набора нажатых клавиш
    public HashSet<Keys> GetPressedKeys() => new(pressedKeys);

    // Обработка клавиш в меню
    private void HandleMenuInput(Keys keyCode)
    {
        if (gameState.GuideVisible && keyCode == Keys.Escape)
        {
            gameState.GuideVisible = false;
            return;
        }

        if (keyCode == Keys.G)
        {
            gameState.GuideVisible = !gameState.GuideVisible;
            return;
        }

        if (gameState.ScreenState == ScreenState.MainMenu && (keyCode == Keys.Enter || keyCode == Keys.Space))
        {
            gameState.ScreenState = ScreenState.ModeSelect;
            return;
        }

        if (gameState.ScreenState == ScreenState.ModeSelect && keyCode == Keys.Escape)
        {
            gameState.ScreenState = ScreenState.MainMenu;
            gameState.GuideVisible = false;
        }
    }

    public event Action? OnResetGameRequested;
}