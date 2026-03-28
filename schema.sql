-- ============================================================
-- CoffeeShop — Schema + Seed Data
-- Специальность: 09.02.07, СТИ МИСИС
-- Сгенерировано: 2026-03-23
-- ============================================================
-- Пароли: admin123 (админ), barista123 (баристы), client123 (клиенты)
-- Телефоны: +79991111111 (админ), +79992222222 (бариста1), +79993333333 (клиент1)
-- ============================================================

-- Расширения
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ============================================================
-- УДАЛЕНИЕ ОБЪЕКТОВ (обратный порядок FK)
-- ============================================================
DROP TABLE IF EXISTS audit_log CASCADE;
DROP TABLE IF EXISTS staff_earnings CASCADE;
DROP TABLE IF EXISTS loyalty_transactions CASCADE;
DROP TABLE IF EXISTS order_items CASCADE;
DROP TABLE IF EXISTS orders CASCADE;
DROP TABLE IF EXISTS shifts CASCADE;
DROP TABLE IF EXISTS menu_items CASCADE;
DROP TABLE IF EXISTS menu_categories CASCADE;
DROP TABLE IF EXISTS users CASCADE;
DROP TABLE IF EXISTS roles CASCADE;

DROP PROCEDURE IF EXISTS sp_create_order CASCADE;
DROP PROCEDURE IF EXISTS sp_complete_order CASCADE;
DROP PROCEDURE IF EXISTS sp_close_shift CASCADE;

DROP VIEW IF EXISTS v_orders_detailed CASCADE;
DROP VIEW IF EXISTS v_shift_report CASCADE;
DROP VIEW IF EXISTS v_menu_available CASCADE;

DROP FUNCTION IF EXISTS fn_update_timestamp CASCADE;
DROP FUNCTION IF EXISTS fn_recalculate_order_total CASCADE;
DROP FUNCTION IF EXISTS fn_log_order_status CASCADE;

-- ============================================================
-- ТАБЛИЦЫ
-- ============================================================

-- 1. Роли пользователей
CREATE TABLE roles (
    id          SERIAL PRIMARY KEY,
    name        VARCHAR(50)  NOT NULL UNIQUE,
    description TEXT
);

-- 2. Пользователи
CREATE TABLE users (
    id             SERIAL PRIMARY KEY,
    full_name      VARCHAR(200) NOT NULL,
    phone          VARCHAR(20)  UNIQUE,
    email          VARCHAR(200) UNIQUE,
    password_hash  VARCHAR(255) NOT NULL,
    role_id        INTEGER      NOT NULL REFERENCES roles(id),
    is_active      BOOLEAN      NOT NULL DEFAULT TRUE,
    bonus_balance  DECIMAL(10,2)         DEFAULT 0.00,
    created_at     TIMESTAMP             DEFAULT NOW(),
    updated_at     TIMESTAMP             DEFAULT NOW()
);

-- 3. Категории меню
CREATE TABLE menu_categories (
    id         SERIAL PRIMARY KEY,
    name       VARCHAR(100) NOT NULL UNIQUE,
    sort_order INTEGER      NOT NULL DEFAULT 0
);

-- 4. Позиции меню
CREATE TABLE menu_items (
    id           SERIAL PRIMARY KEY,
    category_id  INTEGER       NOT NULL REFERENCES menu_categories(id),
    name         VARCHAR(200)  NOT NULL,
    description  TEXT,
    price        DECIMAL(10,2) NOT NULL CHECK (price > 0),
    is_available BOOLEAN       NOT NULL DEFAULT TRUE,
    image_path   VARCHAR(500),
    created_at   TIMESTAMP              DEFAULT NOW(),
    updated_at   TIMESTAMP              DEFAULT NOW()
);

-- 5. Рабочие смены
CREATE TABLE shifts (
    id            SERIAL PRIMARY KEY,
    opened_by     INTEGER       NOT NULL REFERENCES users(id),
    opened_at     TIMESTAMP     NOT NULL DEFAULT NOW(),
    closed_at     TIMESTAMP,
    is_closed     BOOLEAN       NOT NULL DEFAULT FALSE,
    total_revenue DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    created_at    TIMESTAMP              DEFAULT NOW(),
    updated_at    TIMESTAMP              DEFAULT NOW()
);

-- 6. Заказы
-- Статусы: pending → in_progress → ready → completed
CREATE TABLE orders (
    id           SERIAL PRIMARY KEY,
    client_id    INTEGER       NOT NULL REFERENCES users(id),
    barista_id   INTEGER                REFERENCES users(id),
    shift_id     INTEGER                REFERENCES shifts(id),
    status       VARCHAR(50)   NOT NULL DEFAULT 'pending'
                               CHECK (status IN ('pending','in_progress','ready','completed')),
    total_amount DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    bonus_used   DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    final_amount DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    created_at   TIMESTAMP              DEFAULT NOW(),
    updated_at   TIMESTAMP              DEFAULT NOW()
);

