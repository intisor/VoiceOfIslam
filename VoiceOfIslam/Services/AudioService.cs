using Microsoft.EntityFrameworkCore;
using VoiceOfIslam.Data;
using VoiceOfIslam.Shared.Models;

namespace VoiceOfIslam.Services
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
                _logger.LogError(exception, "Failed to load past lectures from SQL Server.");
                return [];
            }
        }

        public async Task<AudioStream?> GetLiveLecture()
        {
            // 1. Check for a scheduled live stream in the DB (for flexibility)
            try
            {
                await using var context = await _dbFactory.CreateDbContextAsync();
                var dbLive = await context.AudioStreams
                    .AsNoTracking()
                    .Where(audio => audio.IsLive)
                    .OrderByDescending(audio => audio.ScheduledAt)
                    .FirstOrDefaultAsync();
                if (dbLive != null)
                    return dbLive;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to load live lecture from SQL Server.");
            }

            // 2. Only show Bond FM as live on Mondays at 8:30pm WAT (UTC+1)
            var nowUtc = DateTime.UtcNow;
            var watZone = TimeZoneInfo.FindSystemTimeZoneById("W. Central Africa Standard Time");
            var nowWat = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, watZone);
            // Monday = 1, 8:30pm = 20:30
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
                _logger.LogError(exception, "Failed to load {Count} recent lectures from SQL Server.", count);
                return [];
            }
        }

        public async Task<string?> GetAuthorizedPlaybackUrl(Guid audioStreamId)
        {
            // Special-case: Bond FM test live stream
            if (audioStreamId == Guid.Parse("11111111-2222-3333-4444-555555555555"))
            {
                // Return the test stream URL directly (no SAS needed)
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
