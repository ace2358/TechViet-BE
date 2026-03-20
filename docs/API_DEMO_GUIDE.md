# API Demo Guide (Frontend Handoff)

## Base URL
- Local: `http://127.0.0.1:53011`
- Swagger UI: `http://127.0.0.1:53011/`

## Ready-made demo script
Use `techviet_be/techviet_be.http`.
Run requests top-to-bottom in VS Code REST Client.

## What access token contains
JWT claims currently include:
- `sub`: user id (Guid)
- `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier`: user id
- `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name`: username
- `http://schemas.microsoft.com/ws/2008/06/identity/claims/role`: role (`USER`/`ADMIN`)

Use token as:

```http
Authorization: Bearer <token>
```

## Main API purposes

### Auth
- `POST /api/auth/register`: register normal user (requires username, fullname, email, phone, address, password)
- `POST /api/auth/register-admin`: register admin
- `POST /api/auth/login`: login with `usernameOrEmail` + `password`
- `GET /api/auth/profile`: get current user profile

### Products
- `GET /api/products`: list products with filters + pagination
- `GET /api/products/{id}`: product details
- `POST /api/products` (ADMIN): create product
- `PUT /api/products/{id}` (ADMIN): update product
- `DELETE /api/products/{id}` (ADMIN): soft delete product

### Product Specs
- `GET /api/products/{id}/specs`
- `POST /api/products/{id}/specs` (ADMIN)
- `PUT /api/products/{id}/specs/{specId}` (ADMIN)
- `DELETE /api/products/{id}/specs/{specId}` (ADMIN)

### Image Upload
- `POST /api/upload` (multipart/form-data, requires `productId`, `file`, optional `sortOrder`)
- `POST /api/products/{id}/images/upload` (ADMIN, multipart/form-data)

### Cart
- `GET /api/cart`: current user cart
- `GET /api/cart/user/{userId}` (ADMIN): cart by userId
- `POST /api/cart/items`
- `PUT /api/cart/items/{id}`
- `DELETE /api/cart/items/{id}`

### Orders
- `POST /api/orders`: create order from cart
- `GET /api/orders?pageNumber=1&pageSize=10`: current user orders (paginated)
- `GET /api/orders/user/{userId}?pageNumber=1&pageSize=10` (ADMIN)
- `PUT /api/orders/{orderId}/status` (ADMIN): update order status (`PAID`, `SHIPPING`, `COMPLETED`, `CANCELLED`)

### Payment
- `POST /api/payment/vnpay/create`: create payment URL
- `GET /api/payment/vnpay/callback`: VNPAY callback
- `PUT /api/payment/vnpay/transactions/{paymentTransactionId}/status` (ADMIN): update tx status (`PAID` or `FAILED`)

### Users (Management)
- `GET /api/users` (ADMIN): paginated users
- `GET /api/users/{userId}` (ADMIN)
- `PUT /api/users/{userId}` (ADMIN)
- `DELETE /api/users/{userId}` (ADMIN)
