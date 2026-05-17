using Amazon;
using Amazon.KinesisVideo;
using Amazon.Rekognition;
using Microsoft.EntityFrameworkCore;
using VideoProcessingApp.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. VERİTABANI BAĞLANTISI (PostgreSQL)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. AWS KİMLİK BİLGİLERİNİ OKUMA
var awsOptions = builder.Configuration.GetSection("AWS");
var awsAccessKey = awsOptions["AccessKey"];
var awsSecretKey = awsOptions["SecretKey"];
var awsRegion = RegionEndpoint.GetBySystemName(awsOptions["Region"]);

// 3. AWS SERVİSLERİNİ SİSTEME TANITMA// ŞU KISMI KONTROL ET:
builder.Services.AddSingleton<IAmazonKinesisVideo>(sp => 
    new AmazonKinesisVideoClient(awsAccessKey, awsSecretKey, awsRegion));

builder.Services.AddSingleton<IAmazonRekognition>(sp => 
    new AmazonRekognitionClient(awsAccessKey, awsSecretKey, awsRegion)); 
builder.Services.AddSingleton<Amazon.S3.IAmazonS3>(sp => 
    new Amazon.S3.AmazonS3Client(awsAccessKey, awsSecretKey, awsRegion));
// 4. BİZİM YAZDIĞIMIZ ANALİZ SERVİSİ
builder.Services.AddScoped<VideoProcessingApp.Services.VideoAnalysisService>();

// 5. CONTROLLER DESTEĞİNİ AKTİF ETME
builder.Services.AddControllers(); 
builder.Services.AddOpenApi();

var app = builder.Build();
app.UseDefaultFiles(); // index.html'i otomatik ana sayfa yapar
app.UseStaticFiles();  // wwwroot klasörünü dışarı açar
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// CONTROLLER HARİTASINI ÇIKARMA
app.MapControllers(); 

app.Run();