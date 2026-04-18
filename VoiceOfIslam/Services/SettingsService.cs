using Microsoft.JSInterop;

namespace VoiceOfIslam.Services
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
            if (_initialized) return;
            _initialized = true;
            var darkModeStr = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", DarkModeKey);
            var notificationsStr = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", NotificationsKey);
            var darkMode = darkModeStr == "true";
            var notifications = notificationsStr == null ? true : notificationsStr == "true";
            State = new(darkMode, notifications);
        }

        public async Task SetDarkMode(bool value)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", DarkModeKey, value);
            State = State with { IsDarkMode = value };
        }

        public async Task SetNotificationsEnabled(bool value)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", NotificationsKey, value);
            State = State with { NotificationsEnabled = value };
        }
    }
}
