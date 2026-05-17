using Amazon.Rekognition;
using VideoProcessingApp.Data;
using VideoProcessingApp.Models;

namespace VideoProcessingApp.Services;

public class VideoAnalysisService
{
    private readonly AppDbContext _context;

    public VideoAnalysisService(AppDbContext context)
    {
        _context = context;
    }

    // VideoTimestampMills parametresini ekledik
    public async Task SaveResultAsync(string streamName, string label, double confidence, long timestampMills)
    {
        var result = new AnalysisResult
        {
            StreamName = streamName,
            Label = label,
            Confidence = confidence,
            DetectedAt = DateTime.UtcNow,
            VideoTimestampMills = timestampMills 
        };

        _context.AnalysisResults.Add(result);
        await _context.SaveChangesAsync();
    }
}