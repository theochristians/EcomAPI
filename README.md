# PatternNET – .NET 8 Clean Architecture Template

PatternNET adalah **starter template** untuk membangun aplikasi backend .NET 8 yang **scalable, modular, dan mudah di‑maintain** menggunakan prinsip **Domain‑Driven Design (DDD)** dan **Clean Architecture**.

Template ini dirancang agar:

* aturan bisnis (Domain) **terpisah total** dari detail teknis (DB, framework),
* struktur konsisten dan mudah di‑copy untuk project berikutnya,
* siap dikembangkan menjadi **Microservices** bila dibutuhkan.

---

## 🚀 Teknologi Utama

* **Framework**: .NET 8 (LTS)
* **Language**: C# 12
* **Database**: Entity Framework Core 8 (SQL Server)
* **Architecture**: Clean Architecture + DDD
* **Pattern**: CQRS (Command & Query Separation)
* **Validation**: Domain Validation (Entity & Value Object)
* **Dependency Injection**: Native .NET DI
* **API Docs**: Swagger / OpenAPI

---

## 📂 Struktur Arsitektur

> **Dependency Rule**
> Layer bagian dalam **tidak boleh bergantung** pada layer luar.

```
src
└─ Modules
   └─ PatternNET
      ├─ PatternNET.Domain
      ├─ PatternNET.Application
      ├─ PatternNET.Infrastructure
      └─ PatternNET.API
```

---

## 1️⃣ PatternNET.Domain – Jantung Aplikasi ❤️

Layer terdalam. **Tidak bergantung pada project lain**.
Hanya berisi aturan bisnis murni.

```
PatternNET.Domain
├─ Entities        # Objek bisnis utama (punya identitas)
├─ ValueObjects    # Nilai penting tanpa identitas
├─ Events          # Domain Events (opsional)
└─ Exceptions      # Error khusus bisnis
```

**Aturan penting:**

* ❌ Tidak boleh ada EF Core
* ❌ Tidak boleh ada HTTP / Controller
* ❌ Tidak boleh ada LINQ ke DB

**Contoh isi:**

* `ContactMessage.cs`
* `Email.cs`
* `DomainException.cs`

---

## 2️⃣ PatternNET.Application – Otak Aplikasi 🧠

Mengatur **alur kerja (use case)**.
Bergantung **hanya pada Domain**.

```
PatternNET.Application
├─ UseCases
│  ├─ Commands      # Aksi tulis (Create, Update, Delete)
│  └─ Queries       # Aksi baca (Get, List)
├─ Interfaces       # Kontrak Repository
└─ DependencyInjection.cs
```

**Aturan penting:**

* ✔ Mengatur urutan proses
* ✔ Memanggil Domain
* ✔ Memanggil Repository (via interface)
* ❌ Tidak ada EF Core / SQL

---

## 3️⃣ PatternNET.Infrastructure – Gudang & Alat 🏭

Berisi **implementasi teknis**.
Bergantung pada Domain & Application.

```
PatternNET.Infrastructure
├─ Persistence
│  ├─ Configurations   # Mapping Entity → Table (Fluent API)
│  ├─ Repositories     # Implementasi IRepository
│  └─ AppDbContext.cs
├─ Migrations          # Versi perubahan struktur DB
└─ DependencyInjection.cs
```

**Aturan penting:**

* ✔ EF Core di sini
* ✔ LINQ ke DB di sini
* ❌ Tidak ada aturan bisnis

---

## 4️⃣ PatternNET.API – Pintu Depan 🚪

Entry point aplikasi.
Menangani HTTP Request & Response.

```
PatternNET.API
├─ Controllers     # Endpoint HTTP
├─ Dtos
│  ├─ Requests     # Input JSON
│  └─ Responses    # Output JSON
└─ Program.cs      # Startup & konfigurasi
```

**Aturan penting:**

* ✔ Mapping DTO → Command
* ✔ Panggil UseCase
* ❌ Tidak ada query DB
* ❌ Tidak ada aturan bisnis

---

## 🔁 Alur Request (Flow Standar)

```
Client
 ↓
API (Controller)
 ↓
Application (UseCase)
 ↓
Domain (Validasi & Aturan)
 ↓
Infrastructure (EF Core / DB)
 ↓
Database
 ↓
Response kembali ke API
```

---

## 🛠️ Prasyarat (Requirements)

* .NET 8 SDK
* SQL Server / SQL Server Express / LocalDB
* Visual Studio 2022 atau VS Code
* Git

---

## ⚡ Cara Menjalankan (Getting Started)

### 1. Clone Repository

```
git clone https://github.com/USERNAME/PatternNET.git
cd PatternNET
```

---

### 2. Konfigurasi Database

Edit file:

```
src/Modules/PatternNET/PatternNET.API/appsettings.json
```

Contoh:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=PatternNET_Db;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

---

### 3. Jalankan Migration (Wajib)

Karena **Infrastructure terpisah dari API**, perintah migration harus eksplisit:

```
dotnet ef migrations add InitialCreate \
  --project PatternNET.Infrastructure \
  --startup-project PatternNET.API


dotnet ef database update \
  --project PatternNET.Infrastructure \
  --startup-project PatternNET.API
```

---

### 4. Jalankan Aplikasi

```
dotnet run --project PatternNET.API
```

Akses Swagger UI:

```
https://localhost:7XXX/swagger/index.html
```

---

## 🧑‍💻 Panduan Pengembangan Fitur Baru

Gunakan prinsip **dari dalam ke luar**.

### Contoh: Menambah Fitur "Create Product"

**1. Domain**

* Buat `Product.cs` (Entity)
* Tambahkan validasi di constructor

**2. Application**

* Buat `IProductRepository`
* Buat `CreateProductCommand` & `CreateProductHandler`

**3. Infrastructure**

* Buat konfigurasi EF Core
* Implementasi `ProductRepository`
* Jalankan migration

**4. API**

* Buat `CreateProductRequest`
* Buat `ProductController`

---

## 🧭 Aturan Emas Penempatan Kode

| Jika menulis…   | Taruh di       |
| --------------- | -------------- |
| Aturan bisnis   | Domain         |
| Urutan proses   | Application    |
| EF / LINQ / SQL | Infrastructure |
| HTTP / JSON     | API            |

---

## 🤝 Kontribusi

Silakan fork repository ini dan buat Pull Request jika ingin mengembangkan template.

---

## 📄 Lisensi

Template ini menggunakan **MIT License**.
