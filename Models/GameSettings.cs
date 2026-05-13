namespace BirdVolleyball.Models;

public static class GameSettings
{
    // Физика
    public const float Gravity = 980f;
    public const float BirdMoveSpeed = 390f;
    public const float BirdJumpSpeed = -690f;
    public const float BotMoveSpeed = 455f;

    // Размеры арены
    public const float GroundHeight = 92f;
    public const float NetHeight = 120f;
    public const float NetWidth = 18f;

    // Параметры мяча
    public const float BallRadius = 18f;
    public const float BallAirDrag = 0.994f;
    public const float BallBounce = 0.9f;
    public const float BallMaxSpeedX = 640f;
    public const float BallMaxSpeedY = 820f;

    // Подача и удары
    public const float ServeHoverOffsetY = 78f;
    public const float ServeLaunchSpeedX = 280f;
    public const float ServeLaunchSpeedY = -360f;
    public const float AutoReturnHorizontalSpeed = 300f;
    public const float AutoReturnVerticalSpeed = -420f;
    public const float AutoSmashHorizontalSpeed = 580f;  // Мощный удар
    public const float AutoSmashVerticalSpeed = -650f;   // Сильный отскок

    // Условия победы
    public const int WinningScore = 7;
    public const float BirdWidth = 74f;
    public const float BirdHeight = 72f;
}