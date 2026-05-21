namespace CASA_Client.Services;

public sealed class ThemeState
{
    private bool _isDarkMode;

    public bool IsDarkMode => _isDarkMode;
    public string ThemeName => _isDarkMode ? "dark" : "light";

    public event Action? OnChange;

    public void SetDarkMode(bool isDarkMode)
    {
        if (_isDarkMode == isDarkMode)
        {
            return;
        }

        _isDarkMode = isDarkMode;
        OnChange?.Invoke();
    }

    public void Toggle()
    {
        SetDarkMode(!_isDarkMode);
    }
}
