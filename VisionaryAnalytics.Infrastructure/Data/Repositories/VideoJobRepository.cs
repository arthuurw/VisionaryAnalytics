using MongoDB.Driver;
using VisionaryAnalytics.Domain.Entities;
using VisionaryAnalytics.Domain.Interfaces.Repositories;

namespace VisionaryAnalytics.Infrastructure.Data.Repositories
{
    public class VideoJobRepository : IVideoJobRepository
    {
        private readonly IMongoCollection<VideoJob> _videoJobCollection;

        public VideoJobRepository(IMongoDatabase database)
        {
            _videoJobCollection = database.GetCollection<VideoJob>("jobs");
        }

        public async Task AtualizarAsync(VideoJob job)
        {
            await _videoJobCollection.ReplaceOneAsync(j => j.Id == job.Id, job);
        }

        public async Task CriarAsync(VideoJob job)
        {
            await _videoJobCollection.InsertOneAsync(job);
        }

        public async Task<VideoJob?> ObterPorIdAsync(string id)
        {
            return await _videoJobCollection.Find(job => job.Id == id).FirstOrDefaultAsync();
        }
    }
}
