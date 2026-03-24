# CoffeeShop — Система автоматизации кофейни

> Курсовой проект по ПМ.05 «Информационные системы и программирование»
> Специальность: 09.02.07
> СТИ МИСИС (Оскольский политехнический колледж)
> Студент: Лагошина Ева Сергеевна, группа ИСП-23-2

## Описание

WPF-приложение для автоматизации работы кофейни с системой лояльности.
Три роли пользователей: **Клиент**, **Бариста**, **Администратор**.

## Требования

- **Windows 10/11**
- **.NET 8 SDK** — [скачать](https://dotnet.microsoft.com/download/dotnet/8.0)
- **PostgreSQL 16+** — [скачать](https://www.postgresql.org/download/windows/)

## Быстрый запуск

### Автоматический (рекомендуется)

1. Убедитесь что PostgreSQL установлен и запущен
2. Запустите `setup.bat`
3. Введите пароль PostgreSQL (по умолчанию: `root`)
4. Приложение запустится автоматически

### Ручной

```bash
# 1. Создать базу данных
psql -U postgres -c "CREATE DATABASE coffeeshop;"

# 2. Применить схему
psql -U postgres -d coffeeshop -f schema.sql

# 3. Если пароль PostgreSQL не "root", измените строку подключения:
#    CoffeeShop/Data/AppDbContext.cs → строка 22

# 4. Собрать и запустить
dotnet restore
dotnet run --project CoffeeShop
```

## Тестовые аккаунты

| Роль | Телефон | Пароль |
|------|---------|--------|
| Администратор | `+79991111111` | `admin123` |
| Бариста | `+79992222222` | `barista123` |
| Бариста 2 | `+79992222233` | `barista123` |
| Клиент | `+79993333333` | `client123` |
| Клиент 2 | `+79993333344` | `client123` |

## Стек технологий

- C# / .NET 8 / WPF (code-behind)
- PostgreSQL (Entity Framework Core 8 + Npgsql)
- BCrypt.Net-Next (хеширование паролей)
- ClosedXML (экспорт отчётов в Excel)
- MaterialDesignInXAML (UI-компоненты)

## Структура проекта

```
CoffeeShop/
├── Assets/            # Ресурсы (логотип)
├── Data/              # DbContext, подключение к БД
├── Helpers/           # SessionManager
├── Models/            # Модели EF Core
├── Services/          # Бизнес-логика
├── Views/
│   ├── Admin/         # Страницы администратора
│   ├── Barista/       # Страницы бариста
│   ├── Client/        # Страницы клиента
│   └── Dialogs/       # Диалоговые окна
├── schema.sql         # Схема БД + тестовые данные
└── setup.bat          # Автоматическая настройка
```
