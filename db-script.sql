-- =========================
-- USERS
-- =========================
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username VARCHAR(50) UNIQUE NOT NULL,
    fullname VARCHAR(100) NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    phone VARCHAR(20),
    address TEXT,
    password_hash TEXT NOT NULL,
    role VARCHAR(20) NOT NULL DEFAULT 'USER',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =========================
-- PRODUCTS
-- =========================
CREATE TABLE products (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    brand VARCHAR(100),
    category VARCHAR(100),
    description TEXT,
    price NUMERIC(12,2) NOT NULL,
    original_price NUMERIC(12,2),
    discount_percent NUMERIC(5,2) DEFAULT 0,
    stock_qty INT DEFAULT 0,
    rating_avg NUMERIC(3,2) DEFAULT 0,
    review_count INT DEFAULT 0,
    is_active BOOLEAN DEFAULT TRUE
);

-- =========================
-- PRODUCT IMAGES
-- =========================
CREATE TABLE product_images (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id UUID REFERENCES products(id) ON DELETE CASCADE,
    image_url TEXT NOT NULL,
    sort_order INT DEFAULT 0
);

-- =========================
-- PRODUCT SPECS
-- =========================
CREATE TABLE product_specs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id UUID REFERENCES products(id) ON DELETE CASCADE,
    spec_label VARCHAR(100),
    spec_value TEXT,
    sort_order INT DEFAULT 0
);

-- =========================
-- CART
-- =========================
CREATE TABLE carts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID UNIQUE REFERENCES users(id) ON DELETE CASCADE
);

CREATE TABLE cart_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    cart_id UUID REFERENCES carts(id) ON DELETE CASCADE,
    product_id UUID REFERENCES products(id),
    quantity INT NOT NULL CHECK (quantity > 0),
    variant_info JSONB,
    UNIQUE(cart_id, product_id, variant_info)
);

-- =========================
-- ORDERS
-- =========================
CREATE TABLE orders (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES users(id),
    fullname VARCHAR(100),
    phone VARCHAR(20),
    address TEXT,
    note TEXT,
    payment_method VARCHAR(50),
    payment_status VARCHAR(50),
    order_status VARCHAR(50),
    subtotal NUMERIC(12,2),
    shipping_fee NUMERIC(12,2),
    total NUMERIC(12,2),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE order_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID REFERENCES orders(id) ON DELETE CASCADE,
    product_id UUID REFERENCES products(id),
    quantity INT NOT NULL,
    price NUMERIC(12,2) NOT NULL
);

-- =========================
-- REVIEWS
-- =========================
CREATE TABLE reviews (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id UUID REFERENCES products(id) ON DELETE CASCADE,
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    rating INT CHECK (rating BETWEEN 1 AND 5),
    comment TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(product_id, user_id)
);

-- =========================
-- PAYMENT TRANSACTIONS (NEW)
-- =========================
CREATE TABLE payment_transactions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID REFERENCES orders(id) ON DELETE CASCADE,
    provider VARCHAR(20) NOT NULL, -- vnpay
    transaction_id TEXT,
    status VARCHAR(20) NOT NULL, -- pending, success, failed
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =========================
-- INDEXES (IMPORTANT)
-- =========================
CREATE INDEX idx_products_category ON products(category);
CREATE INDEX idx_products_brand ON products(brand);
CREATE INDEX idx_products_price ON products(price);

CREATE INDEX idx_cart_user ON carts(user_id);
CREATE INDEX idx_order_user ON orders(user_id);

CREATE INDEX idx_review_product ON reviews(product_id);
CREATE INDEX idx_payment_order_id ON payment_transactions(order_id);