-- 7. Позиции заказа
CREATE TABLE order_items (
    id           SERIAL PRIMARY KEY,
    order_id     INTEGER       NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    menu_item_id INTEGER       NOT NULL REFERENCES menu_items(id),
    quantity     INTEGER       NOT NULL DEFAULT 1 CHECK (quantity > 0),
    unit_price   DECIMAL(10,2) NOT NULL,
    subtotal     DECIMAL(10,2) NOT NULL,
    created_at   TIMESTAMP              DEFAULT NOW()
);

-- 8. Транзакции бонусной программы
CREATE TABLE loyalty_transactions (
    id               SERIAL PRIMARY KEY,
    user_id          INTEGER       NOT NULL REFERENCES users(id),
    order_id         INTEGER                REFERENCES orders(id),
    amount           DECIMAL(10,2) NOT NULL,
    transaction_type VARCHAR(50)   NOT NULL CHECK (transaction_type IN ('accrual','redemption')),
    description      TEXT,
    created_at       TIMESTAMP              DEFAULT NOW()
);

-- 9. Начисления персоналу по сменам
CREATE TABLE staff_earnings (
    id              SERIAL PRIMARY KEY,
    user_id         INTEGER       NOT NULL REFERENCES users(id),
    shift_id        INTEGER       NOT NULL REFERENCES shifts(id),
    orders_count    INTEGER       NOT NULL DEFAULT 0,
    orders_total    DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    earning_percent DECIMAL(5,2)  NOT NULL,
    earned_amount   DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    created_at      TIMESTAMP              DEFAULT NOW(),
    updated_at      TIMESTAMP              DEFAULT NOW(),
    UNIQUE (user_id, shift_id)
);

-- 10. Журнал изменений
CREATE TABLE audit_log (
    id         SERIAL PRIMARY KEY,
    table_name VARCHAR(100) NOT NULL,
    record_id  INTEGER      NOT NULL,
    action     VARCHAR(50)  NOT NULL,
    old_values JSONB,
    new_values JSONB,
    changed_by INTEGER      REFERENCES users(id),
    created_at TIMESTAMP    DEFAULT NOW()
);

-- ============================================================
-- ИНДЕКСЫ
-- ============================================================
CREATE INDEX idx_users_phone           ON users(phone);
CREATE INDEX idx_users_role_id         ON users(role_id);
CREATE INDEX idx_orders_client         ON orders(client_id);
CREATE INDEX idx_orders_barista        ON orders(barista_id);
CREATE INDEX idx_orders_shift          ON orders(shift_id);
CREATE INDEX idx_orders_status         ON orders(status);
CREATE INDEX idx_order_items_order     ON order_items(order_id);
CREATE INDEX idx_loyalty_user          ON loyalty_transactions(user_id);
CREATE INDEX idx_staff_earnings_shift  ON staff_earnings(shift_id);
CREATE INDEX idx_menu_items_category   ON menu_items(category_id);

-- ============================================================
-- ТРИГГЕРЫ
-- ============================================================

-- Триггер 1: Автообновление поля updated_at
-- Применяется ко всем таблицам с этим полем.
CREATE OR REPLACE FUNCTION fn_update_timestamp()
RETURNS TRIGGER LANGUAGE plpgsql AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$;

CREATE TRIGGER trg_users_updated_at
    BEFORE UPDATE ON users
    FOR EACH ROW EXECUTE FUNCTION fn_update_timestamp();

CREATE TRIGGER trg_menu_items_updated_at
    BEFORE UPDATE ON menu_items
    FOR EACH ROW EXECUTE FUNCTION fn_update_timestamp();

CREATE TRIGGER trg_shifts_updated_at
    BEFORE UPDATE ON shifts
    FOR EACH ROW EXECUTE FUNCTION fn_update_timestamp();

CREATE TRIGGER trg_orders_updated_at
    BEFORE UPDATE ON orders
    FOR EACH ROW EXECUTE FUNCTION fn_update_timestamp();

CREATE TRIGGER trg_staff_earnings_updated_at
    BEFORE UPDATE ON staff_earnings
    FOR EACH ROW EXECUTE FUNCTION fn_update_timestamp();

-- -------------------------------------------------------
-- Триггер 2: Пересчёт суммы заказа при изменении позиций
-- Автоматически обновляет total_amount и final_amount
-- в таблице orders при любом изменении order_items.
-- -------------------------------------------------------
CREATE OR REPLACE FUNCTION fn_recalculate_order_total()
RETURNS TRIGGER LANGUAGE plpgsql AS $$
DECLARE
    v_order_id INTEGER;
