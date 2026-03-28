# CoffeeShop — Информационная система автоматизации кофейни

**Студент:** Лагошина Ева Сергеевна
**Группа:** ИСП-23-2
**Преподаватель:** Кубанева Е.А.
**Дата:** 2026-03-23

---

## 1. Обзор проекта

### Назначение

CoffeeShop — настольная информационная система для автоматизации операционной деятельности кофейни. Система охватывает полный цикл работы заведения: от регистрации клиента и оформления заказа до закрытия смены и формирования финансового отчёта.

Основная задача системы — устранить ручной учёт заказов, автоматизировать программу лояльности для клиентов (накопление и списание бонусных баллов) и обеспечить прозрачное начисление вознаграждений персоналу. Каждый бариста получает процент от суммы именно тех заказов, которые он принял лично; администратор получает меньший процент от общей выручки смены.

Система разработана в рамках курсового проекта по специальности 09.02.07 «Информационные системы и программирование», СТИ МИСИС (Оскольский политехнический колледж).

### Цель разработки

Разработать настольное WPF-приложение, которое заменяет бумажный учёт заказов и ручной расчёт вознаграждений персонала в небольшой кофейне, а также реализует программу лояльности (накопление бонусов по ставке 5% от суммы заказа, списание из расчёта 1 бонус = 1 рубль).

### Целевая аудитория

- **Клиенты кофейни** — самостоятельное оформление заказов, отслеживание программы лояльности
- **Бариста** — приём и управление статусами заказов
- **Администратор** — управление меню, персоналом и сменами, формирование отчётности

---

## 2. Техническое задание

### 2.1 Роли пользователей

| Роль | Описание | Права доступа |
|------|----------|---------------|
| Клиент | Зарегистрированный посетитель кофейни | Просмотр меню, оформление заказа, управление бонусами, отслеживание статуса |
| Бариста | Сотрудник кофейни | Просмотр входящих заказов, изменение статуса, просмотр своих начислений |
| Администратор | Управляющий кофейней | Полный доступ: меню, персонал, смены, отчётность, экспорт |

### 2.2 Функциональные требования

#### Клиент
- Регистрация по номеру телефона и паролю
- Вход в систему
- Просмотр доступных позиций меню (с категориями и ценами)
- Оформление заказа с выбором позиций и их количества
- Списание бонусных баллов при оформлении (1 бонус = 1 рубль)
- Отслеживание статуса заказа: `Принят → В процессе → Готов → Получен`
- Просмотр истории заказов и баланса бонусного счёта
- Автоматическое начисление 5% от суммы заказа в виде бонусов после его выполнения

#### Бариста
- Вход в систему
- Просмотр списка актуальных заказов (со статусом «Принят»)
- Принятие заказа в работу (статус «В процессе»)
- Изменение статуса: «Готов», «Получен»
- Просмотр своих начислений за текущую смену

#### Администратор
- Вход в систему
- Регистрация новых учётных записей бариста
- Деактивация учётных записей бариста
- Скрытие и отображение позиций меню (при отсутствии товара)
- Открытие и закрытие смены
- Просмотр отчёта по смене: количество заказов, выручка, начисления каждому бариста (10%) и администратору (3%)
- Экспорт отчёта смены в формате Excel

### 2.3 Нефункциональные требования

**Производительность:**
- Загрузка списка заказов и меню — не более 2 секунд
- Интерфейс не блокируется при обращении к БД (async/await)

**Безопасность:**
- Пароли хранятся в виде BCrypt-хеша (cost factor 11)
- Доступ к разделам ограничен по роли пользователя
- Параметризованные запросы через EF Core (защита от SQL-инъекций)
- Сессия хранится только в памяти приложения

**Надёжность:**
- Финансовые операции выполняются через хранимые процедуры PostgreSQL в транзакциях
- Все изменения статуса заказа логируются в `audit_log`

**Интерфейс:**
- Язык: русский
- Цветовая тема: светлая — бежевый `#F5F0E8`, кофейный `#6F4E37`, белый `#FFFFFF`
- UI-библиотека: MaterialDesignInXAML
- Минимальное разрешение: 1280×720

