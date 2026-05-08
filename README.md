# Thrifties E-Commerce Backend API (EComAPI)

![.NET Version](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-success)
![License](https://img.shields.io/badge/License-MIT-blue)

Backend API untuk platform e-commerce **Thrifties**, dibangun menggunakan **.NET 8** dan mengimplementasikan **Clean Architecture**. Sistem ini dirancang dengan fokus pada skalabilitas, maintainability, clean code, dan keamanan yang siap digunakan di environment production.

---

## Daftar Isi

- [Arsitektur](#arsitektur)
- [Teknologi Utama](#teknologi-utama)
- [Fitur Utama](#fitur-utama)
- [Prasyarat](#prasyarat)
- [Struktur Proyek](#struktur-proyek)
- [Cara Menjalankan Proyek](#cara-menjalankan-proyek)
- [Konfigurasi Environment](#konfigurasi-environment)
- [Pengujian (Testing)](#pengujian-testing)
- [CI/CD Pipeline](#cicd-pipeline)
- [Dokumentasi Tambahan](#dokumentasi-tambahan)

---

## Arsitektur

Solusi ini memisahkan concern secara ketat mengikuti prinsip **Clean Architecture**, yang terbagi menjadi beberapa layer:

- **`EComAPI.Domain`**: Core dari sistem. Berisi Entity, Value Object, Enum, dan aturan bisnis dasar. Tidak bergantung pada infrastruktur maupun external framework apapun.
- **`EComAPI.Application`**: Use Case aplikasi (Implementasi CQRS dengan Commands/Queries), Contract/Interfaces, DTOs, dan validasi bisnis. Layer ini mengkoordinasikan interaksi dari layer Domain.
- **`EComAPI.Infrastructure`**: Implementasi teknis eksternal seperti Entity Framework Core (Database), Repository, Caching (Redis), integrasi Email SMTP, dan Security (JWT, Hashing).
- **`EComAPI.API`**: Presentation layer berupa HTTP Web API. Menangani routing, auth/authorization middleware, konfigurasi Swagger, error handling global, dan rate limiting.
- **`tests/EComAPI.Application.Tests`**: Unit Tests komprehensif untuk layer Application.
- **`tests/EComAPI.API.Tests`**: Integration Tests end-to-end untuk endpoints API.

---

## Teknologi Utama

- **Framework**: .NET 8 (C# 12)
- **Database**: SQL Server & Entity Framework Core 8
- **Caching**: Redis (Upstash) dengan In-Memory Cache Fallback
- **Security**: JWT Authentication, BCrypt Password Hashing, Role & Permission-based Authorization
- **Storage**: Azure Blob Storage (SAS Token generation for direct client upload)
- **Email Service**: Brevo SMTP (dilengkapi Fallback Console Logger untuk dev lokal)
- **Testing**: xUnit, Moq, FluentAssertions
- **Dokumentasi API**: Swagger / OpenAPI

---

## Fitur Utama

- **Authentication & Authorization**: JWT (Register, Login, Refresh Token, Logout), OTP Forgot & Reset Password.
- **Keamanan**: Token Blacklist (DB + Redis), Rate Limiting per endpoint (Brute-force protection), Strict Permission Handling.
- **Verifikasi Email**: Registrasi diwajibkan melalui proses verifikasi Email OTP.
- **Manajemen User**: Manajemen Profil pengguna dan multi-Alamat pengiriman.
- **Katalog Produk**: Manajemen Kategori, Produk, Varian, dan integrasi gambar eksternal (Azure Blob).
- **Shopping & Order**: Cart (Keranjang Belanja), Wishlist, Checkout, dan pembuatan Order (Pesanan).
- **Transaksi**: Sistem Kupon Diskon, Ulasan produk (Review), Pengembalian barang (Return), Tracking Status Order, dan Log Stok otomatis (Stock Auditing).

---

## Prasyarat

Pastikan sistem Anda telah menginstal komponen berikut sebelum menjalankan aplikasi:
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (Express / Developer / Azure SQL)
- *(Opsional)* Redis server atau Upstash account
- *(Opsional)* Azure Storage Account (untuk upload gambar)
- *(Opsional)* Akun SMTP (misal Brevo) untuk pengiriman email

---

## Struktur Proyek

```text
EComAPI/
├── src/
│   ├── EComAPI.API/            # Presentation Layer
│   ├── EComAPI.Application/    # Business Logic Layer
│   ├── EComAPI.Domain/         # Core Domain Layer
│   └── EComAPI.Infrastructure/ # Data Access & External Services Layer
├── tests/
│   ├── EComAPI.API.Tests/         # Integration Tests
│   └── EComAPI.Application.Tests/ # Unit Tests
├── .env.example                # Template Environment Variables
├── azure-pipelines.yml         # CI/CD Azure DevOps Pipeline
└── EComAPI.sln                 # .NET Solution File
```

---

## Cara Menjalankan Proyek

1. **Clone Repository dan Masuk ke Direktori**
   ```bash
   git clone <url-repo>
   cd EComAPI
   ```

2. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

3. **Terapkan Migrasi Database**
   Pastikan connection string sudah dikonfigurasi dengan benar sebelum menjalankan perintah ini.
   ```bash
   dotnet ef database update --project EComAPI.Infrastructure --startup-project EComAPI.API
   ```

4. **Jalankan Aplikasi**
   ```bash
   dotnet run --project EComAPI.API
   ```

5. **Akses Swagger UI**
   Buka browser dan akses salah satu URL berikut untuk dokumentasi interaktif dan uji coba API:
   - `http://localhost:5006/swagger`
   - `https://localhost:7091/swagger`

> **Catatan Seeder**: Saat startup pertama kali, API akan secara otomatis menjalankan *Seeder* untuk membuat Role, Permission dasar, dan user `SYSTEM`.

---

## Konfigurasi Environment

Aplikasi ini menggunakan banyak variabel lingkungan. Untuk pengembangan lokal, sangat disarankan menggunakan **Secret Manager**, sedangkan untuk Staging/Production menggunakan Environment Variables.

### Menggunakan .NET User Secrets (Local Dev)
Buka terminal di dalam folder `EComAPI.API` dan jalankan:
```bash
# Konfigurasi Database
dotnet user-secrets set "ConnectionStrings:SQLServerThrifted" "Server=.;Database=ThriftedDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;"

# Konfigurasi JWT
dotnet user-secrets set "JwtSettings:Secret" "<YOUR_SUPER_SECRET_KEY_MIN_32_CHARS>"
```

### Variabel Lingkungan Tersedia

Berikut adalah tabel variabel utama yang dibutuhkan *(Selengkapnya dapat dilihat pada file `.env.example`)*:

| Kategori | Kunci Konfigurasi | Deskripsi |
|-------|----------------------------|-----------|
| **Database** | `ConnectionStrings:SQLServerThrifted` | Connection string SQL Server utama |
| **JWT** | `JwtSettings:Secret`, `Issuer`, `Audience` | Pengaturan enkripsi dan validitas JWT |
| **Redis** | `Redis:Enabled`, `Redis:Url`, `Redis:Token` | Konfigurasi Upstash REST / Redis |
| **Email** | `Email:SmtpHost`, `Email:SmtpUsername`, dst | SMTP credentials (Brevo) untuk OTP Email |
| **Azure Blob** | `AzureBlobStorage:AccountName`, dst | Kredensial generate SAS URI upload file ke Azure |
| **Rate Limit** | `RateLimiting:AuthLogin:PermitLimit`, dst | Limitasi jumlah request endpoint kritikal |

---

## Pengujian (Testing)

Proyek ini dilengkapi dengan Unit Test dan Integration Test untuk memastikan kualitas kode tetap terjaga.

**Menjalankan Seluruh Test:**
```bash
dotnet test
```

**Menjalankan Spesifik Test Project:**
```bash
# Menjalankan Unit Tests (Application Layer)
dotnet test tests/EComAPI.Application.Tests/EComAPI.Application.Tests.csproj

# Menjalankan Integration Tests (API Layer)
dotnet test tests/EComAPI.API.Tests/EComAPI.API.Tests.csproj
```

*(Dokumentasi detail mengenai testing dapat dibaca di [tests/README.md](tests/README.md))*

---

## CI/CD Pipeline

Proyek ini telah memiliki skenario integrasi yang terhubung dengan **Azure DevOps Pipeline** (`azure-pipelines.yml`).
Pipeline akan dieksekusi secara otomatis dan mencakup tahapan berikut:
1. Menggunakan self-hosted agent pool dari variable `AgentPoolName`.
2. Restore `.sln` dan dependencies.
3. Build Project (dengan konfigurasi `Release`).
4. Eksekusi Unit Test `EComAPI.Application.Tests`.
5. Eksekusi Integration Test otomatis (jika variabel pipeline `runApiIntegrationTests` bernilai `true`).
6. Publikasi Test Result (`.trx`) dan Code Coverage (berformat Cobertura).

---

## Dokumentasi Tambahan

Untuk panduan operasional lebih lanjut, silakan baca dokumen berikut:

- **[Demo & Smoke Testing Manual](EComAPI.API/LIVE_DEMO_TESTING.md)** - Urutan flow manual sistem end-to-end (Auth -> Catalog -> Order -> Review -> Return -> Logout).
- **[Timezone Policy](TIME_POLICY.md)** - Aturan standardisasi waktu UTC vs Jakarta Time (WIB) pada sistem dan database.

---

> *Dibuat dengan dedikasi tinggi sebagai Backend Portofolio berskala Enterprise berbasis **.NET Clean Architecture**.*