BEGIN
    IF TG_OP = 'DELETE' THEN
        v_order_id := OLD.order_id;
    ELSE
        v_order_id := NEW.order_id;
    END IF;

    UPDATE orders
    SET
        total_amount = (
            SELECT COALESCE(SUM(subtotal), 0.00)
            FROM order_items
            WHERE order_id = v_order_id
        ),
        final_amount = (
            SELECT COALESCE(SUM(subtotal), 0.00)
            FROM order_items
            WHERE order_id = v_order_id
        ) - bonus_used
    WHERE id = v_order_id;

    IF TG_OP = 'DELETE' THEN
        RETURN OLD;
    ELSE
        RETURN NEW;
    END IF;
END;
$$;

CREATE TRIGGER trg_recalculate_order_total
    AFTER INSERT OR UPDATE OR DELETE ON order_items
    FOR EACH ROW EXECUTE FUNCTION fn_recalculate_order_total();

-- -------------------------------------------------------
-- Триггер 3: Логирование изменений статуса заказа
-- При каждом изменении status в таблице orders
-- создаётся запись в audit_log со старым и новым значением.
-- -------------------------------------------------------
CREATE OR REPLACE FUNCTION fn_log_order_status()
RETURNS TRIGGER LANGUAGE plpgsql AS $$
BEGIN
    IF OLD.status IS DISTINCT FROM NEW.status THEN
        INSERT INTO audit_log (table_name, record_id, action, old_values, new_values, changed_by)
        VALUES (
            'orders',
            NEW.id,
            'STATUS_CHANGE',
            jsonb_build_object('status', OLD.status),
            jsonb_build_object('status', NEW.status, 'barista_id', NEW.barista_id),
            NEW.barista_id
        );
    END IF;
    RETURN NEW;
END;
$$;

CREATE TRIGGER trg_log_order_status
    AFTER UPDATE ON orders
    FOR EACH ROW EXECUTE FUNCTION fn_log_order_status();

-- ============================================================
-- ХРАНИМЫЕ ПРОЦЕДУРЫ
-- ============================================================

-- -------------------------------------------------------
-- ХП 1: Создание заказа с позициями
-- Выполняет: создание записи заказа, добавление позиций,
-- списание бонусов у клиента, запись транзакции бонусов.
-- Параметры:
--   p_client_id  — ID клиента
--   p_items      — JSON-массив [{menu_item_id, quantity}, ...]
--   p_bonus_used — сумма списываемых бонусов
--   p_order_id   — OUT: ID созданного заказа
-- -------------------------------------------------------
CREATE OR REPLACE PROCEDURE sp_create_order(
    IN  p_client_id  INTEGER,
    IN  p_items      JSONB,
    IN  p_bonus_used DECIMAL(10,2),
    INOUT p_order_id INTEGER DEFAULT NULL
)
LANGUAGE plpgsql AS $$
DECLARE
    v_item           JSONB;
    v_price          DECIMAL(10,2);
    v_client_bonus   DECIMAL(10,2);
    v_current_shift  INTEGER;
BEGIN
    -- Проверка бонусного баланса
    SELECT bonus_balance INTO v_client_bonus
    FROM users WHERE id = p_client_id AND is_active = TRUE;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Клиент % не найден или деактивирован', p_client_id;
    END IF;

    IF p_bonus_used > v_client_bonus THEN
        RAISE EXCEPTION 'Недостаточно бонусов. Доступно: %, запрошено: %',
            v_client_bonus, p_bonus_used;
    END IF;

    -- Найти текущую открытую смену
    SELECT id INTO v_current_shift
    FROM shifts WHERE is_closed = FALSE
    ORDER BY opened_at DESC LIMIT 1;

    -- Создать заказ
    INSERT INTO orders (client_id, status, bonus_used, shift_id)
    VALUES (p_client_id, 'pending', p_bonus_used, v_current_shift)
    RETURNING id INTO p_order_id;

    -- Добавить позиции
    FOR v_item IN SELECT * FROM jsonb_array_elements(p_items)
    LOOP
        SELECT price INTO v_price
        FROM menu_items
        WHERE id = (v_item->>'menu_item_id')::INTEGER
          AND is_available = TRUE;

        IF NOT FOUND THEN
            RAISE EXCEPTION 'Позиция меню % недоступна или не существует',
                v_item->>'menu_item_id';
        END IF;

        INSERT INTO order_items (order_id, menu_item_id, quantity, unit_price, subtotal)
        VALUES (
            p_order_id,
            (v_item->>'menu_item_id')::INTEGER,
            (v_item->>'quantity')::INTEGER,
            v_price,
            v_price * (v_item->>'quantity')::INTEGER
        );
    END LOOP;

    -- Обновить final_amount (total_amount уже пересчитан триггером)
    UPDATE orders
    SET final_amount = total_amount - p_bonus_used
    WHERE id = p_order_id;

    -- Списать бонусы у клиента
    IF p_bonus_used > 0 THEN
        UPDATE users
        SET bonus_balance = bonus_balance - p_bonus_used
        WHERE id = p_client_id;

        INSERT INTO loyalty_transactions (user_id, order_id, amount, transaction_type, description)
        VALUES (
            p_client_id,
            p_order_id,
            -p_bonus_used,
            'redemption',
            'Списание бонусов при оформлении заказа #' || p_order_id
        );
    END IF;
