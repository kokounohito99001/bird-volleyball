namespace BirdVolleyball.Views;

// Интерфейс для отрисовки — позволяет рендерить в любой объект с графикой
public interface IGameView
{
    Rectangle ClientRectangle { get; }  // Прямоугольник клиентской области
    Size ClientSize { get; }            // Размер окна
    void Invalidate();                  // Метод для перерисовки
}