---

## 3. Архитектура

### 3.1 Паттерн и стек

| Компонент | Технология |
|-----------|------------|
| Архитектурный паттерн | MVVM |
| Язык | C# (.NET 8) |
| UI-фреймворк | WPF |
| ORM | Entity Framework Core 8 |
| СУБД | PostgreSQL 16 |
| UI-библиотека | MaterialDesignInXAML 5.x |
| Хеширование паролей | BCrypt.Net-Next |
| Экспорт в Excel | ClosedXML |
| Провайдер EF Core | Npgsql.EntityFrameworkCore.PostgreSQL |

### 3.2 Структура проекта

```
CoffeeShop/
├── CoffeeShop.sln
└── CoffeeShop/
    ├── App.xaml / App.xaml.cs
    ├── Models/
    │   ├── Role.cs
    │   ├── User.cs
    │   ├── MenuCategory.cs
    │   ├── MenuItem.cs
    │   ├── Order.cs
    │   ├── OrderItem.cs
    │   ├── LoyaltyTransaction.cs
    │   ├── Shift.cs
    │   ├── StaffEarning.cs
    │   └── AuditLog.cs
    ├── ViewModels/
    │   ├── Base/
    │   │   ├── BaseViewModel.cs        ← INotifyPropertyChanged
    │   │   └── RelayCommand.cs         ← ICommand
    │   ├── AuthViewModel.cs
    │   ├── RegisterViewModel.cs
    │   ├── Client/
    │   │   ├── ClientMenuViewModel.cs
    │   │   ├── OrderViewModel.cs
    │   │   └── OrderHistoryViewModel.cs
    │   ├── Barista/
    │   │   └── BaristaOrdersViewModel.cs
    │   └── Admin/
    │       ├── AdminDashboardViewModel.cs
    │       ├── MenuManagementViewModel.cs
    │       ├── StaffManagementViewModel.cs
    │       └── ShiftReportViewModel.cs
    ├── Views/
    │   ├── AuthWindow.xaml
    │   ├── RegisterWindow.xaml
    │   ├── Client/
    │   │   ├── ClientShell.xaml
    │   │   ├── MenuView.xaml
    │   │   ├── OrderStatusView.xaml
    │   │   └── OrderHistoryView.xaml
    │   ├── Barista/
    │   │   └── BaristaOrdersView.xaml
    │   └── Admin/
    │       ├── AdminShell.xaml
    │       ├── MenuManagementView.xaml
    │       ├── StaffManagementView.xaml
    │       └── ShiftReportView.xaml
    ├── Services/
    │   ├── AuthService.cs
    │   ├── OrderService.cs
    │   ├── MenuService.cs
    │   ├── LoyaltyService.cs
    │   ├── ShiftService.cs
    │   └── ExcelExportService.cs
    ├── Data/
    │   ├── AppDbContext.cs
    │   └── Migrations/
    └── Helpers/
        ├── PasswordHasher.cs
        ├── SessionManager.cs
        └── Converters/
            ├── BoolToVisibilityConverter.cs
            └── OrderStatusToColorConverter.cs
```

### 3.3 Схема взаимодействия

```
[View (XAML)]
    ↕  Bindings / Commands
[ViewModel]
    ↕  async/await вызовы методов
[Service Layer]
    ↕  LINQ / DbSet / ExecuteSqlRawAsync
[AppDbContext (EF Core)]
    ↕  Npgsql Driver
[PostgreSQL — триггеры, хранимые процедуры, представления]
```

---

## 4. База данных

### 4.1 Таблицы

**`roles`** — справочник ролей пользователей

| Поле | Тип | Ограничения | Описание |
|------|-----|-------------|----------|
| id | SERIAL | PK | Идентификатор |
| name | VARCHAR(50) | NOT NULL, UNIQUE | Название роли |
| description | TEXT | | Описание |

---

**`users`** — учётные записи всех пользователей системы

