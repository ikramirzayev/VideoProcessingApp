using Microsoft.AspNetCore.Mvc;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using VideoProcessingApp.Services;
using VideoProcessingApp.Data;
using Microsoft.EntityFrameworkCore;

namespace VideoProcessingApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VideoController : ControllerBase
{
    private readonly VideoAnalysisService _analysisService;
    private readonly IAmazonRekognition _rekognitionClient;
    private readonly AppDbContext _context;

    public VideoController(VideoAnalysisService analysisService, IAmazonRekognition rekognitionClient, AppDbContext context)
    {
        _analysisService = analysisService;
        _rekognitionClient = rekognitionClient;
        _context = context;
    }

    // 1. LİNK: S3'teki .mp4 videosunu 5 saniyelik akıllı filtreyle analiz eder
    [HttpGet("video-analiz-et")]
    public async Task<IActionResult> VideoAnalizEt([FromQuery] string videoAdi)
    {
        try
        {
            var startRequest = new StartLabelDetectionRequest
            {
                Video = new Video { S3Object = new S3Object { Bucket = "ikram-video-isleme-bucket-2026", Name = videoAdi } },
                MinConfidence = 75F 
            };

            var startResponse = await _rekognitionClient.StartLabelDetectionAsync(startRequest);
            string jobId = startResponse.JobId; 

            GetLabelDetectionResponse GetResultsResponse = null!;
            string status = "IN_PROGRESS";
            while (status == "IN_PROGRESS")
            {
                await Task.Delay(5000); 
                var getRequest = new GetLabelDetectionRequest { JobId = jobId };
                GetResultsResponse = await _rekognitionClient.GetLabelDetectionAsync(getRequest);
                status = GetResultsResponse.JobStatus.Value; 
            }

            if (status == "SUCCEEDED")
            {
                int toplamGelen = GetResultsResponse.Labels.Count;
                int kaydedilenSayisi = 0;
                var sonKayitZamanlari = new Dictionary<string, long>();

                foreach (var labelDetection in GetResultsResponse.Labels)
                {
                    var label = labelDetection.Label;
                    long suAnkiMilisaniye = labelDetection.Timestamp ?? 0; 

                    // Akıllı Filtreleme (Deduplication) Mantığı
                    if (sonKayitZamanlari.TryGetValue(label.Name, out long sonKayitZamani))
                    {
                        if (suAnkiMilisaniye - sonKayitZamani < 5000)
                        {
                            continue; // 5 saniyeden kısa sürede aynı şey geldiyse veritabanına yazma, pas geç!
                        }
                    }

                    await _analysisService.SaveResultAsync($"S3-Video-{videoAdi}", label.Name, (double)label.Confidence, suAnkiMilisaniye);
                    
                    sonKayitZamanlari[label.Name] = suAnkiMilisaniye;
                    kaydedilenSayisi++;
                }

                return Ok(new { 
                    mesaj = "Akıllı video analizi tamamlandı ve PostgreSQL'e kaydedildi!", 
                    awsGelenToplamNesne = toplamGelen,
                    veritabaninaYazilanTekilNesne = kaydedilenSayisi,
                    engellenenGereksizTekrar = toplamGelen - kaydedilenSayisi
                });
            }

            return BadRequest(new { hata = "AWS videoyu analiz edemedi." });
        }
        catch (Exception ex) { return BadRequest(new { hata = ex.Message }); }
    }

    // 2. LİNK: S3 içindeki tüm yüklediğin resim/video dosyalarını listeler
    [HttpGet("s3-dosyalari-listele")]
    public async Task<IActionResult> S3DosyalariListele()
    {
        try
        {
            var s3Client = HttpContext.RequestServices.GetRequiredService<Amazon.S3.IAmazonS3>();
            var request = new Amazon.S3.Model.ListObjectsV2Request { BucketName = "ikram-video-isleme-bucket-2026" };
            var response = await s3Client.ListObjectsV2Async(request);

            var dosyaListesi = response.S3Objects.Select(dosya => new
            {
                DosyaAdi = dosya.Key,
                BoyutBayt = dosya.Size,
                YuklemeTarihi = dosya.LastModified
            }).ToList();

            return Ok(dosyaListesi);
        }
        catch (Exception ex) { return BadRequest(new { hata = ex.Message }); }
    }

    // 3. LİNK: PostgreSQL veritabanındaki tüm geçmiş kayıtları listeler
    [HttpGet("gecmis-listele")]
    public async Task<IActionResult> GecmisListele()
    {
        var sonuclar = await _context.AnalysisResults.ToListAsync();
        return Ok(sonuclar);
    }
}