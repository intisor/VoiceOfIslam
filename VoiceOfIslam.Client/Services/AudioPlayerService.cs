using System;
using VoiceOfIslam.Shared.Models;

namespace VoiceOfIslam.Client.Services
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
                State = State with { IsPlaying = !State.IsPlaying };
            }
        }

        public void TogglePlay()
        {
            if (State.CurrentTrack != null)
            {
                State = State with { IsPlaying = !State.IsPlaying };
            }
        }

        public void SetProgress(double progress)
        {
            State = State with { Progress = Math.Clamp(progress, 0, 1) };
        }

        public void AdjustProgressBySeconds(double seconds)
        {
            var durationSeconds = State.CurrentTrack?.Duration.TotalSeconds ?? 0;
            if (durationSeconds <= 0)
            {
                SetProgress(State.Progress + (seconds / 100));
                return;
            }

            var currentSeconds = State.Progress * durationSeconds;
            var updatedSeconds = Math.Clamp(currentSeconds + seconds, 0, durationSeconds);
            State = State with { Progress = updatedSeconds / durationSeconds };
        }

        public void CyclePlaybackSpeed()
        {
            var newSpeed = State.PlaybackSpeed switch
            {
                < 1.0 => 1.0,
                >= 2.0 => 1.0,
                < 1.25 => 1.25,
                < 1.5 => 1.5,
                _ => 2.0
            };

            State = State with { PlaybackSpeed = newSpeed };
        }

        public void Stop()
        {
            State = new AudioPlayerState(null, false, 0, 1.0);
        }
    }
}