| Поле | Тип | Ограничения | Описание |
|------|-----|-------------|----------|
| id | SERIAL | PK | Идентификатор |
| full_name | VARCHAR(200) | NOT NULL | Полное имя |
| phone | VARCHAR(20) | UNIQUE | Номер телефона (логин) |
| email | VARCHAR(200) | UNIQUE | Email (опционально) |
| password_hash | VARCHAR(255) | NOT NULL | BCrypt-хеш пароля |
| role_id | INTEGER | FK → roles(id) | Роль |
| is_active | BOOLEAN | DEFAULT TRUE | Флаг активности |
| bonus_balance | DECIMAL(10,2) | DEFAULT 0 | Баланс бонусных баллов |
| created_at | TIMESTAMP | DEFAULT NOW() | Дата регистрации |
| updated_at | TIMESTAMP | DEFAULT NOW() | Дата обновления |

---

**`menu_categories`** — категории позиций меню

| Поле | Тип | Ограничения | Описание |
|------|-----|-------------|----------|
| id | SERIAL | PK | Идентификатор |
| name | VARCHAR(100) | NOT NULL, UNIQUE | Название |
| sort_order | INTEGER | DEFAULT 0 | Порядок отображения |

---

**`menu_items`** — позиции меню кофейни

| Поле | Тип | Ограничения | Описание |
|------|-----|-------------|----------|
| id | SERIAL | PK | Идентификатор |
| category_id | INTEGER | FK → menu_categories(id) | Категория |
| name | VARCHAR(200) | NOT NULL | Название |
| description | TEXT | | Описание |
| price | DECIMAL(10,2) | NOT NULL, CHECK > 0 | Цена |
| is_available | BOOLEAN | DEFAULT TRUE | Доступность |
| image_path | VARCHAR(500) | | Путь к изображению |
| created_at | TIMESTAMP | DEFAULT NOW() | Дата добавления |
| updated_at | TIMESTAMP | DEFAULT NOW() | Дата обновления |

---

**`shifts`** — рабочие смены

| Поле | Тип | Ограничения | Описание |
|------|-----|-------------|----------|
| id | SERIAL | PK | Идентификатор |
| opened_by | INTEGER | FK → users(id) | Кто открыл смену |
| opened_at | TIMESTAMP | DEFAULT NOW() | Время открытия |
| closed_at | TIMESTAMP | | Время закрытия |
| is_closed | BOOLEAN | DEFAULT FALSE | Флаг закрытия |
| total_revenue | DECIMAL(10,2) | DEFAULT 0 | Общая выручка смены |
| created_at | TIMESTAMP | DEFAULT NOW() | Дата создания |
| updated_at | TIMESTAMP | DEFAULT NOW() | Дата обновления |

---

**`orders`** — заказы клиентов

| Поле | Тип | Ограничения | Описание |
|------|-----|-------------|----------|
| id | SERIAL | PK | Идентификатор |
| client_id | INTEGER | FK → users(id) | Клиент |
| barista_id | INTEGER | FK → users(id), NULL | Принявший бариста |
| shift_id | INTEGER | FK → shifts(id), NULL | Смена |
| status | VARCHAR(50) | NOT NULL, DEFAULT 'pending' | Статус заказа |
| total_amount | DECIMAL(10,2) | DEFAULT 0 | Сумма до списания бонусов |
| bonus_used | DECIMAL(10,2) | DEFAULT 0 | Списано бонусов |
| final_amount | DECIMAL(10,2) | DEFAULT 0 | Итоговая сумма к оплате |
| created_at | TIMESTAMP | DEFAULT NOW() | Время создания |
| updated_at | TIMESTAMP | DEFAULT NOW() | Время обновления |

Допустимые статусы: `pending` → `in_progress` → `ready` → `completed`

---

**`order_items`** — позиции заказа

| Поле | Тип | Ограничения | Описание |
|------|-----|-------------|----------|
| id | SERIAL | PK | Идентификатор |
| order_id | INTEGER | FK → orders(id) ON DELETE CASCADE | Заказ |
| menu_item_id | INTEGER | FK → menu_items(id) | Позиция меню |
| quantity | INTEGER | NOT NULL, DEFAULT 1 | Количество |
| unit_price | DECIMAL(10,2) | NOT NULL | Цена на момент заказа |
| subtotal | DECIMAL(10,2) | NOT NULL | Сумма позиции |
| created_at | TIMESTAMP | DEFAULT NOW() | Дата добавления |

