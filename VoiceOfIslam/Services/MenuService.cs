using System;

namespace VoiceOfIslam.Services
{
    public record MenuState(bool IsMenuOpen);

    public class MenuService
    {
        private MenuState _state = new(false);

        public MenuState State
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

        public bool IsMenuOpen => State.IsMenuOpen;

        public event Action? OnStateChanged;

        public void ToggleMenu()
        {
            State = State with { IsMenuOpen = !State.IsMenuOpen };
        }

        public void CloseMenu()
        {
            if (State.IsMenuOpen)
            {
                State = State with { IsMenuOpen = false };
            }
        }
    }
}
