using System;
using VoiceOfIslam.Shared.Models;

namespace VoiceOfIslam.Services
{
    public record AudioPlayerState(
        AudioStream? CurrentTrack,
        bool IsPlaying,
        double Progress,
        double PlaybackSpeed
    );

    public class AudioPlayerService
    {
        private AudioPlayerState _state = new(null, false, 0, 1.0);

        public AudioPlayerState State
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

        public AudioStream? CurrentTrack => State.CurrentTrack;
        public bool IsPlaying => State.IsPlaying;
        public double Progress => State.Progress; // 0.0 to 1.0
        public double PlaybackSpeed => State.PlaybackSpeed;

        public event Action? OnStateChanged;

        public void PlayTrack(AudioStream track)
        {
            if (State.CurrentTrack?.Id != track.Id)
            {
                State = State with { CurrentTrack = track, IsPlaying = true, Progress = 0 };
            }
            else
            {
                State = State with { IsPlaying = true };
            }
        }

        public void Pause()
        {
            State = State with { IsPlaying = false };
        }

        public void Stop()
        {
            State = new(null, false, 0, 1.0);
        }

        public void SetProgress(double progress)
        {
            State = State with { Progress = progress };
        }

        public void SetPlaybackSpeed(double speed)
        {
            State = State with { PlaybackSpeed = speed };
        }
    }
}
