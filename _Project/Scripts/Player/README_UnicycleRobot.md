# Система движения одноколесного робота

## Обзор

Эта система обеспечивает реалистичное движение одноколесного робота с плавным замедлением, балансировкой и визуальными эффектами.

## Компоненты

### 1. BeltMover (обновлен)
Основной компонент движения с плавным замедлением.

**Настройки:**
- `maxSpeed` (4f) - Максимальная скорость
- `acceleration` (8f) - Скорость ускорения
- `deceleration` (6f) - Скорость замедления
- `friction` (0.9f) - Трение (0-1, где 1 = нет трения)
- `airResistance` (0.95f) - Сопротивление воздуха (0-1, где 1 = нет сопротивления)

### 2. UnicycleRobotController
Продвинутый контроллер с балансировкой.

**Настройки:**
- `balanceForce` (10f) - Сила балансировки
- `balanceDamping` (5f) - Демпфирование балансировки
- `maxTiltAngle` (30f) - Максимальный угол наклона
- `wheelRadius` (0.5f) - Радиус колеса
- `wheelMass` (1f) - Масса колеса

### 3. RobotVisualEffects
Визуальные эффекты и звуки.

**Настройки:**
- `trailLength` (0.5f) - Длина следа от колеса
- `dustEmissionRate` (10f) - Скорость эмиссии пыли
- `pitchRange` (0.5f) - Диапазон изменения высоты звука

## Настройка

### Базовая настройка
1. Добавьте `BeltMover` к объекту игрока
2. Настройте параметры движения в инспекторе
3. Установите границы области ходьбы

### Продвинутая настройка
1. Добавьте `UnicycleRobotController` к объекту игрока
2. Настройте параметры балансировки
3. Добавьте визуальные элементы (колесо, тело) в соответствующие поля

### Визуальные эффекты
1. Добавьте `RobotVisualEffects` к объекту игрока
2. Настройте TrailRenderer для следа от колеса
3. Добавьте ParticleSystem для пыли и искр
4. Настройте AudioSource для звуков

## API

### BeltMover
```csharp
// Получить текущую скорость
Vector2 velocity = beltMover.GetCurrentVelocity();
float speed = beltMover.GetCurrentSpeed();

// Установить параметры движения
beltMover.SetMovementSettings(maxSpeed, acceleration, deceleration);

// Установить параметры трения
beltMover.SetFrictionSettings(friction, airResistance);

// Применить импульс
beltMover.ApplyImpulse(impulseVector);

// Остановить робота
beltMover.Stop();
```

### UnicycleRobotController
```csharp
// Получить текущий наклон
float tilt = robotController.GetCurrentTilt();

// Получить угловую скорость колеса
float angularVelocity = robotController.GetWheelAngularVelocity();

// Проверить баланс
bool isBalanced = robotController.IsBalanced();

// Применить толчок
robotController.ApplyPush(force, direction);
```

## События

### UnicycleRobotController
- `OnWheelSpin` - При вращении колеса
- `OnTiltChanged` - При изменении наклона
- `OnBalanceLost` - При потере баланса

## Рекомендации по настройке

### Для реалистичного движения:
- `friction`: 0.85-0.95
- `airResistance`: 0.90-0.98
- `deceleration`: 4-8

### Для аркадного движения:
- `friction`: 0.95-0.99
- `airResistance`: 0.98-1.0
- `deceleration`: 8-12

### Для балансировки:
- `balanceForce`: 8-15
- `balanceDamping`: 3-8
- `maxTiltAngle`: 20-45

## Отладка

### Gizmos
- Зеленый куб: границы области ходьбы
- Красная стрелка: текущая скорость
- Желтая линия: направление наклона
- Синий круг: радиус колеса

### Консоль
Используйте методы `GetCurrentVelocity()` и `GetCurrentSpeed()` для отладки движения.

## Производительность

Система оптимизирована для использования в FixedUpdate и не создает лишних аллокаций. Все вычисления выполняются с использованием кэшированных значений.