END;
$$;

-- -------------------------------------------------------
-- ХП 2: Завершение заказа
-- Выполняет: перевод статуса в completed, начисление
-- 5% бонусов клиенту, начисление 10% бариста в staff_earnings,
-- обновление выручки смены.
-- -------------------------------------------------------
CREATE OR REPLACE PROCEDURE sp_complete_order(
    IN p_order_id  INTEGER,
    IN p_barista_id INTEGER
)
LANGUAGE plpgsql AS $$
DECLARE
    v_order         RECORD;
    v_bonus_accrual DECIMAL(10,2);
    v_barista_earn  DECIMAL(10,2);
    v_shift_id      INTEGER;
BEGIN
    SELECT * INTO v_order FROM orders WHERE id = p_order_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Заказ % не найден', p_order_id;
    END IF;

    IF v_order.status = 'completed' THEN
        RAISE EXCEPTION 'Заказ % уже завершён', p_order_id;
    END IF;

    v_shift_id := v_order.shift_id;

    -- Если в заказе нет смены — найти текущую открытую
    IF v_shift_id IS NULL THEN
        SELECT id INTO v_shift_id
        FROM shifts WHERE is_closed = FALSE
        ORDER BY opened_at DESC LIMIT 1;
    END IF;

    -- Привязать заказ к смене, если нашлась
    IF v_shift_id IS NOT NULL AND v_order.shift_id IS NULL THEN
        UPDATE orders SET shift_id = v_shift_id WHERE id = p_order_id;
    END IF;

    -- Начислить бонусы клиенту: 5% от итоговой суммы
    v_bonus_accrual := ROUND(v_order.final_amount * 0.05, 2);

    UPDATE users
    SET bonus_balance = bonus_balance + v_bonus_accrual
    WHERE id = v_order.client_id;

    INSERT INTO loyalty_transactions (user_id, order_id, amount, transaction_type, description)
    VALUES (
        v_order.client_id,
        p_order_id,
        v_bonus_accrual,
        'accrual',
        'Начисление бонусов за заказ #' || p_order_id
    );

    -- Начислить 10% бариста (только при наличии смены)
    IF v_shift_id IS NOT NULL THEN
        v_barista_earn := ROUND(v_order.final_amount * 0.10, 2);

        INSERT INTO staff_earnings (user_id, shift_id, orders_count, orders_total, earning_percent, earned_amount)
        VALUES (p_barista_id, v_shift_id, 1, v_order.final_amount, 10.00, v_barista_earn)
        ON CONFLICT (user_id, shift_id) DO UPDATE
            SET orders_count = staff_earnings.orders_count + 1,
                orders_total = staff_earnings.orders_total + v_order.final_amount,
                earned_amount = staff_earnings.earned_amount + v_barista_earn;

        -- Обновить выручку смены
        UPDATE shifts
        SET total_revenue = total_revenue + v_order.final_amount
        WHERE id = v_shift_id;
    END IF;

    -- Перевести заказ в completed
    UPDATE orders
    SET status = 'completed', barista_id = p_barista_id
    WHERE id = p_order_id;
END;
$$;

-- -------------------------------------------------------
-- ХП 3: Закрытие смены
-- Выполняет: начисление 3% администратору от общей выручки,
-- закрытие смены, запись в audit_log.
-- -------------------------------------------------------
CREATE OR REPLACE PROCEDURE sp_close_shift(
    IN p_shift_id INTEGER,
    IN p_admin_id INTEGER
)
LANGUAGE plpgsql AS $$
DECLARE
    v_shift       RECORD;
    v_admin_earn  DECIMAL(10,2);
