# 🐦 Bird Volleyball - Setup Guide

## Требования
- Unity 2021.3 LTS или новее
- TextMeshPro (устанавливается через Package Manager)

## Установка

### 1. Импорт проекта в Unity
```
1. Откройте Unity Hub
2. Нажмите "Add" и выберите папку с проектом
3. Дождитесь импорта ассетов
```

### 2. Настройка сцены

#### Создайте следующую иерархию объектов:

```
Scene
├── GameManager (пустой объект со скриптом GameManager)
│   └── AudioManager (добавьте как компонент или отдельный объект)
├── ControlsManager (пустой объект)
├── Court (пустой объект со скриптом CourtBoundary)
│   ├── Ground (Box Collider 2D, слой Ground)
│   └── Net (Box Collider 2D, слой Net)
├── PlayerOne (спрайт птицы)
│   ├── Rigidbody2D
│   ├── Box Collider 2D (слой Bird)
│   ├── BirdController
│   └── GroundCheck (пустой дочерний объект для проверки земли)
├── PlayerTwo/AI (спрайт птицы)
│   ├── Rigidbody2D
│   ├── Box Collider 2D (слой Bird)
│   ├── BirdController (isPlayerOne = false)
│   ├── BirdAI
│   └── GroundCheck
├── Ball (спрайт мяча)
│   ├── Rigidbody2D (Gravity Scale = 2.5)
│   ├── Circle Collider 2D
│   └── BallController
└── Canvas (UI)
    ├── ScorePanel
    │   ├── PlayerOneScore (TextMeshPro)
    │   └── PlayerTwoScore (TextMeshPro)
    ├── CountdownText (TextMeshPro)
    └── GameOverPanel
        └── WinnerText (TextMeshPro)
```

### 3. Настройка слоёв (Layers)

Создайте следующие слои:
- `Bird` (слой 6)
- `Ground` (слой 7)
- `Net` (слой 8)
- `Ball` (слой 9)

### 4. Настройка Physics 2D

В Edit → Project Settings → Physics 2D:
- Отключите collision между слоем Bird и Bird
- Включите collision между Bird и Ball
- Включите collision между Ball и Ground/Net

### 5. Настройка Controls.json

Поместите файл `Settings/Controls.json` в папку `Assets/Resources/`:
```
Assets/
└── Resources/
    └── Controls.json
```

Или измените путь в скрипте `ControlsManager.cs`.

### 6. Настройка Audio

Добавьте AudioSource компоненты и создайте AudioClip для:
- BallHit
- NetHit
- GameOver
- Whistle

## Управление по умолчанию

| Действие | Клавиша |
|----------|---------|
| Движение влево | A / ← |
| Движение вправо | D / → |
| Нижний приём | Пробел / ЛКМ |
| Удар сверху (смэш) | Shift / ПКМ |
| Сброс раунда | R |

## Сборка проекта

1. File → Build Settings
2. Добавьте сцену MainScene
3. Выберите платформу (Windows/Mac/Linux)
4. Нажмите Build

## Troubleshooting

### Мяч проваливается сквозь пол
- Проверьте наличие Box Collider 2D на Ground
- Убедитесь, что у мяча есть Circle Collider 2D

### Птица не двигается
- Проверьте наличие Rigidbody 2D
- Убедитесь, что Frozen Rotation Z включён в Rigidbody2D

### Контролы не работают
- Проверьте путь к Controls.json
- Убедитесь, что ControlsManager создан на сцене

## Кастомизация

### Изменение физики
Отредактируйте `Config/GameConfig.json` для настройки:
- Силы ударов
- Скорости движения
- Параметров ИИ

### Добавление спрайтов
Поместите ваши спрайты в `Assets/Sprites/` и назначьте их на соответствующие объекты.

---

🎮 Приятной игры!
