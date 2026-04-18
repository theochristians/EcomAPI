# EComAPI Test Guide

Dokumen ini menjelaskan struktur test, cara menjalankan test, dan smoke-test command yang dipakai di project EComAPI saat ini.

## Struktur Test

```
tests/
|-- EComAPI.Application.Tests/
|   |-- Auth/
|   |-- Categories/
|   |-- Products/
|   |-- Shopping/
|   |-- Transaction/
|   `-- Common/
`-- EComAPI.API.Tests/
    |-- Auth/
    |-- Addresses/
    |-- Categories/
    |-- Products/
    |-- Shopping/
    |-- Transaction/
    `-- Infrastructure/
```

## Framework yang Digunakan

- xUnit
- FluentAssertions
- Moq
- AutoFixture
- Microsoft.AspNetCore.Mvc.Testing
- EF Core InMemory (untuk integration test)
- Coverlet collector

## Cara Menjalankan Test

### 1. Semua test

```powershell
dotnet test
```

### 2. Application tests saja

```powershell
dotnet test tests/EComAPI.Application.Tests/EComAPI.Application.Tests.csproj
```

### 3. API integration tests saja

```powershell
dotnet test tests/EComAPI.API.Tests/EComAPI.API.Tests.csproj
```

### 4. Test berdasarkan nama class

```powershell
dotnet test tests/EComAPI.Application.Tests/EComAPI.Application.Tests.csproj --filter "FullyQualifiedName~RegisterUserHandlerTests"
```

### 5. Test dengan code coverage

```powershell
dotnet test --collect:"XPlat Code Coverage"
```

## Smoke Test Commands (yang umum dipakai sebelum deploy)

### API smoke suite terarah

```powershell
dotnet test tests/EComAPI.API.Tests/EComAPI.API.Tests.csproj --filter "FullyQualifiedName~AuthControllerTests|FullyQualifiedName~CategoriesControllerTests|FullyQualifiedName~ProductsControllerTests|FullyQualifiedName~OrderControllerTests|FullyQualifiedName~CouponControllerTests|FullyQualifiedName~ReviewControllerTests|FullyQualifiedName~ReturnControllerTests|FullyQualifiedName~StockLogControllerTests"
```

### Application smoke suite terarah (auth + token)

```powershell
dotnet test tests/EComAPI.Application.Tests/EComAPI.Application.Tests.csproj --filter "FullyQualifiedName~LogoutUserHandlerTests|FullyQualifiedName~RefreshTokenHandlerTests|FullyQualifiedName~TokenHelperTests"
```

## Saran Urutan Validasi Sebelum Deploy

1. `dotnet restore`
2. `dotnet build EComAPI.API/EComAPI.API.csproj`
3. `dotnet test tests/EComAPI.Application.Tests/EComAPI.Application.Tests.csproj`
4. `dotnet test tests/EComAPI.API.Tests/EComAPI.API.Tests.csproj`
5. Manual smoke via endpoint kritikal (lihat `EComAPI.API/LIVE_DEMO_TESTING.md`)

## Catatan

- Integration test berjalan di environment `Test` (rate limiter dimatikan).
- Jika ada perubahan kontrak endpoint, update test API dan live demo doc sekaligus.
- Jika ada perubahan DI constructor handler/repository, update unit test mocks yang terdampak.

## Referensi Tambahan

- Tutorial dasar testing: [tests/TUTORIAL.md](TUTORIAL.md)