---

**`loyalty_transactions`** — транзакции бонусной программы

| Поле | Тип | Ограничения | Описание |
|------|-----|-------------|----------|
| id | SERIAL | PK | Идентификатор |
| user_id | INTEGER | FK → users(id) | Клиент |
| order_id | INTEGER | FK → orders(id), NULL | Связанный заказ |
| amount | DECIMAL(10,2) | NOT NULL | Сумма (+ начисление, − списание) |
| transaction_type | VARCHAR(50) | NOT NULL | `accrual` или `redemption` |
| description | TEXT | | Описание транзакции |
| created_at | TIMESTAMP | DEFAULT NOW() | Дата транзакции |

---

**`staff_earnings`** — начисления персоналу по сменам

| Поле | Тип | Ограничения | Описание |
|------|-----|-------------|----------|
| id | SERIAL | PK | Идентификатор |
| user_id | INTEGER | FK → users(id) | Сотрудник |
| shift_id | INTEGER | FK → shifts(id) | Смена |
| orders_count | INTEGER | DEFAULT 0 | Количество принятых заказов |
| orders_total | DECIMAL(10,2) | DEFAULT 0 | Сумма принятых заказов |
| earning_percent | DECIMAL(5,2) | NOT NULL | Процент начисления |
| earned_amount | DECIMAL(10,2) | DEFAULT 0 | Начислено рублей |
| created_at | TIMESTAMP | DEFAULT NOW() | Дата создания |
| updated_at | TIMESTAMP | DEFAULT NOW() | Дата обновления |
| | | UNIQUE(user_id, shift_id) | Одна запись на сотрудника за смену |

---

**`audit_log`** — журнал ключевых изменений

| Поле | Тип | Ограничения | Описание |
|------|-----|-------------|----------|
| id | SERIAL | PK | Идентификатор |
| table_name | VARCHAR(100) | NOT NULL | Таблица |
| record_id | INTEGER | NOT NULL | ID записи |
| action | VARCHAR(50) | NOT NULL | Тип действия |
| old_values | JSONB | | Старые значения |
| new_values | JSONB | | Новые значения |
| changed_by | INTEGER | FK → users(id), NULL | Кто изменил |
| created_at | TIMESTAMP | DEFAULT NOW() | Время записи |

**Связи между таблицами:**
- `users` →(N:1)→ `roles`
- `menu_items` →(N:1)→ `menu_categories`
- `shifts` →(N:1)→ `users` (opened_by — администратор)
- `orders` →(N:1)→ `users` (client_id)
- `orders` →(N:1)→ `users` (barista_id)
- `orders` →(N:1)→ `shifts`
- `order_items` →(N:1)→ `orders`
- `order_items` →(N:1)→ `menu_items`
- `loyalty_transactions` →(N:1)→ `users`
- `loyalty_transactions` →(N:1)→ `orders`
- `staff_earnings` →(N:1)→ `users`
- `staff_earnings` →(N:1)→ `shifts`

### 4.2 Индексы

| Индекс | Таблица | Поля | Обоснование |
|--------|---------|------|-------------|
| idx_users_phone | users | phone | Поиск при авторизации |
| idx_users_role_id | users | role_id | Фильтрация по роли |
| idx_orders_client | orders | client_id | История заказов клиента |
| idx_orders_barista | orders | barista_id | Заказы конкретного бариста |
| idx_orders_shift | orders | shift_id | Отчёт по смене |
| idx_orders_status | orders | status | Фильтрация активных заказов |
| idx_order_items_order | order_items | order_id | Позиции заказа |
| idx_loyalty_user | loyalty_transactions | user_id | История бонусов клиента |
| idx_staff_earnings_shift | staff_earnings | shift_id | Отчёт по смене |
| idx_menu_items_category | menu_items | category_id | Меню по категориям |

---

## 5. Экраны и формы

