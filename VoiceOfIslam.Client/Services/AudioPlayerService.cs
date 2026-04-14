using System;
using VoiceOfIslam.Shared.Models;

namespace VoiceOfIslam.Client.Services
{
    public class AudioPlayerService
    {
        public AudioStream? CurrentTrack { get; private set; }
        public bool IsPlaying { get; private set; }
        public double Progress { get; private set; } // 0.0 to 1.0

        public event Action? OnStateChanged;

        public void PlayTrack(AudioStream track)
        {
            if (CurrentTrack?.Id != track.Id)
            {
                CurrentTrack = track;
                IsPlaying = true;
                Progress = 0;
            }
            else
            {
                IsPlaying = !IsPlaying;
            }
            NotifyStateChanged();
        }

        public void TogglePlay()
        {
            if (CurrentTrack != null)
            {
                IsPlaying = !IsPlaying;
                NotifyStateChanged();
            }
        }

        public void SetProgress(double progress)
        {
            Progress = progress;
            NotifyStateChanged();
        }

        public void Stop()
        {
            CurrentTrack = null;
            IsPlaying = false;
            Progress = 0;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnStateChanged?.Invoke();
    }
}
