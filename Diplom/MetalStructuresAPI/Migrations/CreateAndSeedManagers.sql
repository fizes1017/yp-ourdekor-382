-- Создание таблицы managers
CREATE TABLE IF NOT EXISTS managers (
    id SERIAL PRIMARY KEY,
    username VARCHAR(100) NOT NULL UNIQUE,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    full_name VARCHAR(100) NOT NULL,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP
);

-- Создание индексов
CREATE INDEX IF NOT EXISTS IX_managers_username ON managers(username);
CREATE INDEX IF NOT EXISTS IX_managers_email ON managers(email);

-- Заполнение данными (15 менеджеров)
-- Пароль для всех: password123
-- Хэш SHA256: jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=

INSERT INTO managers (username, email, password_hash, full_name, created_at) VALUES
('manager1', 'manager1@metalstructures.com', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 'Иванов Иван Иванович', NOW()),
('manager2', 'manager2@metalstructures.com', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 'Петров Петр Петрович', NOW()),
('manager3', 'manager3@metalstructures.com', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 'Сидоров Сидор Сидорович', NOW()),
('manager4', 'manager4@metalstructures.com', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 'Кузнецов Алексей Владимирович', NOW()),
('manager5', 'manager5@metalstructures.com', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 'Смирнов Дмитрий Сергеевич', NOW()),
('manager6', 'manager6@metalstructures.com', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 'Попов Андрей Николаевич', NOW()),
('manager7', 'manager7@metalstructures.com', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 'Васильев Максим Олегович', NOW()),
('manager8', 'manager8@metalstructures.com', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 'Михайлов Сергей Александрович', NOW()),
('manager9', 'manager9@metalstructures.com', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 'Новиков Игорь Викторович', NOW()),
('manager10', 'manager10@metalstructures.com', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 'Федоров Роман Юрьевич', NOW()),
('manager11', 'manager11@metalstructures.com', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 'Морозов Павел Дмитриевич', NOW()),
('manager12', 'manager12@metalstructures.com', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 'Волков Константин Игоревич', NOW()),
('manager13', 'manager13@metalstructures.com', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 'Алексеев Владимир Станиславович', NOW()),
('manager14', 'manager14@metalstructures.com', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 'Лебедев Артем Валерьевич', NOW()),
('manager15', 'manager15@metalstructures.com', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', 'Семенов Евгений Анатольевич', NOW())
ON CONFLICT (username) DO NOTHING;