BEGIN
    SELECT * INTO v_shift FROM shifts WHERE id = p_shift_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Смена % не найдена', p_shift_id;
    END IF;

    IF v_shift.is_closed THEN
        RAISE EXCEPTION 'Смена % уже закрыта', p_shift_id;
    END IF;

    -- Начислить администратору 3% от выручки смены
    v_admin_earn := ROUND(v_shift.total_revenue * 0.03, 2);

    INSERT INTO staff_earnings (user_id, shift_id, orders_count, orders_total, earning_percent, earned_amount)
    VALUES (p_admin_id, p_shift_id, 0, v_shift.total_revenue, 3.00, v_admin_earn)
    ON CONFLICT (user_id, shift_id) DO UPDATE
        SET earned_amount = v_admin_earn,
            orders_total  = v_shift.total_revenue;

    -- Закрыть смену
    UPDATE shifts
    SET is_closed  = TRUE,
        closed_at  = NOW()
    WHERE id = p_shift_id;

    -- Записать в журнал
    INSERT INTO audit_log (table_name, record_id, action, new_values, changed_by)
    VALUES (
        'shifts',
        p_shift_id,
        'CLOSE_SHIFT',
        jsonb_build_object(
            'closed_at',     NOW(),
            'total_revenue', v_shift.total_revenue,
            'admin_earning', v_admin_earn
        ),
        p_admin_id
    );
END;
$$;

-- ============================================================
-- ПРЕДСТАВЛЕНИЯ (VIEWS)
-- ============================================================

-- -------------------------------------------------------
-- VIEW 1: Детализированные заказы с JOIN на пользователей
-- Используется: панель бариста, история клиента, отчёты.
-- -------------------------------------------------------
CREATE OR REPLACE VIEW v_orders_detailed AS
SELECT
    o.id                  AS order_id,
    o.status,
    o.total_amount,
    o.bonus_used,
    o.final_amount,
    o.created_at          AS ordered_at,
    o.updated_at,
    c.id                  AS client_id,
    c.full_name           AS client_name,
    c.phone               AS client_phone,
    b.id                  AS barista_id,
    b.full_name           AS barista_name,
    s.id                  AS shift_id,
    s.opened_at           AS shift_date,
    COUNT(oi.id)          AS items_count
FROM orders o
JOIN  users c  ON c.id = o.client_id
LEFT JOIN users b  ON b.id = o.barista_id
LEFT JOIN shifts s ON s.id = o.shift_id
LEFT JOIN order_items oi ON oi.order_id = o.id
GROUP BY o.id, c.id, c.full_name, c.phone, b.id, b.full_name, s.id, s.opened_at;

-- -------------------------------------------------------
-- VIEW 2: Агрегированный отчёт по каждой смене
-- Используется: экран отчёта смены у администратора.
-- -------------------------------------------------------
CREATE OR REPLACE VIEW v_shift_report AS
SELECT
    s.id                                          AS shift_id,
    s.opened_at,
    s.closed_at,
    s.is_closed,
    s.total_revenue,
    u.full_name                                   AS admin_name,
    COUNT(DISTINCT o.id)                          AS orders_count,
    COUNT(DISTINCT o.client_id)                   AS unique_clients,
    COALESCE(SUM(o.bonus_used), 0)                AS total_bonus_used,
    COALESCE(SUM(se.earned_amount), 0)            AS total_staff_payout,
    s.total_revenue - COALESCE(SUM(se.earned_amount), 0) AS net_revenue
FROM shifts s
JOIN  users u  ON u.id = s.opened_by
LEFT JOIN orders o  ON o.shift_id = s.id AND o.status = 'completed'
LEFT JOIN staff_earnings se ON se.shift_id = s.id
GROUP BY s.id, u.full_name;

-- -------------------------------------------------------
-- VIEW 3: Только доступные позиции меню с категорией
-- Используется: экран меню клиента.
-- -------------------------------------------------------
CREATE OR REPLACE VIEW v_menu_available AS
SELECT
    mi.id,
    mi.name,
    mi.description,
    mi.price,
    mi.image_path,
    mc.id        AS category_id,
    mc.name      AS category_name,
    mc.sort_order
FROM menu_items mi
JOIN menu_categories mc ON mc.id = mi.category_id
WHERE mi.is_available = TRUE
ORDER BY mc.sort_order, mi.name;

-- ============================================================
-- SEED DATA
-- ============================================================

-- Роли (3 записи)
INSERT INTO roles (name, description) VALUES
    ('Клиент',        'Зарегистрированный клиент кофейни'),
    ('Бариста',       'Сотрудник кофейни'),
    ('Администратор', 'Управляющий кофейней');

-- Категории меню (4 записи)
INSERT INTO menu_categories (name, sort_order) VALUES
    ('Кофе',    1),
    ('Чай',     2),
    ('Десерты', 3),
    ('Снэки',   4);

