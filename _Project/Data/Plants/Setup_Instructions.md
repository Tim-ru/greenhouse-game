# Инструкция по настройке системы растений

## 🔧 Создание assets через Unity Editor

### 1. Создание PlantData:
```
1. В Project окне перейдите в папку Assets/_Project/Data/Plants/
2. Правый клик → Create → Greenhouse → Plant Data
3. Назовите файл "TomatoPlant" или "LettucePlant"
4. Настройте параметры:
   - tempComfort: (18, 26) для помидора, (15, 22) для салата
   - humidityComfort: (0.4, 0.7) для помидора, (0.5, 0.8) для салата
   - waterConsumptionRate: 0.08 для помидора, 0.12 для салата
   - damagePerSecondOutside: 0.05
   - stageThresholds: [0.2, 0.5, 0.8, 1]
```

### 2. Создание PotData:
```
1. Правый клик → Create → Greenhouse → Pot Data
2. Назовите файл "BasicPot"
3. Настройте параметры:
   - waterCapacity: 1
   - waterRefillAmount: 0.5
   - waterRetentionTime: 10
   - canHoldPlant: true
```

### 3. Создание SeedInventory:
```
1. Правый клик → Create → Greenhouse → Seed Inventory
2. Назовите файл "PlayerSeedInventory"
3. Добавьте семена в список seeds
```

## 🔧 Как настроить в Unity:

### 1. Создание горшка в сцене:
```
1. Создайте пустой GameObject
2. Добавьте компоненты:
   - SpriteRenderer (для отображения)
   - Collider2D (для взаимодействия)
   - PotEntity (наш скрипт)
3. В PotEntity назначьте:
   - Pot Data: BasicPot.asset
```

### 2. Настройка растений:
```
1. Найдите TomatoPlant.asset или LettucePlant.asset
2. В Inspector добавьте спрайты в stageSprites:
   - [0] - семя/росток
   - [1] - молодое растение
   - [2] - взрослое растение
   - [3] - цветущее растение
```

### 3. Настройка инвентаря:
```
1. Найдите PlayerSeedInventory.asset
2. В Inspector в списке seeds добавьте ссылки на растения
3. Установите количество семян
```

### 4. Создание UI для выбора семян:
```
1. Создайте Canvas
2. Добавьте панель для выбора семян
3. Добавьте SeedSelectionUI компонент
4. Назначьте PlayerSeedInventory в поле inventory
```

## 🎮 Тестирование:
1. Запустите сцену
2. Подойдите к горшку
3. Нажмите кнопку взаимодействия
4. Выберите семя для посадки
5. Поливайте растение по мере необходимости

## 📝 Примечания:
- Спрайты растений нужно добавить вручную в Unity Inspector
- Система автоматически найдет все горшки в сцене
- Растения будут расти автоматически при подходящих условиях
