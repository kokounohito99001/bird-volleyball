namespace BirdVolleyball;

static class Program
{
    /// <summary>
    /// Точка входа в приложение
    /// </summary>
    [STAThread]
    static void Main()
    {
        // Инициализация конфигурации приложения (DPI, шрифты и т.д.)
        ApplicationConfiguration.Initialize();

        // Запуск главной формы
        Application.Run(new Form1());
    }
}