### Экран авторизации
- **Назначение:** Вход в систему
- **Доступно для:** все
- **Элементы:** поле «Телефон», поле «Пароль», кнопка «Войти», ссылка «Зарегистрироваться»
- **Поведение:** проверка логина/пароля через BCrypt; при успехе — переход по роли; при ошибке — сообщение «Неверный телефон или пароль»
- **Навигация:** → Меню клиента / Панель бариста / Панель администратора

### Экран регистрации
- **Назначение:** Создание учётной записи клиента
- **Доступно для:** неаутентифицированный пользователь
- **Элементы:** поле «ФИО», поле «Телефон», поле «Пароль», поле «Повторите пароль», кнопка «Зарегистрироваться»
- **Валидация:** телефон в формате +7XXXXXXXXXX; пароль не менее 6 символов; оба пароля совпадают; телефон не занят
- **Навигация:** → Главный экран клиента

### Главный экран клиента (меню)
- **Назначение:** Просмотр меню и оформление заказа
- **Доступно для:** Клиент
- **Элементы:** вкладки категорий меню, карточки позиций (название, цена, кнопка «Добавить»), корзина (список позиций, итого, поле «Списать бонусы»), кнопка «Оформить заказ», баланс бонусов в шапке
- **Поведение:** поле «Списать бонусы» ограничено текущим балансом; при оформлении — вызов `sp_create_order`
- **Навигация:** → Экран статуса заказа

### Экран статуса заказа
- **Назначение:** Отслеживание состояния текущего заказа
- **Доступно для:** Клиент
- **Элементы:** номер заказа, список позиций, итоговая сумма, прогресс-индикатор с 4 шагами (`Принят → В процессе → Готов → Получен`), баланс бонусов
- **Поведение:** обновление статуса по кнопке или автоматически; при «Получен» — появляется кнопка «Новый заказ»
- **Навигация:** → Главное меню (после завершения)

### История заказов клиента
- **Назначение:** Просмотр прошлых заказов и транзакций бонусов
- **Доступно для:** Клиент
- **Элементы:** таблица заказов (дата, сумма, статус, начислено бонусов), таблица бонусных транзакций
- **Навигация:** из шапки клиентского интерфейса

### Панель бариста
- **Назначение:** Управление очередью заказов
- **Доступно для:** Бариста
- **Элементы:** список активных заказов (номер, время, позиции, сумма, статус), кнопки «Принять», «Готов», «Выдан», блок «Мои начисления за смену»
- **Поведение:** «Принять» → `in_progress`, привязывает бариста к заказу; «Выдан» → вызов `sp_complete_order`
- **Навигация:** единственный экран для роли «Бариста»

### Панель администратора — Дашборд
- **Назначение:** Обзор текущей смены
- **Доступно для:** Администратор
- **Элементы:** KPI-карточки (заказов сегодня, выручка, активные бариста), кнопки навигации в разделы, кнопка «Закрыть смену»
- **Навигация:** → Управление меню / Управление персоналом / Отчёт смены

### Управление меню
- **Назначение:** Скрытие/отображение позиций меню
- **Доступно для:** Администратор
- **Элементы:** таблица позиций (название, категория, цена, доступность), переключатель «Доступно/Скрыто» для каждой позиции, кнопки добавления/редактирования
- **Навигация:** из панели администратора

### Управление персоналом
- **Назначение:** Регистрация и деактивация бариста
- **Доступно для:** Администратор
- **Элементы:** таблица сотрудников (ФИО, телефон, статус), форма добавления нового бариста, кнопка «Деактивировать»
- **Поведение:** деактивированный бариста не может войти (`is_active = FALSE`)
- **Навигация:** из панели администратора

### Отчёт смены
- **Назначение:** Финансовый итог смены и экспорт
- **Доступно для:** Администратор
- **Элементы:** итоги смены (всего заказов, выручка, списано бонусов), таблица начислений персоналу (ФИО, кол-во заказов, сумма заказов, % ставка, начислено), кнопки «Закрыть смену» и «Экспорт в Excel»
- **Поведение:** «Закрыть смену» → вызов `sp_close_shift`; экспорт формирует .xlsx через ClosedXML
- **Навигация:** из панели администратора