-- Пользователи (9 записей: 1 admin, 3 barista, 5 clients)
-- Пароли: admin123 / barista123 / client123
INSERT INTO users (full_name, phone, email, password_hash, role_id, is_active, bonus_balance) VALUES
    -- Администратор (role_id=3) — телефон: +79991111111, пароль: admin123
    ('Иванова Марина Александровна', '+79991111111', 'admin@coffeeshop.ru',
     '$2a$11$bPMoUgzYMoH3OU0C/TEdo.zzPV3qvs9xuG.uiuTfUPXEFau4L0WN2', 3, TRUE,  0.00),
    -- Бариста (role_id=2) — пароль: barista123
    ('Петров Алексей Игоревич',      '+79992222222', 'barista1@coffeeshop.ru',
     '$2a$11$QmOg7lLQrFv7jV1WqD/Wo.h4ZLFj36NWEnZiExhU7m6j5RDK1TqHy', 2, TRUE,  0.00),
    ('Сидорова Юлия Павловна',       '+79992222233', 'barista2@coffeeshop.ru',
     '$2a$11$QmOg7lLQrFv7jV1WqD/Wo.h4ZLFj36NWEnZiExhU7m6j5RDK1TqHy', 2, TRUE,  0.00),
    ('Козлов Дмитрий Сергеевич',     '+79992222244', NULL,
     '$2a$11$QmOg7lLQrFv7jV1WqD/Wo.h4ZLFj36NWEnZiExhU7m6j5RDK1TqHy', 2, FALSE, 0.00),
    -- Клиенты (role_id=1) — пароль: client123
    ('Смирнова Анна Владимировна',      '+79993333333', NULL,
     '$2a$11$ZrgiZ03CzASkxRxEESMV5OnU.iE7D5dGI8T3m6xpoy9SZZyyWSXvu', 1, TRUE, 61.00),
    ('Новиков Константин Борисович',    '+79993333344', NULL,
     '$2a$11$ZrgiZ03CzASkxRxEESMV5OnU.iE7D5dGI8T3m6xpoy9SZZyyWSXvu', 1, TRUE, 49.00),
    ('Федорова Ольга Михайловна',       '+79993333355', NULL,
     '$2a$11$ZrgiZ03CzASkxRxEESMV5OnU.iE7D5dGI8T3m6xpoy9SZZyyWSXvu', 1, TRUE, 40.50),
    ('Воробьев Игорь Николаевич',       '+79993333366', NULL,
     '$2a$11$ZrgiZ03CzASkxRxEESMV5OnU.iE7D5dGI8T3m6xpoy9SZZyyWSXvu', 1, TRUE, 21.00),
    ('Захарова Екатерина Дмитриевна',   '+79993333377', NULL,
     '$2a$11$ZrgiZ03CzASkxRxEESMV5OnU.iE7D5dGI8T3m6xpoy9SZZyyWSXvu', 1, TRUE, 26.00);
-- user_id: 1=admin, 2=barista1(Петров), 3=barista2(Сидорова), 4=barista3(inactive)
--          5=Смирнова, 6=Новиков, 7=Федорова, 8=Воробьев, 9=Захарова

-- Позиции меню (12 записей)
INSERT INTO menu_items (category_id, name, description, price, is_available) VALUES
    -- Кофе (category_id=1)
    (1, 'Эспрессо',    'Классический двойной эспрессо',                    150.00, TRUE),
    (1, 'Капучино',    'Эспрессо с нежной молочной пенкой',                220.00, TRUE),
    (1, 'Латте',       'Мягкий кофейно-молочный напиток',                  250.00, TRUE),
    (1, 'Флэт уайт',   'Крепкий эспрессо с небольшим количеством молока', 240.00, TRUE),
    -- Чай (category_id=2)
    (2, 'Чай чёрный',  'Классический цейлонский чай',                      120.00, TRUE),
    (2, 'Матча латте', 'Японский зелёный чай с молоком',                   280.00, TRUE),
    (2, 'Чай с имбирём','Имбирный чай с лимоном и мёдом',                  160.00, TRUE),
    -- Десерты (category_id=3)
    (3, 'Чизкейк нью-йорк', 'Классический сливочный чизкейк',             290.00, TRUE),
    (3, 'Брауни',            'Шоколадный брауни с грецкими орехами',       220.00, TRUE),
    (3, 'Круассан',          'Свежий слоёный круассан с маслом',           180.00, TRUE),
    -- Снэки (category_id=4)
    (4, 'Сэндвич с курицей', 'Тост с куриной грудкой, овощами и соусом',  320.00, TRUE),
    (4, 'Авокадо тост',      'Цельнозерновой тост с авокадо и яйцом',     350.00, FALSE);
