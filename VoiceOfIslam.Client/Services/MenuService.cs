using System;

namespace VoiceOfIslam.Client.Services
{
    public class MenuService
    {
        public bool IsMenuOpen { get; private set; }
        public event Action? OnStateChanged;

        public void ToggleMenu()
        {
            IsMenuOpen = !IsMenuOpen;
            OnStateChanged?.Invoke();
        }

        public void CloseMenu()
        {
            if (IsMenuOpen)
            {
                IsMenuOpen = false;
                OnStateChanged?.Invoke();
            }
        }
    }
}
