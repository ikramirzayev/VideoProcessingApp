namespace VideoProcessingApp.Models;

public class AnalysisResult
{
    public int Id { get; set; } 
    public string? StreamName { get; set; } 
    public string? Label { get; set; }      
    public double Confidence { get; set; } 
    public DateTime DetectedAt { get; set; } 
    public long VideoTimestampMills { get; set; } // YENİ: Videonun kaçıncı milisaniyesinde tespit edildi?
}