namespace Taurus.Application;

public sealed record ApplicationResult(bool Succeeded, string? ErrorMessage)
{
    public static ApplicationResult Success()
    {
        return new ApplicationResult(true, null);
    }

    public static ApplicationResult Failure(string errorMessage)
    {
        return new ApplicationResult(false, errorMessage);
    }
}

public sealed record ApplicationResult<T>(bool Succeeded, T? Value, string? ErrorMessage)
{
    public static ApplicationResult<T> Success(T value)
    {
        return new ApplicationResult<T>(true, value, null);
    }

    public static ApplicationResult<T> Failure(string errorMessage)
    {
        return new ApplicationResult<T>(false, default, errorMessage);
    }
}