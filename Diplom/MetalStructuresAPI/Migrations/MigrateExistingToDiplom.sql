-- Миграция существующей БД theoneprogram -> diplom
-- Если БД уже создана как diplom и таблицы users/materials/calculations/calculation_items существуют,
-- выполните только нужные команды (добавление role, новых таблиц).

-- 1. Добавить колонку role в users (если таблица users существует)
ALTER TABLE users ADD COLUMN IF NOT EXISTS role VARCHAR(20) NOT NULL DEFAULT 'Manager';

-- 2. Таблица коммерческих предложений
CREATE TABLE IF NOT EXISTS commercial_proposals (
    id SERIAL PRIMARY KEY,
    calculation_id INTEGER NOT NULL REFERENCES calculations(id) ON DELETE CASCADE,
    manager_id INTEGER REFERENCES users(id) ON DELETE SET NULL,
    customer_company VARCHAR(255),
    customer_person VARCHAR(255),
    customer_phone VARCHAR(50),
    customer_email VARCHAR(255),
    customer_address VARCHAR(500),
    proposal_number VARCHAR(50),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    comments TEXT
);

CREATE INDEX IF NOT EXISTS ix_commercial_proposals_calculation_id ON commercial_proposals(calculation_id);
CREATE INDEX IF NOT EXISTS ix_commercial_proposals_manager_id ON commercial_proposals(manager_id);

-- 3. Реквизиты компании
CREATE TABLE IF NOT EXISTS company_info (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    address VARCHAR(500),
    phone VARCHAR(50),
    email VARCHAR(255),
    inn VARCHAR(20),
    kpp VARCHAR(20),
    bank_details TEXT,
    updated_at TIMESTAMP
);

-- 4. Журнал аудита
CREATE TABLE IF NOT EXISTS audit_log (
    id SERIAL PRIMARY KEY,
    entity_type VARCHAR(50) NOT NULL,
    entity_id INTEGER,
    action VARCHAR(50) NOT NULL,
    user_id INTEGER REFERENCES users(id) ON DELETE SET NULL,
    timestamp TIMESTAMP NOT NULL DEFAULT NOW(),
    details TEXT
);

CREATE INDEX IF NOT EXISTS ix_audit_log_entity ON audit_log(entity_type, entity_id);
CREATE INDEX IF NOT EXISTS ix_audit_log_user_id ON audit_log(user_id);
CREATE INDEX IF NOT EXISTS ix_audit_log_timestamp ON audit_log(timestamp);