-- menu_item_id: 1=Эспрессо, 2=Капучино, 3=Латте, 4=Флэт уайт, 5=Чай чёрный,
--              6=Матча, 7=Имбирный, 8=Чизкейк, 9=Брауни, 10=Круассан,
--              11=Сэндвич, 12=Авокадо тост (недоступен)

-- Смены (3 записи: 2 закрытые, 1 текущая)
INSERT INTO shifts (opened_by, opened_at, closed_at, is_closed, total_revenue) VALUES
    (1, '2026-03-21 08:00:00', '2026-03-21 20:00:00', TRUE,  1480.00),  -- shift_id=1
    (1, '2026-03-22 08:00:00', '2026-03-22 20:00:00', TRUE,  1960.00),  -- shift_id=2
    (1, '2026-03-23 08:00:00', NULL,                  FALSE,  510.00);  -- shift_id=3 (текущая)

-- Заказы (12 записей)
-- Смена 1 (все completed): клиенты 5,6,7,8 / бариста 2,3
INSERT INTO orders (client_id, barista_id, shift_id, status, total_amount, bonus_used, final_amount) VALUES
    (5, 2, 1, 'completed', 340.00,  0.00, 340.00),  -- order_id=1
    (6, 3, 1, 'completed', 540.00, 50.00, 490.00),  -- order_id=2 (списал 50 бонусов)
    (7, 2, 1, 'completed', 280.00,  0.00, 280.00),  -- order_id=3
    (8, 3, 1, 'completed', 420.00,  0.00, 420.00),  -- order_id=4
-- Смена 2 (все completed)
    (5, 2, 2, 'completed', 370.00,  0.00, 370.00),  -- order_id=5
    (6, 3, 2, 'completed', 540.00,  0.00, 540.00),  -- order_id=6
    (7, 2, 2, 'completed', 530.00,  0.00, 530.00),  -- order_id=7
    (9, 3, 2, 'completed', 520.00,  0.00, 520.00),  -- order_id=8
-- Смена 3 (текущая: 1 completed, 3 в процессе)
    (5, 2, 3, 'completed', 510.00,  0.00, 510.00),  -- order_id=9
    (6, NULL, 3, 'pending',    400.00,  0.00, 400.00),  -- order_id=10
    (7, 2,    3, 'in_progress',220.00,  0.00, 220.00),  -- order_id=11
    (8, NULL, 3, 'ready',      430.00, 50.00, 380.00);  -- order_id=12

-- Позиции заказов
-- Заказ 1  (340):  Капучино(220) + Чай чёрный(120)
-- Заказ 2  (540):  Латте(250) + Чизкейк(290)          | bonus_used=50, final=490
-- Заказ 3  (280):  Матча латте(280)
-- Заказ 4  (420):  Флэт уайт(240) + Круассан(180)
-- Заказ 5  (370):  Эспрессо(150) + Брауни(220)
-- Заказ 6  (540):  Капучино(220) + Сэндвич(320)
-- Заказ 7  (530):  Флэт уайт(240) + Чизкейк(290)
-- Заказ 8  (520):  Чай чёрный(120) + Круассан(180) + Брауни(220)
-- Заказ 9  (510):  Капучино(220) + Чизкейк(290)
-- Заказ 10 (400):  Флэт уайт(240) + Чай с имбирём(160)
-- Заказ 11 (220):  Капучино(220)
-- Заказ 12 (430):  Латте(250) + Круассан(180)          | bonus_used=50, final=380
INSERT INTO order_items (order_id, menu_item_id, quantity, unit_price, subtotal) VALUES
    (1,  2, 1, 220.00, 220.00),
    (1,  5, 1, 120.00, 120.00),
    (2,  3, 1, 250.00, 250.00),
    (2,  8, 1, 290.00, 290.00),
    (3,  6, 1, 280.00, 280.00),
    (4,  4, 1, 240.00, 240.00),
    (4, 10, 1, 180.00, 180.00),
    (5,  1, 1, 150.00, 150.00),
    (5,  9, 1, 220.00, 220.00),
    (6,  2, 1, 220.00, 220.00),
    (6, 11, 1, 320.00, 320.00),
    (7,  4, 1, 240.00, 240.00),
    (7,  8, 1, 290.00, 290.00),
    (8,  5, 1, 120.00, 120.00),
    (8, 10, 1, 180.00, 180.00),
    (8,  9, 1, 220.00, 220.00),
    (9,  2, 1, 220.00, 220.00),
    (9,  8, 1, 290.00, 290.00),
    (10, 4, 1, 240.00, 240.00),
    (10, 7, 1, 160.00, 160.00),
    (11, 2, 1, 220.00, 220.00),
    (12, 3, 1, 250.00, 250.00),
    (12,10, 1, 180.00, 180.00);

