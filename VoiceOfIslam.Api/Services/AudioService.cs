using Microsoft.EntityFrameworkCore;
using VoiceOfIslam.Api.Data;
using VoiceOfIslam.Shared.Models;

namespace VoiceOfIslam.Api.Services
{
    public class AudioService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private readonly BlobSasService _blobSasService;
        private readonly ILogger<AudioService> _logger;

        public AudioService(
            IDbContextFactory<AppDbContext> dbContext,
            BlobSasService blobSasService,
            ILogger<AudioService> logger)
        {
            _dbFactory = dbContext;
            _blobSasService = blobSasService;
            _logger = logger;
        }

        public async Task<List<AudioStream>> GetPastAudios()
        {
            try
            {
                await using var context = await _dbFactory.CreateDbContextAsync();
                return await context.AudioStreams
                    .AsNoTracking()
                    .OrderByDescending(audio => audio.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to load past lectures from the database.");
                return new List<AudioStream>();
            }
        }

        public async Task<AudioStream?> GetLiveLecture()
        {
            try
            {
                await using var context = await _dbFactory.CreateDbContextAsync();
                var dbLive = await context.AudioStreams
                    .AsNoTracking()
                    .Where(audio => audio.IsLive)
                    .OrderByDescending(audio => audio.ScheduledAt)
                    .FirstOrDefaultAsync();
                if (dbLive != null)
                {
                    return dbLive;
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to load live lecture from the database.");
            }

            var nowUtc = DateTime.UtcNow;
            var watZone = TimeZoneInfo.FindSystemTimeZoneById("W. Central Africa Standard Time");
            var nowWat = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, watZone);

            if (nowWat.DayOfWeek == DayOfWeek.Monday && nowWat.Hour == 20 && nowWat.Minute >= 30 && nowWat.Minute < 60)
            {
                return new AudioStream
                {
                    Id = Guid.Parse("11111111-2222-3333-4444-555555555555"),
                    Title = "Bond FM Live",
                    Description = "Bond FM Monday Live Stream",
                    BlobUrl = "https://go.webgateready.com/bondfm/radio.mp3",
                    CreatedAt = nowUtc,
                    ScheduledAt = nowUtc,
                    IsLive = true,
                    Speaker = "Bond FM",
                    Duration = TimeSpan.Zero
                };
            }

            return null;
        }

        public async Task<List<AudioStream>> GetRecentLectures(int count)
        {
            try
            {
                await using var context = await _dbFactory.CreateDbContextAsync();
                return await context.AudioStreams
                    .AsNoTracking()
                    .OrderByDescending(audio => audio.CreatedAt)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to load {Count} recent lectures from the database.", count);
                return new List<AudioStream>();
            }
        }

        public async Task<AudioStream?> GetAudioStreamById(Guid audioStreamId)
        {
            try
            {
                await using var context = await _dbFactory.CreateDbContextAsync();
                return await context.AudioStreams
                    .AsNoTracking()
                    .FirstOrDefaultAsync(audio => audio.Id == audioStreamId);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to load audio stream by id {AudioStreamId}.", audioStreamId);
                return null;
            }
        }

        public async Task<string?> GetAuthorizedPlaybackUrl(Guid audioStreamId)
        {
            if (audioStreamId == Guid.Parse("11111111-2222-3333-4444-555555555555"))
            {
                return "https://go.webgateready.com/bondfm/radio.mp3";
            }

            try
            {
                await using var context = await _dbFactory.CreateDbContextAsync();
                var blobUrl = await context.AudioStreams
                    .AsNoTracking()
                    .Where(audio => audio.Id == audioStreamId)
                    .Select(audio => audio.BlobUrl)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrWhiteSpace(blobUrl))
                {
                    return null;
                }

                if (_blobSasService.TryCreateReadSasUrl(blobUrl, out var signedUrl))
                {
                    return signedUrl;
                }

                return null;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to get authorized playback URL for {AudioStreamId}", audioStreamId);
                return null;
            }
        }
    }
}
