# Система триггеров камеры

Эта система позволяет автоматически перемещать камеру когда игрок достигает определенных зон (левая и правая дверь).

## Компоненты системы

### 1. CameraController.cs
Основной контроллер камеры с плавным перемещением между позициями.

**Настройки:**
- `moveSpeed` - скорость перемещения камеры
- `smoothTime` - время плавного перемещения
- `leftDoorPosition` - позиция камеры у левой двери
- `rightDoorPosition` - позиция камеры у правой двери
- `centerPosition` - центральная позиция камеры

### 2. CameraTrigger.cs
Триггер для перемещения камеры при входе игрока в зону.

**Настройки:**
- `triggerType` - тип триггера (левая дверь, правая дверь, центр)
- `requirePlayerTag` - требовать тег игрока
- `playerTag` - тег игрока
- `cameraController` - ссылка на контроллер камеры

### 3. CameraTriggerSetup.cs
Утилита для автоматической настройки триггеров.

## Настройка системы

### Шаг 1: Настройка камеры
1. Выберите Main Camera в сцене
2. Добавьте компонент `CameraController`
3. Настройте позиции камеры:
   - `leftDoorPosition` - позиция у левой двери
   - `rightDoorPosition` - позиция у правой двери
   - `centerPosition` - центральная позиция

### Шаг 2: Автоматическая настройка триггеров
1. Создайте пустой GameObject в сцене
2. Добавьте компонент `CameraTriggerSetup`
3. Настройте параметры:
   - `cameraController` - ссылка на CameraController
   - `leftDoorTriggerOffset` - смещение триггера левой двери
   - `rightDoorTriggerOffset` - смещение триггера правой двери
   - `leftDoorTriggerSize` - размер триггера левой двери
   - `rightDoorTriggerSize` - размер триггера правой двери
4. Нажмите "Setup Camera Triggers" в контекстном меню

### Шаг 3: Ручная настройка триггеров (альтернатива)
1. Создайте два пустых GameObject для триггеров
2. Добавьте компонент `BoxCollider2D` (настройте как Trigger)
3. Добавьте компонент `CameraTrigger`
4. Настройте:
   - `triggerType` - LeftDoor или RightDoor
   - `cameraController` - ссылка на CameraController

## Использование

### Перемещение камеры
```csharp
// Получить ссылку на контроллер камеры
CameraController cameraController = FindObjectOfType<CameraController>();

// Переместить к левой двери
cameraController.MoveToLeftDoor();

// Переместить к правой двери
cameraController.MoveToRightDoor();

// Вернуть в центр
cameraController.MoveToCenter();
```

### Проверка состояния камеры
```csharp
// Проверить, движется ли камера
bool isMoving = cameraController.IsMoving;

// Получить целевую позицию
Vector3 targetPos = cameraController.TargetPosition;
```

## Принципы SOLID

### Single Responsibility Principle (SRP)
- `CameraController` - отвечает только за управление камерой
- `CameraTrigger` - отвечает только за обнаружение входа игрока
- `CameraTriggerSetup` - отвечает только за настройку системы

### Open/Closed Principle (OCP)
- Система легко расширяется новыми типами триггеров
- Можно добавить новые позиции камеры без изменения существующего кода

### Liskov Substitution Principle (LSP)
- Все триггеры взаимозаменяемы через общий интерфейс

### Interface Segregation Principle (ISP)
- Каждый компонент имеет только необходимые методы
- Нет избыточных зависимостей

### Dependency Inversion Principle (DIP)
- Компоненты зависят от абстракций, а не от конкретных реализаций
- Легко заменить реализацию без изменения кода

## Отладка

### Визуализация триггеров
Триггеры автоматически создаются с полупрозрачными красными индикаторами для отладки.

### Логирование
Все действия системы логируются в консоль Unity для отладки.

### Проверка настройки
1. Убедитесь, что у игрока есть тег "Player"
2. Проверьте, что триггеры имеют правильные размеры и позиции
3. Убедитесь, что CameraController назначен на Main Camera

