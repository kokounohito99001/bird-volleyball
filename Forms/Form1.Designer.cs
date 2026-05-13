namespace BirdVolleyball;

partial class Form1
{
    /// <summary>
    /// Обязательная переменная для дизайнера
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Освобождение ресурсов
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Код, созданный конструктором форм Windows

    /// <summary>
    /// Требуемый метод для поддержки конструктора — не изменяйте
    /// содержимое этого метода в редакторе кода
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(232, 245, 255);
        ClientSize = new Size(1280, 720);
        DoubleBuffered = true;           // Включение двойной буферизации
        FormBorderStyle = FormBorderStyle.FixedSingle;  // Фиксированный размер окна
        KeyPreview = true;               // Форма получает клавиши перед контролами
        MaximizeBox = false;             // Отключение кнопки развёртывания
        StartPosition = FormStartPosition.CenterScreen;  // Центрирование при запуске
        Text = "Bird Volleyball";        // Заголовок окна
    }

    #endregion
}