---

## 6. UML-диаграммы (описание)

### 6.1 Use Case диаграмма

**Акторы:** Клиент, Бариста, Администратор, Система

**Варианты использования:**
- `Войти в систему` (include → `Проверить учётные данные`) — все роли
- `Зарегистрироваться` — Клиент
- `Просмотреть меню` — Клиент
- `Оформить заказ` (include → `Рассчитать итог`) — Клиент
- `Списать бонусы` (extend → `Оформить заказ`) — Клиент
- `Отследить статус заказа` — Клиент
- `Просмотреть историю заказов` — Клиент
- `Просмотреть входящие заказы` — Бариста
- `Изменить статус заказа` — Бариста
- `Завершить заказ` (include → `Начислить бонусы клиенту`, include → `Начислить % бариста`) — Бариста
- `Управлять меню` — Администратор
- `Управлять персоналом` — Администратор
- `Закрыть смену` (include → `Начислить % администратору`) — Администратор
- `Экспортировать отчёт в Excel` — Администратор

### 6.2 Sequence диаграмма — Оформление заказа клиентом

**Участники:** Клиент, MenuView, OrderViewModel, OrderService, AppDbContext, PostgreSQL

1. Клиент добавляет позиции в корзину → `OrderViewModel.AddItem(menuItemId)`
2. OrderViewModel обновляет `ObservableCollection<CartItem>` и пересчитывает итог
3. Клиент вводит сумму списания бонусов (опционально)
4. Клиент нажимает «Оформить заказ» → `OrderViewModel.PlaceOrderCommand`
5. OrderViewModel → `OrderService.CreateOrderAsync(clientId, items, bonusUsed)`
6. OrderService → `CALL sp_create_order(...)` через `AppDbContext.Database.ExecuteSqlRawAsync`
7. PostgreSQL: создаёт запись в `orders`, добавляет `order_items`, списывает бонусы из `users.bonus_balance`, записывает транзакцию в `loyalty_transactions`
8. PostgreSQL → OrderService: возвращает `orderId`
9. OrderViewModel навигирует на `OrderStatusView` с `orderId`

### 6.3 Activity диаграмма — Жизненный цикл заказа

**Swimlanes:** Клиент | Система | Бариста | Администратор

1. **Клиент:** Открывает меню → Добавляет позиции → Оформляет заказ
2. **Система:** Создаёт запись `orders` со статусом `pending`; привязывает к текущей смене
3. **Бариста:** Видит заказ [pending] → Принимает в работу → [in_progress]
4. **Бариста:** Готовит напиток → Меняет статус → [ready]
5. **Клиент:** Видит «Готов» → Забирает заказ
6. **Бариста:** Выдаёт заказ → [completed]
7. **Система:** Вызывает `sp_complete_order` → начисляет 5% бонусов клиенту → начисляет 10% бариста в `staff_earnings`
8. **Администратор:** В конце смены нажимает «Закрыть смену» → **Система** вызывает `sp_close_shift` → начисляет администратору 3% от выручки

### 6.4 ER-диаграмма

```
roles (PK: id)
  ↑ 1:N
users (PK: id, FK: role_id)
  ├── 1:N → orders (client_id)
  ├── 1:N → orders (barista_id)
  ├── 1:N → shifts (opened_by)
  ├── 1:N → staff_earnings (user_id)
  └── 1:N → loyalty_transactions (user_id)

menu_categories (PK: id)
  ↑ 1:N
menu_items (PK: id, FK: category_id)
  ↑ 1:N
order_items (PK: id, FK: menu_item_id, FK: order_id)
  ↑ N:1
orders (PK: id, FK: client_id, FK: barista_id, FK: shift_id)
  ├── 1:N → order_items
  └── 1:N → loyalty_transactions

shifts (PK: id, FK: opened_by)
  ├── 1:N → orders
  └── 1:N → staff_earnings

audit_log (PK: id, FK: changed_by → users)
```

### 6.5 Диаграмма классов

