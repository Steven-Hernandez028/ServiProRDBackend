-- ============================================
-- SERVIPRO - Base de Datos PostgreSQL
-- Marketplace de Servicios
-- ============================================

-- Eliminar tablas existentes (en orden correcto por dependencias)
DROP TABLE IF EXISTS reviews CASCADE;
DROP TABLE IF EXISTS service_requests CASCADE;
DROP TABLE IF EXISTS provider_categories CASCADE;
DROP TABLE IF EXISTS user_sessions CASCADE;
DROP TABLE IF EXISTS providers CASCADE;
DROP TABLE IF EXISTS clients CASCADE;
DROP TABLE IF EXISTS users CASCADE;
DROP TABLE IF EXISTS service_categories CASCADE;

-- ============================================
-- TIPOS ENUMERADOS
-- ============================================

-- Tipo de rol de usuario
DROP TYPE IF EXISTS user_role CASCADE;
CREATE TYPE user_role AS ENUM ('provider', 'client', 'admin');

-- Estado de solicitud de servicio
DROP TYPE IF EXISTS request_status CASCADE;
CREATE TYPE request_status AS ENUM ('pending', 'accepted', 'rejected', 'completed', 'cancelled');

-- ============================================
-- TABLA: service_categories
-- Catálogo de categorías de servicios
-- ============================================
CREATE TABLE service_categories (
    id SERIAL PRIMARY KEY,
    code VARCHAR(50) UNIQUE NOT NULL,
    label VARCHAR(100) NOT NULL,
    icon VARCHAR(50) NOT NULL,
    description TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Insertar categorías de servicios
INSERT INTO service_categories (code, label, icon, description) VALUES
    ('mecanico', 'Mecánico', 'Wrench', 'Servicios mecánicos automotrices'),
    ('albanil', 'Albañil', 'HardHat', 'Construcción y albañilería'),
    ('pintor', 'Pintor', 'Paintbrush', 'Pintura interior y exterior'),
    ('plomero', 'Plomero', 'Droplets', 'Instalaciones y reparaciones de plomería'),
    ('electricista', 'Electricista', 'Zap', 'Instalaciones y reparaciones eléctricas'),
    ('limpieza', 'Limpieza', 'Sparkles', 'Servicios de limpieza profesional'),
    ('jardinero', 'Jardinero', 'TreeDeciduous', 'Jardinería y paisajismo'),
    ('carpintero', 'Carpintero', 'Hammer', 'Carpintería y muebles'),
    ('cerrajero', 'Cerrajero', 'Key', 'Cerrajería y seguridad'),
    ('tecnico_electrodomesticos', 'Técnico Electrodomésticos', 'Tv', 'Reparación de electrodomésticos'),
    ('mudanzas', 'Mudanzas', 'Truck', 'Servicios de mudanza'),
    ('otro', 'Otro', 'MoreHorizontal', 'Otros servicios');

-- ============================================
-- TABLA: users
-- Tabla principal de usuarios (base)
-- ============================================
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    name VARCHAR(150) NOT NULL,
    phone VARCHAR(20),
    role user_role NOT NULL DEFAULT 'client',
    avatar TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    email_verified BOOLEAN DEFAULT FALSE,
    email_verified_at TIMESTAMP WITH TIME ZONE,
    last_login_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Índices para búsquedas frecuentes
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_role ON users(role);
CREATE INDEX idx_users_is_active ON users(is_active);

-- ============================================
-- TABLA: user_sessions
-- Sesiones de inicio de sesión
-- ============================================
CREATE TABLE user_sessions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    session_token VARCHAR(255) UNIQUE NOT NULL,
    ip_address INET,
    user_agent TEXT,
    expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    last_activity_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Índices para sesiones
CREATE INDEX idx_user_sessions_user_id ON user_sessions(user_id);
CREATE INDEX idx_user_sessions_token ON user_sessions(session_token);
CREATE INDEX idx_user_sessions_expires ON user_sessions(expires_at);

-- ============================================
-- TABLA: clients
-- Información adicional para clientes
-- ============================================
CREATE TABLE clients (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID UNIQUE NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    address TEXT,
    city VARCHAR(100),
    zone VARCHAR(100),
    postal_code VARCHAR(10),
    preferences JSONB DEFAULT '{}',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Índices para clientes
CREATE INDEX idx_clients_user_id ON clients(user_id);
CREATE INDEX idx_clients_city ON clients(city);

-- ============================================
-- TABLA: providers
-- Información adicional para proveedores de servicios
-- ============================================
CREATE TABLE providers (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID UNIQUE NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    description TEXT,
    city VARCHAR(100) NOT NULL,
    zone VARCHAR(100),
    rating DECIMAL(3, 2) DEFAULT 0.00 CHECK (rating >= 0 AND rating <= 5),
    review_count INTEGER DEFAULT 0,
    hourly_rate DECIMAL(10, 2),
    is_verified BOOLEAN DEFAULT FALSE,
    verified_at TIMESTAMP WITH TIME ZONE,
    experience VARCHAR(100),
    availability TEXT,
    portfolio_images TEXT[],
    business_name VARCHAR(200),
    tax_id VARCHAR(50),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Índices para proveedores
CREATE INDEX idx_providers_user_id ON providers(user_id);
CREATE INDEX idx_providers_city ON providers(city);
CREATE INDEX idx_providers_rating ON providers(rating DESC);
CREATE INDEX idx_providers_is_verified ON providers(is_verified);

-- ============================================
-- TABLA: provider_categories
-- Relación muchos a muchos: proveedores - categorías
-- ============================================
CREATE TABLE provider_categories (
    id SERIAL PRIMARY KEY,
    provider_id UUID NOT NULL REFERENCES providers(id) ON DELETE CASCADE,
    category_id INTEGER NOT NULL REFERENCES service_categories(id) ON DELETE CASCADE,
    years_experience INTEGER,
    is_primary BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(provider_id, category_id)
);

-- Índices para categorías de proveedores
CREATE INDEX idx_provider_categories_provider ON provider_categories(provider_id);
CREATE INDEX idx_provider_categories_category ON provider_categories(category_id);

-- ============================================
-- TABLA: service_requests
-- Solicitudes de servicio
-- ============================================
CREATE TABLE service_requests (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    client_id UUID NOT NULL REFERENCES clients(id) ON DELETE CASCADE,
    provider_id UUID REFERENCES providers(id) ON DELETE SET NULL,
    category_id INTEGER NOT NULL REFERENCES service_categories(id),
    title VARCHAR(200) NOT NULL,
    description TEXT NOT NULL,
    city VARCHAR(100) NOT NULL,
    zone VARCHAR(100),
    address TEXT NOT NULL,
    status request_status DEFAULT 'pending',
    budget DECIMAL(12, 2),
    final_price DECIMAL(12, 2),
    preferred_date DATE,
    preferred_time_start TIME,
    preferred_time_end TIME,
    completed_at TIMESTAMP WITH TIME ZONE,
    cancelled_at TIMESTAMP WITH TIME ZONE,
    cancellation_reason TEXT,
    images TEXT[],
    notes TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Índices para solicitudes
CREATE INDEX idx_service_requests_client ON service_requests(client_id);
CREATE INDEX idx_service_requests_provider ON service_requests(provider_id);
CREATE INDEX idx_service_requests_category ON service_requests(category_id);
CREATE INDEX idx_service_requests_status ON service_requests(status);
CREATE INDEX idx_service_requests_city ON service_requests(city);
CREATE INDEX idx_service_requests_created ON service_requests(created_at DESC);

-- ============================================
-- TABLA: reviews
-- Reseñas de servicios
-- ============================================
CREATE TABLE reviews (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    service_request_id UUID UNIQUE REFERENCES service_requests(id) ON DELETE SET NULL,
    provider_id UUID NOT NULL REFERENCES providers(id) ON DELETE CASCADE,
    client_id UUID NOT NULL REFERENCES clients(id) ON DELETE CASCADE,
    rating INTEGER NOT NULL CHECK (rating >= 1 AND rating <= 5),
    comment TEXT,
    provider_response TEXT,
    provider_response_at TIMESTAMP WITH TIME ZONE,
    is_visible BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Índices para reseñas
CREATE INDEX idx_reviews_provider ON reviews(provider_id);
CREATE INDEX idx_reviews_client ON reviews(client_id);
CREATE INDEX idx_reviews_rating ON reviews(rating);
CREATE INDEX idx_reviews_created ON reviews(created_at DESC);

-- ============================================
-- FUNCIONES Y TRIGGERS
-- ============================================

-- Función para actualizar updated_at automáticamente
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Triggers para updated_at
CREATE TRIGGER update_users_updated_at
    BEFORE UPDATE ON users
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_clients_updated_at
    BEFORE UPDATE ON clients
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_providers_updated_at
    BEFORE UPDATE ON providers
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_service_requests_updated_at
    BEFORE UPDATE ON service_requests
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_reviews_updated_at
    BEFORE UPDATE ON reviews
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- Función para actualizar rating y review_count del proveedor
CREATE OR REPLACE FUNCTION update_provider_rating()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE providers
    SET 
        rating = (
            SELECT COALESCE(AVG(rating)::DECIMAL(3,2), 0)
            FROM reviews
            WHERE provider_id = COALESCE(NEW.provider_id, OLD.provider_id)
            AND is_visible = TRUE
        ),
        review_count = (
            SELECT COUNT(*)
            FROM reviews
            WHERE provider_id = COALESCE(NEW.provider_id, OLD.provider_id)
            AND is_visible = TRUE
        )
    WHERE id = COALESCE(NEW.provider_id, OLD.provider_id);
    
    RETURN COALESCE(NEW, OLD);
END;
$$ LANGUAGE plpgsql;

-- Trigger para actualizar rating automáticamente
CREATE TRIGGER trigger_update_provider_rating
    AFTER INSERT OR UPDATE OR DELETE ON reviews
    FOR EACH ROW
    EXECUTE FUNCTION update_provider_rating();

-- ============================================
-- DATOS DE EJEMPLO
-- ============================================

-- Insertar usuarios de ejemplo
INSERT INTO users (id, email, password_hash, name, phone, role, is_active, email_verified) VALUES
    ('a1b2c3d4-e5f6-7890-abcd-ef1234567890', 'carlos.martinez@email.com', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewKyNiLXCJzFgKOe', 'Carlos Martínez', '+52 55 1234 5678', 'provider', TRUE, TRUE),
    ('b2c3d4e5-f6a7-8901-bcde-f12345678901', 'maria.lopez@email.com', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewKyNiLXCJzFgKOe', 'María López', '+52 55 2345 6789', 'provider', TRUE, TRUE),
    ('c3d4e5f6-a7b8-9012-cdef-123456789012', 'jose.hernandez@email.com', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewKyNiLXCJzFgKOe', 'José Hernández', '+52 33 3456 7890', 'provider', TRUE, TRUE),
    ('d4e5f6a7-b8c9-0123-defa-234567890123', 'pedro.gomez@email.com', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewKyNiLXCJzFgKOe', 'Pedro Gómez', '+52 55 1111 2222', 'client', TRUE, TRUE),
    ('e5f6a7b8-c9d0-1234-efab-345678901234', 'sofia.rodriguez@email.com', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewKyNiLXCJzFgKOe', 'Sofía Rodríguez', '+52 55 3333 4444', 'client', TRUE, TRUE),
    ('f6a7b8c9-d0e1-2345-fabc-456789012345', 'admin@servipro.com', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewKyNiLXCJzFgKOe', 'Administrador', NULL, 'admin', TRUE, TRUE);

-- Insertar proveedores
INSERT INTO providers (id, user_id, description, city, zone, rating, review_count, hourly_rate, is_verified, experience, availability) VALUES
    ('p1a2b3c4-d5e6-7890-abcd-111111111111', 'a1b2c3d4-e5f6-7890-abcd-ef1234567890', 'Electricista certificado con más de 10 años de experiencia. Instalaciones eléctricas, reparaciones y mantenimiento preventivo.', 'Ciudad de México', 'Coyoacán', 4.80, 127, 350.00, TRUE, '10 años', 'Lunes a Sábado, 8am - 6pm'),
    ('p2b3c4d5-e6f7-8901-bcde-222222222222', 'b2c3d4e5-f6a7-8901-bcde-f12345678901', 'Servicios profesionales de limpieza para hogares y oficinas. Limpieza profunda, mantenimiento regular y servicios post-obra.', 'Ciudad de México', 'Roma Norte', 4.90, 89, 200.00, TRUE, '5 años', 'Lunes a Viernes, 7am - 5pm'),
    ('p3c4d5e6-f7a8-9012-cdef-333333333333', 'c3d4e5f6-a7b8-9012-cdef-123456789012', 'Albañilería general, construcción y remodelación. Trabajos de pintura interior y exterior con acabados de primera.', 'Guadalajara', 'Zapopan', 4.60, 64, 300.00, TRUE, '15 años', 'Lunes a Sábado, 7am - 4pm');

-- Insertar clientes
INSERT INTO clients (id, user_id, address, city, zone) VALUES
    ('c1a2b3c4-d5e6-7890-abcd-444444444444', 'd4e5f6a7-b8c9-0123-defa-234567890123', 'Av. Universidad 1234, Col. Del Valle', 'Ciudad de México', 'Del Valle'),
    ('c2b3c4d5-e6f7-8901-bcde-555555555555', 'e5f6a7b8-c9d0-1234-efab-345678901234', 'Calle Durango 456, Col. Roma Norte', 'Ciudad de México', 'Roma Norte');

-- Insertar categorías de proveedores
INSERT INTO provider_categories (provider_id, category_id, is_primary) VALUES
    ('p1a2b3c4-d5e6-7890-abcd-111111111111', 5, TRUE),  -- Electricista (primary)
    ('p1a2b3c4-d5e6-7890-abcd-111111111111', 4, FALSE), -- Plomero
    ('p2b3c4d5-e6f7-8901-bcde-222222222222', 6, TRUE),  -- Limpieza
    ('p3c4d5e6-f7a8-9012-cdef-333333333333', 2, TRUE),  -- Albañil (primary)
    ('p3c4d5e6-f7a8-9012-cdef-333333333333', 3, FALSE); -- Pintor

-- Insertar solicitudes de servicio
INSERT INTO service_requests (id, client_id, provider_id, category_id, title, description, city, zone, address, status, budget, preferred_date) VALUES
    ('sr1a2b3c-d5e6-7890-abcd-666666666666', 'c1a2b3c4-d5e6-7890-abcd-444444444444', 'p1a2b3c4-d5e6-7890-abcd-111111111111', 5, 'Instalación de lámparas LED', 'Necesito instalar 5 lámparas LED en el techo de la sala y comedor. Ya tengo las lámparas.', 'Ciudad de México', 'Coyoacán', 'Av. Universidad 1234, Col. Del Valle', 'pending', 1500.00, '2024-02-15'),
    ('sr2b3c4d-e6f7-8901-bcde-777777777777', 'c2b3c4d5-e6f7-8901-bcde-555555555555', 'p2b3c4d5-e6f7-8901-bcde-222222222222', 6, 'Limpieza profunda departamento', 'Departamento de 80m2, necesita limpieza profunda incluyendo ventanas y cocina.', 'Ciudad de México', 'Roma Norte', 'Calle Durango 456, Col. Roma Norte', 'accepted', 1200.00, '2024-02-12'),
    ('sr3c4d5e-f7a8-9012-cdef-888888888888', 'c1a2b3c4-d5e6-7890-abcd-444444444444', 'p3c4d5e6-f7a8-9012-cdef-333333333333', 2, 'Construcción de barda', 'Necesito construir una barda perimetral de aproximadamente 15 metros lineales.', 'Guadalajara', 'Zapopan', 'Av. Patria 789, Zapopan', 'completed', 25000.00, '2024-01-20');

-- Insertar reseñas
INSERT INTO reviews (id, service_request_id, provider_id, client_id, rating, comment) VALUES
    ('rv1a2b3c-d5e6-7890-abcd-999999999999', 'sr3c4d5e-f7a8-9012-cdef-888888888888', 'p3c4d5e6-f7a8-9012-cdef-333333333333', 'c1a2b3c4-d5e6-7890-abcd-444444444444', 5, 'Excelente trabajo, muy profesional y puntual. Recomendado al 100%.'),
    ('rv2b3c4d-e6f7-8901-bcde-000000000000', NULL, 'p1a2b3c4-d5e6-7890-abcd-111111111111', 'c2b3c4d5-e6f7-8901-bcde-555555555555', 4, 'Buen servicio, llegó un poco tarde pero el trabajo quedó muy bien.');

-- ============================================
-- CONSULTAS DE EJEMPLO
-- ============================================

-- ============================================
-- 1. CONSULTAS DE USUARIOS Y AUTENTICACIÓN
-- ============================================

-- Buscar usuario por email para login
-- SELECT id, email, password_hash, name, role, is_active 
-- FROM users 
-- WHERE email = 'carlos.martinez@email.com' AND is_active = TRUE;

-- Verificar sesión activa
-- SELECT us.*, u.name, u.role 
-- FROM user_sessions us
-- JOIN users u ON us.user_id = u.id
-- WHERE us.session_token = 'token_aqui' 
-- AND us.expires_at > CURRENT_TIMESTAMP;

-- Obtener perfil completo de usuario cliente
-- SELECT u.*, c.address, c.city, c.zone
-- FROM users u
-- JOIN clients c ON u.id = c.user_id
-- WHERE u.id = 'd4e5f6a7-b8c9-0123-defa-234567890123';

-- ============================================
-- 2. CONSULTAS DE PROVEEDORES
-- ============================================

-- Buscar proveedores por categoría y ciudad
-- SELECT 
--     u.name,
--     u.phone,
--     u.avatar,
--     p.*,
--     ARRAY_AGG(sc.label) as categorias
-- FROM providers p
-- JOIN users u ON p.user_id = u.id
-- JOIN provider_categories pc ON p.id = pc.provider_id
-- JOIN service_categories sc ON pc.category_id = sc.id
-- WHERE p.city = 'Ciudad de México'
-- AND sc.code = 'electricista'
-- AND u.is_active = TRUE
-- GROUP BY u.id, p.id
-- ORDER BY p.rating DESC;

-- Top 10 proveedores mejor calificados
-- SELECT 
--     u.name,
--     p.city,
--     p.rating,
--     p.review_count,
--     p.is_verified
-- FROM providers p
-- JOIN users u ON p.user_id = u.id
-- WHERE u.is_active = TRUE
-- ORDER BY p.rating DESC, p.review_count DESC
-- LIMIT 10;

-- Proveedores verificados con sus categorías
-- SELECT 
--     u.name,
--     u.email,
--     p.city,
--     p.rating,
--     STRING_AGG(sc.label, ', ') as servicios
-- FROM providers p
-- JOIN users u ON p.user_id = u.id
-- JOIN provider_categories pc ON p.id = pc.provider_id
-- JOIN service_categories sc ON pc.category_id = sc.id
-- WHERE p.is_verified = TRUE
-- GROUP BY u.id, p.id
-- ORDER BY p.rating DESC;

-- ============================================
-- 3. CONSULTAS DE SOLICITUDES DE SERVICIO
-- ============================================

-- Solicitudes pendientes para un proveedor
-- SELECT 
--     sr.*,
--     u.name as client_name,
--     u.phone as client_phone,
--     sc.label as category_name
-- FROM service_requests sr
-- JOIN clients c ON sr.client_id = c.id
-- JOIN users u ON c.user_id = u.id
-- JOIN service_categories sc ON sr.category_id = sc.id
-- WHERE sr.provider_id = 'p1a2b3c4-d5e6-7890-abcd-111111111111'
-- AND sr.status = 'pending'
-- ORDER BY sr.created_at DESC;

-- Historial de solicitudes de un cliente
-- SELECT 
--     sr.*,
--     up.name as provider_name,
--     sc.label as category_name
-- FROM service_requests sr
-- JOIN service_categories sc ON sr.category_id = sc.id
-- LEFT JOIN providers p ON sr.provider_id = p.id
-- LEFT JOIN users up ON p.user_id = up.id
-- WHERE sr.client_id = 'c1a2b3c4-d5e6-7890-abcd-444444444444'
-- ORDER BY sr.created_at DESC;

-- Estadísticas de solicitudes por estado
-- SELECT 
--     status,
--     COUNT(*) as total,
--     ROUND(COUNT(*) * 100.0 / SUM(COUNT(*)) OVER(), 2) as porcentaje
-- FROM service_requests
-- GROUP BY status
-- ORDER BY total DESC;

-- ============================================
-- 4. CONSULTAS DE RESEÑAS
-- ============================================

-- Reseñas de un proveedor con información del cliente
-- SELECT 
--     r.*,
--     u.name as client_name,
--     u.avatar as client_avatar
-- FROM reviews r
-- JOIN clients c ON r.client_id = c.id
-- JOIN users u ON c.user_id = u.id
-- WHERE r.provider_id = 'p1a2b3c4-d5e6-7890-abcd-111111111111'
-- AND r.is_visible = TRUE
-- ORDER BY r.created_at DESC;

-- Promedio de calificaciones por categoría
-- SELECT 
--     sc.label as categoria,
--     ROUND(AVG(p.rating), 2) as promedio_rating,
--     SUM(p.review_count) as total_reviews
-- FROM providers p
-- JOIN provider_categories pc ON p.id = pc.provider_id
-- JOIN service_categories sc ON pc.category_id = sc.id
-- WHERE pc.is_primary = TRUE
-- GROUP BY sc.id
-- ORDER BY promedio_rating DESC;

-- ============================================
-- 5. CONSULTAS ADMINISTRATIVAS
-- ============================================

-- Dashboard: Resumen general
-- SELECT 
--     (SELECT COUNT(*) FROM users WHERE role = 'client') as total_clients,
--     (SELECT COUNT(*) FROM users WHERE role = 'provider') as total_providers,
--     (SELECT COUNT(*) FROM service_requests) as total_requests,
--     (SELECT COUNT(*) FROM service_requests WHERE status = 'pending') as pending_requests,
--     (SELECT COUNT(*) FROM service_requests WHERE status = 'completed') as completed_requests,
--     (SELECT COUNT(*) FROM reviews) as total_reviews;

-- Proveedores pendientes de verificación
-- SELECT 
--     u.name,
--     u.email,
--     u.phone,
--     p.city,
--     p.description,
--     p.created_at
-- FROM providers p
-- JOIN users u ON p.user_id = u.id
-- WHERE p.is_verified = FALSE
-- AND u.is_active = TRUE
-- ORDER BY p.created_at ASC;

-- Solicitudes recientes (últimos 7 días)
-- SELECT 
--     sr.id,
--     sr.title,
--     sr.status,
--     sc.label as category,
--     uc.name as client_name,
--     up.name as provider_name,
--     sr.created_at
-- FROM service_requests sr
-- JOIN service_categories sc ON sr.category_id = sc.id
-- JOIN clients c ON sr.client_id = c.id
-- JOIN users uc ON c.user_id = uc.id
-- LEFT JOIN providers p ON sr.provider_id = p.id
-- LEFT JOIN users up ON p.user_id = up.id
-- WHERE sr.created_at >= CURRENT_DATE - INTERVAL '7 days'
-- ORDER BY sr.created_at DESC;

-- ============================================
-- 6. CONSULTAS DE BÚSQUEDA Y FILTRADO
-- ============================================

-- Búsqueda de proveedores con filtros múltiples
-- SELECT 
--     u.id as user_id,
--     u.name,
--     u.phone,
--     u.avatar,
--     p.id as provider_id,
--     p.description,
--     p.city,
--     p.zone,
--     p.rating,
--     p.review_count,
--     p.hourly_rate,
--     p.is_verified,
--     ARRAY_AGG(DISTINCT sc.label) as categories
-- FROM providers p
-- JOIN users u ON p.user_id = u.id
-- JOIN provider_categories pc ON p.id = pc.provider_id
-- JOIN service_categories sc ON pc.category_id = sc.id
-- WHERE u.is_active = TRUE
-- AND (p.city = 'Ciudad de México' OR 'Ciudad de México' IS NULL)
-- AND (p.rating >= 4.0 OR 4.0 IS NULL)
-- AND (p.is_verified = TRUE OR TRUE IS NULL)
-- GROUP BY u.id, p.id
-- HAVING 'electricista' = ANY(ARRAY_AGG(sc.code)) OR 'electricista' IS NULL
-- ORDER BY p.is_verified DESC, p.rating DESC
-- LIMIT 20 OFFSET 0;

-- ============================================
-- VISTAS ÚTILES
-- ============================================

-- Vista: Proveedores con información completa
CREATE OR REPLACE VIEW v_providers_full AS
SELECT 
    u.id as user_id,
    u.email,
    u.name,
    u.phone,
    u.avatar,
    u.is_active,
    u.created_at as user_created_at,
    p.id as provider_id,
    p.description,
    p.city,
    p.zone,
    p.rating,
    p.review_count,
    p.hourly_rate,
    p.is_verified,
    p.experience,
    p.availability,
    ARRAY_AGG(DISTINCT sc.code) as category_codes,
    ARRAY_AGG(DISTINCT sc.label) as category_labels
FROM providers p
JOIN users u ON p.user_id = u.id
LEFT JOIN provider_categories pc ON p.id = pc.provider_id
LEFT JOIN service_categories sc ON pc.category_id = sc.id
GROUP BY u.id, p.id;

-- Vista: Solicitudes con información completa
CREATE OR REPLACE VIEW v_service_requests_full AS
SELECT 
    sr.*,
    sc.code as category_code,
    sc.label as category_label,
    uc.name as client_name,
    uc.phone as client_phone,
    uc.email as client_email,
    up.name as provider_name,
    up.phone as provider_phone,
    pr.rating as provider_rating
FROM service_requests sr
JOIN service_categories sc ON sr.category_id = sc.id
JOIN clients c ON sr.client_id = c.id
JOIN users uc ON c.user_id = uc.id
LEFT JOIN providers pr ON sr.provider_id = pr.id
LEFT JOIN users up ON pr.user_id = up.id;

-- ============================================
-- FIN DEL SCRIPT
-- ============================================
