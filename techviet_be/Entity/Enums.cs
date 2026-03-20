namespace techviet_be.Entity;

public enum UserRole
{
    USER,
    ADMIN
}

public enum OrderStatus
{
    PENDING,
    PAID,
    SHIPPING,
    COMPLETED,
    CANCELLED
}

public enum PaymentStatus
{
    PENDING,
    PAID,
    FAILED
}

public enum PaymentProvider
{
    VNPAY
}

public enum PaymentTransactionStatus
{
    PENDING,
    SUCCESS,
    FAILED
}