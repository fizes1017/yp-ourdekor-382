-- Схема базы данных diplom по дипломному проекту
-- Выполнять для новой БД: CREATE DATABASE diplom;

-- Таблица пользователей (менеджеры и администраторы)
CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    email VARCHAR(255) NOT NULL UNIQUE,
    phone VARCHAR(20) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    full_name VARCHAR(100) NOT NULL,
    role VARCHAR(20) NOT NULL DEFAULT 'Manager',
    created_at DATE NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_users_email ON users(email);
CREATE INDEX IF NOT EXISTS ix_users_phone ON users(phone);

-- Таблица материалов (справочник цен)
CREATE TABLE IF NOT EXISTS materials (
    id SERIAL PRIMARY KEY,
    article VARCHAR(50) NOT NULL UNIQUE,
    name VARCHAR(255) NOT NULL,
    price DECIMAL(10,2) NOT NULL,
    unit VARCHAR(20) NOT NULL,
    created_at DATE NOT NULL,
    created_by INTEGER REFERENCES users(id) ON DELETE SET NULL,
    updated_at TIMESTAMP,
    updated_by INTEGER REFERENCES users(id) ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS ix_materials_article ON materials(article);

-- Таблица расчётов
CREATE TABLE IF NOT EXISTS calculations (
    id SERIAL PRIMARY KEY,
    total_amount DECIMAL(10,2) NOT NULL,
    calculated_at DATE NOT NULL,
    manager_id INTEGER REFERENCES users(id) ON DELETE SET NULL,
    updated_at TIMESTAMP,
    updated_by INTEGER REFERENCES users(id) ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS ix_calculations_manager_id ON calculations(manager_id);
CREATE INDEX IF NOT EXISTS ix_calculations_calculated_at ON calculations(calculated_at);

-- Таблица позиций расчёта
CREATE TABLE IF NOT EXISTS calculation_items (
    id SERIAL PRIMARY KEY,
    calculation_id INTEGER NOT NULL REFERENCES calculations(id) ON DELETE CASCADE,
    material_id INTEGER NOT NULL REFERENCES materials(id) ON DELETE CASCADE,
    quantity DECIMAL(10,3) NOT NULL,
    unit_price DECIMAL(10,2) NOT NULL,
    total_price DECIMAL(10,2) NOT NULL,
    created_at TIMESTAMP NOT NULL,
    created_by INTEGER REFERENCES users(id) ON DELETE SET NULL,
    updated_at TIMESTAMP,
    updated_by INTEGER REFERENCES users(id) ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS ix_calculation_items_calculation_id ON calculation_items(calculation_id);
CREATE INDEX IF NOT EXISTS ix_calculation_items_material_id ON calculation_items(material_id);

-- Таблица коммерческих предложений
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
    created_at TIMESTAMP NOT NULL,
    comments TEXT
);

CREATE INDEX IF NOT EXISTS ix_commercial_proposals_calculation_id ON commercial_proposals(calculation_id);
CREATE INDEX IF NOT EXISTS ix_commercial_proposals_manager_id ON commercial_proposals(manager_id);

-- Реквизиты компании
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

-- Журнал аудита
CREATE TABLE IF NOT EXISTS audit_log (
    id SERIAL PRIMARY KEY,
    entity_type VARCHAR(50) NOT NULL,
    entity_id INTEGER,
    action VARCHAR(50) NOT NULL,
    user_id INTEGER REFERENCES users(id) ON DELETE SET NULL,
    timestamp TIMESTAMP NOT NULL,
    details TEXT
);

CREATE INDEX IF NOT EXISTS ix_audit_log_entity ON audit_log(entity_type, entity_id);
CREATE INDEX IF NOT EXISTS ix_audit_log_user_id ON audit_log(user_id);
CREATE INDEX IF NOT EXISTS ix_audit_log_timestamp ON audit_log(timestamp);
