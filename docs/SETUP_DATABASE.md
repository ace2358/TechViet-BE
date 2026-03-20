# Database Setup (PostgreSQL)

## 1) Prerequisites
- PostgreSQL installed and running
- Access to `psql`
- Script file at project root: `db-script.sql`

## 2) Create database
Use a database name exactly matching backend config (`tech-viet`):

```sql
CREATE DATABASE "tech-viet";
```

## 3) Run schema script
From project root:

```powershell
psql -h localhost -U postgres -d "tech-viet" -f .\db-script.sql
```

If `psql` is not in PATH, use full path to `psql.exe`.

## 4) Verify

```sql
\c "tech-viet"
\dt
```

You should see tables like `users`, `products`, `orders`, `payment_transactions`.

## Important
Frontend/backend teammate must update connection string locally in `techviet_be/appsettings.Development.json` or environment variables:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=tech-viet;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```
