# 📹 AI Video Analysis & Metadata Management System

Bu proje; AWS S3 üzerindeki `.mp4` formatındaki videoları, **AWS Rekognition (Computer Vision)** yapay zeka servisiyle asenkron olarak baştan sona tarayan, nesneleri tespit eden ve bu verileri **PostgreSQL** veritabanında depolayan modern bir **.NET Backend** ve **Bootstrap Frontend** uygulamasıdır.

Uygulama, veritabanının gereksiz yere şişmesini önlemek amacıyla, peş peşe gelen mükerrer verileri engelleyen özel bir **Smart Deduplication (Akıllı Tekilleştirme)** algoritmasına sahiptir.

---

## 🛠️ Teknolojiler ve Mimari

* **Backend:** .NET 9.0 Web API (Controllers Architecture)
* **Frontend:** HTML5, CSS3 (Bootstrap 5), JavaScript (Fetch API)
* **Veritabanı:** PostgreSQL (Docker üzerinde container olarak çalışmaktadır)
* **ORM:** Entity Framework Core (Code-First)
* **Bulut Servisleri (Cloud):** * **AWS S3:** Video dosyalarının güvenli depolanması.
  * **AWS Rekognition:** Video üzerinde nesne algılama (Object & Label Detection).

---

## 🧠 Öne Çıkan Mühendislik Çözümleri

### 1. Asenkron Video İşleme (Job Polling)
Büyük video dosyalarının analizi zaman alabileceğinden, sistem AWS Rekognition üzerinde asenkron bir `StartLabelDetection` işi (Job) başlatır. Backend, AWS sunucularını arka planda bloklamadan belirli aralıklarla sorgulayarak (`Polling`), iş bittiğinde sonuçları teslim alır.

### 2. Akıllı Filtreleme / Debouncing Algoritması
Yapay zeka videoyu kare kare tararken, aynı nesneyi (örneğin "Cat") videonun ardışık saniyelerinde yüzlerce kez raporlayabilir. Sistem, veritabanını çöplüğe çevirmemek için bir hafıza sözlüğü (`Dictionary`) tutar:
* Eğer aynı nesne **5 saniye (5000 ms)** içinde tekrar tespit edilirse, veritabanına yazılmadan **pas geçilir**.
* Bu sayede veritabanı boyutu optimize edilir ve anlamlı zaman damgaları (`VideoTimestampMills`) elde edilir.

---

## 🚀 Sistemi Yerelde Çalıştırma Rehberi

### 1. Veritabanını Ayağa Kaldırma (Docker)
Projenin PostgreSQL veritabanını Docker üzerinde başlatmak için terminalden şu komutu çalıştırın:
```bash
docker run --name video-postgres -e POSTGRES_PASSWORD=#### -p 5433:5432 -d postgres
