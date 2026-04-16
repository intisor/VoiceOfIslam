using Microsoft.JSInterop;

namespace VoiceOfIslam.Client.Services
{
    public record SettingsState(bool IsDarkMode, bool NotificationsEnabled);

    public class SettingsService
    {
        private const string DarkModeKey = "voiceofislam.settings.darkMode";
        private const string NotificationsKey = "voiceofislam.settings.notifications";
        private readonly IJSRuntime _jsRuntime;
        private bool _initialized;

        private SettingsState _state = new(false, true);

        public SettingsState State
        {
            get => _state;
            private set
            {
                if (_state != value)
                {
                    _state = value;
                    OnStateChanged?.Invoke();
                }
            }
        }

        public bool IsDarkMode => State.IsDarkMode;
        public bool NotificationsEnabled => State.NotificationsEnabled;

        public event Action? OnStateChanged;

        public SettingsService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task InitializeAsync()
        {
            if (_initialized)
            {
                return;
            }

            var isDark = await GetBooleanAsync(DarkModeKey);
            var notifies = await GetBooleanAsync(NotificationsKey, defaultValue: true);
            
            State = new SettingsState(isDark, notifies);
            
            await ApplyThemeAsync(isDark);
            _initialized = true;
        }

        public async Task SetDarkModeAsync(bool enabled)
        {
            State = State with { IsDarkMode = enabled };
            await SetBooleanAsync(DarkModeKey, enabled);
            await ApplyThemeAsync(enabled);
        }

        public async Task SetNotificationsAsync(bool enabled)
        {
            State = State with { NotificationsEnabled = enabled };
            await SetBooleanAsync(NotificationsKey, enabled);
        }

        private async Task<bool> GetBooleanAsync(string key, bool defaultValue = false)
        {
            var rawValue = await _jsRuntime.InvokeAsync<string?>("voiceOfIslamStorage.get", key);
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return defaultValue;
            }

            return bool.TryParse(rawValue, out var parsed) ? parsed : defaultValue;
        }

        private Task SetBooleanAsync(string key, bool value)
        {
            return _jsRuntime.InvokeVoidAsync("voiceOfIslamStorage.set", key, value.ToString()).AsTask();
        }

        private Task ApplyThemeAsync(bool enabled)
        {
            return _jsRuntime.InvokeVoidAsync("voiceOfIslamStorage.applyTheme", enabled).AsTask();
        }
    }
}