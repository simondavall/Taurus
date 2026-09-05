using FluentValidation;

namespace Taurus.Components.Features.Shared;

public interface IMudValidator<in T>: IValidator<T>
{
    public Func<object, string, Task<IEnumerable<string>>> ValidateValueAsync { get; }
}