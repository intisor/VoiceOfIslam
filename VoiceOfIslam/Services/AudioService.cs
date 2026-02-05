using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using VoiceOfIslam.Data;
using VoiceOfIslam.Shared.Models;

namespace VoiceOfIslam.Services
{
    public class AudioService 
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;

        public AudioService(IDbContextFactory<AppDbContext> dbContext)
        {
            _dbFactory = dbContext;
        }
        public async Task<List<AudioStream>> GetPastAudios()
        {
            var context = _dbFactory.CreateDbContext();
            return await context.AudioStreams.AsNoTracking().ToListAsync(); 
        }
    }
}
