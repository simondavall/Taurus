namespace Taurus.Components.Pages;

public partial class Home
{
    private bool _isVerified;

    private Task VerifyLayoutAsync()
    {
        _isVerified = true;

        return Task.CompletedTask;
    }
}