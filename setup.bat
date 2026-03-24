@echo off
chcp 65001 >nul
echo ============================================
echo   CoffeeShop - Автоматическая настройка
echo   Специальность: 09.02.07, СТИ МИСИС
echo   Студент: Лагошина Е.С., ИСП-23-2
echo ============================================
echo.

:: --- Проверка .NET 8 SDK ---
echo [1/4] Проверка .NET SDK...
dotnet --version >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo ОШИБКА: .NET SDK не установлен!
    echo Скачайте .NET 8 SDK: https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)
echo       .NET SDK найден.

:: --- Проверка PostgreSQL ---
echo [2/4] Проверка PostgreSQL...
where psql >nul 2>&1
if %ERRORLEVEL% neq 0 (
    :: Попробовать стандартные пути
    if exist "C:\Program Files\PostgreSQL\18\bin\psql.exe" (
        set "PSQL=C:\Program Files\PostgreSQL\18\bin\psql.exe"
    ) else if exist "C:\Program Files\PostgreSQL\17\bin\psql.exe" (
        set "PSQL=C:\Program Files\PostgreSQL\17\bin\psql.exe"
    ) else if exist "C:\Program Files\PostgreSQL\16\bin\psql.exe" (
        set "PSQL=C:\Program Files\PostgreSQL\16\bin\psql.exe"
    ) else (
        echo ОШИБКА: PostgreSQL не найден!
        echo Скачайте PostgreSQL: https://www.postgresql.org/download/windows/
        echo При установке запомните пароль для пользователя postgres.
        pause
        exit /b 1
    )
) else (
    set "PSQL=psql"
)
echo       PostgreSQL найден.

:: --- Запрос пароля PostgreSQL ---
echo.
echo [3/4] Настройка базы данных...
set /p "PGPASS=Введите пароль PostgreSQL (по умолчанию: root): "
if "%PGPASS%"=="" set "PGPASS=root"

:: --- Создание базы данных ---
echo       Создание базы данных coffeeshop...
set PGPASSWORD=%PGPASS%
"%PSQL%" -U postgres -h localhost -c "DROP DATABASE IF EXISTS coffeeshop;" >nul 2>&1
"%PSQL%" -U postgres -h localhost -c "CREATE DATABASE coffeeshop;" >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo ОШИБКА: Не удалось подключиться к PostgreSQL.
    echo Проверьте: 1) PostgreSQL запущен  2) Пароль верный  3) Порт 5432
    pause
    exit /b 1
)

:: --- Применение схемы ---
echo       Применение схемы и тестовых данных...
"%PSQL%" -U postgres -h localhost -d coffeeshop -f "%~dp0schema.sql" >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo ОШИБКА: Не удалось применить схему БД.
    pause
    exit /b 1
)
echo       База данных создана успешно!

:: --- Обновление строки подключения если пароль не root ---
if not "%PGPASS%"=="root" (
    echo       Обновление строки подключения...
    powershell -Command "(Get-Content '%~dp0CoffeeShop\Data\AppDbContext.cs') -replace 'Password=root', 'Password=%PGPASS%' | Set-Content '%~dp0CoffeeShop\Data\AppDbContext.cs'"
)

:: --- Сборка и запуск ---
echo.
echo [4/4] Сборка проекта...
cd /d "%~dp0"
dotnet restore >nul 2>&1
dotnet build --configuration Release >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo ОШИБКА: Сборка не удалась. Проверьте вывод выше.
    pause
    exit /b 1
)
echo       Сборка успешна!

echo.
echo ============================================
echo   Готово! Запуск приложения...
echo ============================================
echo.
echo   Тестовые аккаунты:
echo   Админ:   +79991111111 / admin123
echo   Бариста: +79992222222 / barista123
echo   Клиент:  +79993333333 / client123
echo.

dotnet run --project CoffeeShop --configuration Release
pause
