using System.Net.Http.Json;
using VoiceOfIslam.Shared.Models;

namespace VoiceOfIslam.Client.Services
{
    public class LectureService
    {
        private readonly HttpClient _httpClient;
        private List<AudioStream>? _cachedLectures;

        public LectureService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<AudioStream>> GetAllLectures()
        {
            if (_cachedLectures is not null)
            {
                return _cachedLectures;
            }

            _cachedLectures = await _httpClient.GetFromJsonAsync<List<AudioStream>>("api/audio-streams") ?? [];
            return _cachedLectures;
        }

        public async Task<AudioStream?> GetLiveLecture()
        {
            return await _httpClient.GetFromJsonAsync<AudioStream?>("api/audio-streams/live");
        }

        public async Task<List<AudioStream>> GetRecentLectures(int count)
        {
            return await _httpClient.GetFromJsonAsync<List<AudioStream>>($"api/audio-streams/recent/{count}") ?? [];
        }

        public async Task<string?> GetPlaybackUrl(Guid audioStreamId)
        {
            var response = await _httpClient.GetFromJsonAsync<PlaybackUrlResponse>($"api/audio-streams/{audioStreamId}/playback-url");
            return string.IsNullOrWhiteSpace(response?.Url) ? null : response.Url;
        }

        public async Task<List<AudioStream>> Search(string query)
        {
            var normalizedQuery = query.Trim();
            if (string.IsNullOrWhiteSpace(normalizedQuery))
            {
                return await GetAllLectures();
            }

            var lectures = await GetAllLectures();
            return lectures
                .Where(lecture =>
                    lecture.Title.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) ||
                    lecture.Speaker.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) ||
                    lecture.Description.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}