**Пакет Models:**
- `Role { Id, Name, Description }`
- `User { Id, FullName, Phone, Email, PasswordHash, RoleId, IsActive, BonusBalance, CreatedAt, UpdatedAt }`
- `MenuCategory { Id, Name, SortOrder }`
- `MenuItem { Id, CategoryId, Name, Description, Price, IsAvailable, ImagePath }`
- `Order { Id, ClientId, BaristaId, ShiftId, Status, TotalAmount, BonusUsed, FinalAmount, CreatedAt, UpdatedAt }`
- `OrderItem { Id, OrderId, MenuItemId, Quantity, UnitPrice, Subtotal }`
- `LoyaltyTransaction { Id, UserId, OrderId, Amount, TransactionType, Description, CreatedAt }`
- `Shift { Id, OpenedBy, OpenedAt, ClosedAt, IsClosed, TotalRevenue }`
- `StaffEarning { Id, UserId, ShiftId, OrdersCount, OrdersTotal, EarningPercent, EarnedAmount }`
- `AuditLog { Id, TableName, RecordId, Action, OldValues, NewValues, ChangedBy, CreatedAt }`

**Пакет Services:**
- `AuthService { Login(phone, password): User?, Register(dto): User }`
- `OrderService { CreateOrderAsync(clientId, items, bonusUsed): int, GetOrderStatusAsync(orderId): string, UpdateStatusAsync(orderId, status, baristaId) }`
- `MenuService { GetAvailableItemsAsync(): List<MenuItem>, GetAllItemsAsync(): List<MenuItem>, ToggleAvailabilityAsync(id) }`
- `LoyaltyService { GetBalanceAsync(userId): decimal, GetTransactionsAsync(userId): List<LoyaltyTransaction> }`
- `ShiftService { OpenShiftAsync(adminId): Shift, CloseShiftAsync(shiftId, adminId), GetReportAsync(shiftId): ShiftReport }`
- `ExcelExportService { ExportShiftReportAsync(shiftId, filePath) }`

**Пакет Data:**
- `AppDbContext : DbContext` — содержит `DbSet<>` для всех сущностей, конфигурацию через Fluent API

---

## 7. Список скриншотов

| № | Файл | Что снять | Под какой ролью | Примечание |
|---|------|-----------|-----------------|------------|
| 1 | screenshot_01_auth.png | Экран авторизации | — | Форма входа |
| 2 | screenshot_02_register.png | Экран регистрации | — | Форма с заполненными полями |
| 3 | screenshot_03_client_menu.png | Главный экран клиента | Клиент | Категории и карточки позиций |
| 4 | screenshot_04_client_cart.png | Корзина с позициями | Клиент | Список заказа, итого, поле бонусов |
| 5 | screenshot_05_order_status.png | Статус заказа | Клиент | Прогресс-индикатор на шаге «В процессе» |
| 6 | screenshot_06_order_history.png | История заказов | Клиент | Таблица с несколькими завершёнными заказами |
| 7 | screenshot_07_barista_orders.png | Панель бариста | Бариста | Список входящих заказов |
| 8 | screenshot_08_barista_accept.png | Принятие заказа | Бариста | Заказ переведён в статус «В процессе» |
| 9 | screenshot_09_admin_dashboard.png | Дашборд администратора | Администратор | KPI-карточки |
| 10 | screenshot_10_admin_menu.png | Управление меню | Администратор | Таблица с переключателями доступности |
| 11 | screenshot_11_admin_staff.png | Управление персоналом | Администратор | Список бариста, форма добавления |
| 12 | screenshot_12_admin_report.png | Отчёт смены | Администратор | Таблица начислений персоналу |
| 13 | screenshot_13_excel_export.png | Excel-отчёт | Администратор | Открытый .xlsx файл |

---

## 8. Команды запуска

### Создание базы данных
```bash
psql -U postgres -h localhost -f schema.sql
```

### Строка подключения (appsettings.json)
```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=coffeeshop;Username=postgres;Password=root"
  }
}
```

### Запуск приложения
```bash
dotnet run --project CoffeeShop/CoffeeShop.csproj
```

### Сборка Release
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```
