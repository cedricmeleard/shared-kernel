# SharedKernel.Results

`SharedKernel.Results` is a lightweight, zero-dependency .NET 11 library providing a functional `Result` and `Result<TValue>` pattern built on C# 15 `union` types (`Success`, `Success<TValue>`, `Error`) along with Railway-Oriented Programming (ROP) combinators for explicit error handling without throwing costly exceptions.

---

## 🚀 Features

- **`Result` & `Result<TValue>` Unions**: C# 15 `union` types (`Result(Success, Error)` and `Result<TValue>(Success<TValue>, Error)`) supporting exhaustive `switch` expressions, implicit `Error` conversion, and `IResult<TSelf>`.
- **`Error` Record**: Structural representation of errors containing `Code` and `Name`.
- **Railway-Oriented Programming (ROP) Combinators**:
  - **`Match`**: Branch execution based on success or failure.
  - **`Bind`**: Chain operations returning a `Result`.
  - **`Map`**: Transform success values.
  - **`Tap`**: Execute side-effects on success without altering the result.
  - **`Ensure`**: Validate predicates on success values.

---

## 💻 Usage Examples

### 1. Returning Results from Domain Logic

```csharp
using SharedKernel.Results;

public class BankAccount
{
    public decimal Balance { get; private set; }

    public Result Withdraw(decimal amount)
    {
        if (amount <= 0)
        {
            return new Error("Amount.Invalid", "Withdrawal amount must be greater than zero.");
        }

        if (amount > Balance)
        {
            return new Error("Balance.Insufficient", "Insufficient funds for withdrawal.");
        }

        Balance -= amount;
        return Result.Success();
    }
}
```

### 2. Railway-Oriented Programming (ROP) Pipeline

```csharp
public async Task<IResult> ProcessOrderAsync(OrderRequest request)
{
    return await ValidateRequest(request)
        .Bind(req => CalculateTotal(req))
        .Tap(order => logger.LogInformation("Order total calculated: {Total}", order.Total))
        .Ensure(order => order.Total > 0, new Error("Order.InvalidTotal", "Order total must be positive"))
        .Match(
            successOrder => TypedResults.Ok(successOrder),
            error => error.ToProblemDetails());
}
```

### 3. Exhaustive C# 15 Pattern Matching

```csharp
var response = result switch
{
    Success<Order>(var order) => TypedResults.Ok(order),
    Error error => error.ToProblemDetails()
};
```
