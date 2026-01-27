PatternNET - .NET 8 Clean Architecture Template

PatternNET adalah template starter-kit untuk membangun aplikasi Backend yang scalable, modular, dan mudah di-maintain menggunakan teknologi .NET 8.

Project ini menerapkan prinsip Domain-Driven Design (DDD) dan Clean Architecture secara ketat, memisahkan aturan bisnis murni dari kerumitan teknis (seperti Database atau Framework).

🚀 Teknologi Utama

Project ini dibangun menggunakan stack teknologi modern standar industri:

Framework: .NET 8 (LTS)

Language: C# 12

Database: Entity Framework Core 8 (SQL Server)

Pola Desain: CQRS (Command Query Responsibility Segregation) via MediatR

Validasi: Domain Validations

Dokumentasi API: Swagger UI (OpenAPI)

Dependency Injection: Native .NET DI Container

📂 Struktur Arsitektur

Struktur solusi ini mengikuti aturan Dependency Rule: Layer dalam tidak boleh bergantung pada layer luar.

src/Modules/PatternNET
│
├── 1. PatternNET.Domain (Jantung Aplikasi) ❤️
│   │  Layer terdalam. Murni logic C#, tidak ada dependency ke project lain.
│   ├── Entities/       # Object bisnis utama (misal: ContactMessage.cs)
│   ├── ValueObjects/   # Object tanpa identitas
│   ├── Events/         # Domain Events (Komunikasi antar domain)
│   └── Exceptions/     # Error khusus bisnis
│
├── 2. PatternNET.Application (Otak Aplikasi) 🧠
│   │  Mengatur alur kerja (Orchestration). Bergantung HANYA pada Domain.
│   ├── UseCases/       # Logika fitur (Commands & Queries)
│   ├── Interfaces/     # Kontrak Repository (IRepository)
│   └── DependencyInjection.cs
│
├── 3. PatternNET.Infrastructure (Gudang & Alat) 🏭
│   │  Implementasi teknis. Bergantung pada Domain & Application.
│   ├── Persistence/    # DbContext & Konfigurasi Tabel EF Core
│   ├── Repositories/   # Implementasi Interface Repository
│   └── DependencyInjection.cs
│
└── 4. PatternNET.API (Pintu Depan) 🚪
    │  Entry point aplikasi. Bergantung pada Application & Infrastructure.
    ├── Controllers/    # Menerima HTTP Request
    ├── Dtos/           # Request/Response Json Contract
    └── Program.cs      # Konfigurasi awal (Startup)


🛠️ Prasyarat (Requirements)

Pastikan komputer Anda sudah terinstall:

.NET 8 SDK

SQL Server Express (atau LocalDB)

Visual Studio 2022 atau VS Code.

Git.

⚡ Cara Menjalankan (Getting Started)

Ikuti langkah ini untuk menjalankan aplikasi di mesin lokal.

1. Clone Repository

git clone [https://github.com/USERNAME/PatternNET.git](https://github.com/USERNAME/PatternNET.git)
cd PatternNET


2. Konfigurasi Database

Buka file src/Modules/PatternNET/PatternNET.API/appsettings.json.
Sesuaikan ConnectionStrings dengan server database lokal Anda.

"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=PatternNET_Db;Trusted_Connection=True;TrustServerCertificate=True;"
}


3. Jalankan Migrasi (Wajib)

Karena kita memisahkan project Infrastructure (Tempat DB) dan API (Tempat Startup), perintah migrasinya harus spesifik.

Buka terminal di folder src/Modules/PatternNET, lalu jalankan:

# 1. Membuat file migrasi baru (Snapshot)
dotnet ef migrations add InitialCreate --project PatternNET.Infrastructure --startup-project PatternNET.API

# 2. Menerapkan ke Database SQL Server (Update DB)
dotnet ef database update --project PatternNET.Infrastructure --startup-project PatternNET.API


4. Jalankan Aplikasi

dotnet run --project PatternNET.API


Akses Swagger UI di: https://localhost:7XXX/swagger/index.html

📝 Panduan Pengembangan (How-To)

Ingin menambahkan fitur baru? Ikuti alur "Dari Dalam ke Luar" ini agar kodingan tetap rapi.

Contoh: Menambah Fitur "Simpan Produk"

Domain:

Buat Product.cs di folder Entities. Pastikan ada validasi logic di constructor-nya.

Application:

Buat IProductRepository.cs di Interfaces.

Buat CreateProductCommand dan CreateProductHandler di UseCases.

Infrastructure:

Buat konfigurasi tabel EF Core di Persistence.

Implementasikan ProductRepository.cs.

Jalankan perintah dotnet ef migrations add AddProductEntity ....

API:

Buat CreateProductRequest.cs (DTO).

Buat ProductController.cs dan panggil Handler via MediatR.

🤝 Kontribusi

Jika ingin mengembangkan template ini, silakan Fork repository ini dan buat Pull Request baru.

📄 Lisensi

Project ini dilisensikan di bawah MIT License.