-- Транзакции бонусной программы (по завершённым заказам)
INSERT INTO loyalty_transactions (user_id, order_id, amount, transaction_type, description) VALUES
    -- Заказ 1: клиент 5 получает 5% от 340 = 17
    (5, 1,   17.00, 'accrual',    'Начисление бонусов за заказ #1'),
    -- Заказ 2: клиент 6 списал 50 + получает 5% от 490 = 24.50
    (6, 2,  -50.00, 'redemption', 'Списание бонусов при оформлении заказа #2'),
    (6, 2,   24.50, 'accrual',    'Начисление бонусов за заказ #2'),
    -- Заказ 3: клиент 7 получает 5% от 280 = 14
    (7, 3,   14.00, 'accrual',    'Начисление бонусов за заказ #3'),
    -- Заказ 4: клиент 8 получает 5% от 420 = 21
    (8, 4,   21.00, 'accrual',    'Начисление бонусов за заказ #4'),
    -- Заказ 5: клиент 5 получает 5% от 370 = 18.50
    (5, 5,   18.50, 'accrual',    'Начисление бонусов за заказ #5'),
    -- Заказ 6: клиент 6 получает 5% от 540 = 27
    (6, 6,   27.00, 'accrual',    'Начисление бонусов за заказ #6'),
    -- Заказ 7: клиент 7 получает 5% от 530 = 26.50
    (7, 7,   26.50, 'accrual',    'Начисление бонусов за заказ #7'),
    -- Заказ 8: клиент 9 получает 5% от 520 = 26
    (9, 8,   26.00, 'accrual',    'Начисление бонусов за заказ #8'),
    -- Заказ 9: клиент 5 получает 5% от 510 = 25.50
    (5, 9,   25.50, 'accrual',    'Начисление бонусов за заказ #9'),
    -- Заказ 12 (ready — бонусы ещё не начислены, но был списан 50 при оформлении)
    (8, 12, -50.00, 'redemption', 'Списание бонусов при оформлении заказа #12');

-- Начисления персоналу по закрытым сменам
-- Смена 1: Петров взял заказы 1,3 (340+280=620, 10%=62),
--           Сидорова взяла заказы 2,4 (490+420=910, 10%=91)
--           Иванова (admin) 3% от 1480 = 44.40
INSERT INTO staff_earnings (user_id, shift_id, orders_count, orders_total, earning_percent, earned_amount) VALUES
    (2, 1, 2,  620.00, 10.00,  62.00),
    (3, 1, 2,  910.00, 10.00,  91.00),
    (1, 1, 0, 1480.00,  3.00,  44.40),
-- Смена 2: Петров взял 5,7 (370+530=900, 10%=90),
--           Сидорова взяла 6,8 (540+520=1060, 10%=106)
--           Иванова 3% от 1960 = 58.80
    (2, 2, 2,  900.00, 10.00,  90.00),
    (3, 2, 2, 1060.00, 10.00, 106.00),
    (1, 2, 0, 1960.00,  3.00,  58.80),
-- Смена 3 (текущая, только завершённый заказ 9): Петров взял заказ 9 (510, 10%=51)
    (2, 3, 1,  510.00, 10.00,  51.00);

-- Несколько записей в журнале (смены закрыты)
INSERT INTO audit_log (table_name, record_id, action, old_values, new_values, changed_by) VALUES
    ('shifts', 1, 'CLOSE_SHIFT', NULL,
     '{"closed_at": "2026-03-21 20:00:00", "total_revenue": 1480.00, "admin_earning": 44.40}'::JSONB, 1),
    ('shifts', 2, 'CLOSE_SHIFT', NULL,
     '{"closed_at": "2026-03-22 20:00:00", "total_revenue": 1960.00, "admin_earning": 58.80}'::JSONB, 1),
    ('orders', 1, 'STATUS_CHANGE',
     '{"status": "pending"}'::JSONB,
     '{"status": "in_progress", "barista_id": 2}'::JSONB, 2),
    ('orders', 1, 'STATUS_CHANGE',
     '{"status": "in_progress"}'::JSONB,
     '{"status": "ready", "barista_id": 2}'::JSONB, 2),
    ('orders', 1, 'STATUS_CHANGE',
     '{"status": "ready"}'::JSONB,
     '{"status": "completed", "barista_id": 2}'::JSONB, 2);

-- ============================================================
-- Проверка: выборка из представлений
-- ============================================================
-- SELECT * FROM v_menu_available;
-- SELECT * FROM v_orders_detailed WHERE shift_id = 3;
-- SELECT * FROM v_shift_report;
