# EComAPI Live Demo Testing Guide

Dokumen ini untuk testing live demo API secara end-to-end dari layer API.

## 1. Prasyarat

1. Pastikan database bisa diakses dari connection string di `EComAPI.API/appsettings.json`.
2. Jalankan API:

```powershell
dotnet run --project EComAPI.API
```

3. Base URL (development):
- `https://localhost:7091`
- `http://localhost:5006`

Agar paling mudah untuk demo dari terminal tanpa isu certificate, pakai `http://localhost:5006`.

## 2. Akun Admin Seed (untuk endpoint ber-permission)

Saat aplikasi start, seeder membuat admin default (lihat `AdminSeeder`):

- Email: `theochristian.sir@ecomapi.com`
- Password: `Admin123!`

## 3. Endpoint Matrix (Method, Path, Auth, Body)

### 3.1 Auth

| Method | Path | Auth | Body |
|---|---|---|---|
| POST | `/api/auth/register` | No | `fullName`, `email`, `password` |
| POST | `/api/auth/login` | No | `email`, `password`, `deviceName` |
| POST | `/api/auth/refresh-token` | No | `refreshToken` |
| POST | `/api/auth/logout` | Bearer | `refreshToken` (opsional) |

### 3.2 Users

| Method | Path | Auth | Body |
|---|---|---|---|
| GET | `/api/users/me` | Bearer + permission `users.read.own` | - |
| PATCH | `/api/users/me` | Bearer + permission `users.update.own` | `fullName`, `email`, `phone`, `avatar`, `dateOfBirth`, `gender` |
| POST | `/api/users/me/email-verification/send` | Bearer + permission `users.update.own` | - || POST | `/api/users/me/email-verification/send` | Bearer + permission `users.update.own` | - |
| POST | `/api/users/me/email-verification/verify` | Bearer + permission `users.update.own` | `code` || POST | `/api/users/me/email-verification/verify` | Bearer + permission `users.update.own` | `code` |
| DELETE | `/api/users/me` | Bearer + permission `users.delete.own` | - |

### 3.3 Addresses

| Method | Path | Auth | Body |
|---|---|---|---|
| GET | `/api/addresses` | Bearer | - |
| GET | `/api/addresses/default` | Bearer | - |
| POST | `/api/addresses` | Bearer | `label`, `recipientName`, `recipientPhone`, `fullAddress`, `city`, `province`, `postalCode`, `isDefault` |
| PATCH | `/api/addresses/{id}` | Bearer | `label`, `recipientName`, `recipientPhone`, `fullAddress`, `city`, `province`, `postalCode` |
| POST | `/api/addresses/{id}/set-default` | Bearer | - |
| DELETE | `/api/addresses/{id}` | Bearer | - |

### 3.4 Categories

| Method | Path | Auth | Body |
|---|---|---|---|
| GET | `/api/categories` | No | - |
| GET | `/api/categories/{slug}` | No | - |
| POST | `/api/categories` | Bearer + permission `categories.create` | `name`, `slug`, `parentId`, `imageUrl`, `description` |
| PATCH | `/api/categories/{id}` | Bearer + permission `categories.update` | `name`, `slug`, `parentId`, `imageUrl`, `description` |
| DELETE | `/api/categories/{id}` | Bearer + permission `categories.delete` | - |
| POST | `/api/categories/{id}/restore` | Bearer + permission `categories.restore` | - |

### 3.5 Products

| Method | Path | Auth | Body |
|---|---|---|---|
| GET | `/api/products` | No | Query string |
| GET | `/api/products/{slug}` | No | - |
| POST | `/api/products` | Bearer + permission `products.create` | `categoryId`, `name`, `slug`, `basePrice`, `description` |
| PATCH | `/api/products/{id}` | Bearer + permission `products.update` | `name`, `slug`, `basePrice`, `categoryId`, `description` |
| PATCH | `/api/products/{id}/deactivate` | Bearer + permission `products.update` | - |
| PATCH | `/api/products/{id}/activate` | Bearer + permission `products.update` | - |
| DELETE | `/api/products/{id}` | Bearer + permission `products.delete` | - |
| POST | `/api/products/{id}/restore` | Bearer + permission `products.restore` | - |
| POST | `/api/products/{productId}/variants` | Bearer + permission `products.create` | `sku`, `stock`, `priceAdjustment`, `size`, `color` |
| PATCH | `/api/products/variants/{id}` | Bearer + permission `products.update` | `sku`, `stock`, `priceAdjustment`, `size`, `color` |
| DELETE | `/api/products/variants/{id}` | Bearer + permission `products.delete` | - |
| POST | `/api/products/variants/{id}/restore` | Bearer + permission `products.update` | - |
| POST | `/api/products/{productId}/images` | Bearer + permission `products.create` | `imageUrl`, `isPrimary`, `displayOrder` |
| PATCH | `/api/products/images/{id}` | Bearer + permission `products.update` | `imageUrl`, `isPrimary`, `displayOrder` |
| DELETE | `/api/products/images/{id}` | Bearer + permission `products.delete` | - |
| POST | `/api/products/images/{id}/restore` | Bearer + permission `products.update` | - |

## 4. Testing Satu-Satu di Postman

### 4.1 Postman Environment Variables

Buat environment di Postman dan isi:

- `baseUrl` = `http://localhost:5006`
- `adminEmail` = `theochristian.sir@ecomapi.com`
- `adminPassword` = `Admin123!`
- `userEmail` = email user test kamu
- `userPassword` = password user test kamu

Variabel yang akan terisi dari response:

- `adminAccessToken`
- `userAccessToken`
- `userRefreshToken`
- `verificationCode`
- `categoryId`
- `categorySlug`
- `productId`
- `productSlug`
- `variantId`
- `imageId`
- `addressId`

### 4.2 Urutan Test Recommended

1. `Auth` (login admin dan/atau register user)
2. `Users Email Verification` (send code + verify code)
3. `Categories`
4. `Products`
5. `Product Variants`
6. `Product Images`
7. `Users Me`
8. `Addresses`
9. `Refresh Token` dan `Logout`

### 4.3 Request Detail Per Endpoint

Format umum request di Postman:

- URL: `{{baseUrl}}/api/...`
- Header JSON: `Content-Type: application/json`
- Header auth endpoint protected: `Authorization: Bearer {{token}}`

### A. Auth

1. Register
- Method: `POST`
- URL: `{{baseUrl}}/api/auth/register`
- Auth: none
- Body:

```json
{
  "fullName": "Demo User",
  "email": "{{userEmail}}",
  "password": "{{userPassword}}"
}
```

2. Login (Admin)
- Method: `POST`
- URL: `{{baseUrl}}/api/auth/login`
- Auth: none
- Body:

```json
{
  "email": "{{adminEmail}}",
  "password": "{{adminPassword}}",
  "deviceName": "Postman Admin"
}
```

3. Login (User)
- Method: `POST`
- URL: `{{baseUrl}}/api/auth/login`
- Auth: none
- Body:

```json
{
  "email": "{{userEmail}}",
  "password": "{{userPassword}}",
  "deviceName": "Postman User"
}
```

4. Refresh Token
- Method: `POST`
- URL: `{{baseUrl}}/api/auth/refresh-token`
- Auth: none
- Body:

```json
{
  "refreshToken": "{{userRefreshToken}}"
}
```

5. Logout
- Method: `POST`
- URL: `{{baseUrl}}/api/auth/logout`
- Auth: Bearer `{{userAccessToken}}`
- Body:

```json
{
  "refreshToken": "{{userRefreshToken}}"
}
```

### B. Users

1. Get Own Profile
- Method: `GET`
- URL: `{{baseUrl}}/api/users/me`
- Auth: Bearer `{{userAccessToken}}`
- Body: none

2. Update Own Profile
- Method: `PATCH`
- URL: `{{baseUrl}}/api/users/me`
- Auth: Bearer `{{userAccessToken}}`
- Body:

```json
{
  "fullName": "Demo User Updated",
  "email": "{{userEmail}}",
  "phone": "081234567890",
  "avatar": "https://example.com/avatar.jpg",
  "dateOfBirth": "1998-01-15T00:00:00Z",
  "gender": "Male"
}
```

3. Send Email Verification Code
- Method: `POST`
- URL: `{{baseUrl}}/api/users/me/email-verification/send`
- Auth: Bearer `{{userAccessToken}}`
- Body: none

Catatan:
- Pada implementasi saat ini, OTP dikirim ke log aplikasi (service `ConsoleEmailSender`), jadi ambil kodenya dari terminal API.

4. Verify Email
- Method: `POST`
- URL: `{{baseUrl}}/api/users/me/email-verification/verify`
- Auth: Bearer `{{userAccessToken}}`
- Body:

```json
{
  "code": "{{verificationCode}}"
}
```

5. Delete Own Account
- Method: `DELETE`
- URL: `{{baseUrl}}/api/users/me`
- Auth: Bearer `{{userAccessToken}}`
- Body: none

### B.4 Email Verification

1. Send Email Verification Code
- Method: `POST`
- URL: `{{baseUrl}}/api/users/me/email-verification/send`
- Auth: Bearer `{{userAccessToken}}`
- Body: none
- Response: `{ "expiresAt": "2026-..." }` (OTP valid 10 menit)

```json
{
  "success": true,
  "message": "Verification code sent successfully",
  "data": {
    "expiresAt": "2026-02-27T10:30:00Z"
  }
}
```

2. Verify Email
- Method: `POST`
- URL: `{{baseUrl}}/api/users/me/email-verification/verify`
- Auth: Bearer `{{userAccessToken}}`
- Body:

```json
{
  "code": "A1B2C3"
}
```

- Response sukses:

```json
{
  "success": true,
  "message": "Email verified successfully",
  "data": {
    "isEmailVerified": true
  }
}
```

**Catatan alur:**
1. Register → `IsEmailVerified = false`
2. POST send → OTP digenerate dan dikirim ke email (development: cek console log API)
3. POST verify dengan OTP → `IsEmailVerified = true`
4. Kalau kirim ulang: OTP lama otomatis diinvalidasi, OTP baru dibuat
5. OTP berlaku 10 menit, max 5 percobaan salah

### C. Addresses

1. Create Address
- Method: `POST`
- URL: `{{baseUrl}}/api/addresses`
- Auth: Bearer `{{userAccessToken}}`
- Body:

```json
{
  "label": "Rumah",
  "recipientName": "Demo User",
  "recipientPhone": "081234567890",
  "fullAddress": "Jl. Merdeka No. 10",
  "city": "Bandung",
  "province": "Jawa Barat",
  "postalCode": "40123",
  "isDefault": true
}
```

2. Get All Addresses
- Method: `GET`
- URL: `{{baseUrl}}/api/addresses`
- Auth: Bearer `{{userAccessToken}}`
- Body: none

3. Get Default Address
- Method: `GET`
- URL: `{{baseUrl}}/api/addresses/default`
- Auth: Bearer `{{userAccessToken}}`
- Body: none

4. Update Address
- Method: `PATCH`
- URL: `{{baseUrl}}/api/addresses/{{addressId}}`
- Auth: Bearer `{{userAccessToken}}`
- Body:

```json
{
  "label": "Kantor",
  "recipientName": "Demo User",
  "recipientPhone": "081234567891",
  "fullAddress": "Jl. Asia Afrika No. 25",
  "city": "Bandung",
  "province": "Jawa Barat",
  "postalCode": "40211"
}
```

5. Set Default Address
- Method: `POST`
- URL: `{{baseUrl}}/api/addresses/{{addressId}}/set-default`
- Auth: Bearer `{{userAccessToken}}`
- Body: none

6. Delete Address
- Method: `DELETE`
- URL: `{{baseUrl}}/api/addresses/{{addressId}}`
- Auth: Bearer `{{userAccessToken}}`
- Body: none

### D. Categories

1. Create Category
- Method: `POST`
- URL: `{{baseUrl}}/api/categories`
- Auth: Bearer `{{adminAccessToken}}`
- Body:

```json
{
  "name": "Electronics Demo",
  "slug": "electronics-demo",
  "parentId": null,
  "imageUrl": "https://example.com/cat.jpg",
  "description": "Category demo"
}
```

2. Get All Categories
- Method: `GET`
- URL: `{{baseUrl}}/api/categories`
- Auth: none
- Body: none

3. Get Category By Slug
- Method: `GET`
- URL: `{{baseUrl}}/api/categories/{{categorySlug}}`
- Auth: none
- Body: none

4. Update Category
- Method: `PATCH`
- URL: `{{baseUrl}}/api/categories/{{categoryId}}`
- Auth: Bearer `{{adminAccessToken}}`
- Body:

```json
{
  "name": "Electronics Demo Updated",
  "slug": "electronics-demo-updated",
  "parentId": null,
  "imageUrl": "https://example.com/cat-updated.jpg",
  "description": "Updated category demo"
}
```

5. Delete Category
- Method: `DELETE`
- URL: `{{baseUrl}}/api/categories/{{categoryId}}`
- Auth: Bearer `{{adminAccessToken}}`
- Body: none

6. Restore Category
- Method: `POST`
- URL: `{{baseUrl}}/api/categories/{{categoryId}}/restore`
- Auth: Bearer `{{adminAccessToken}}`
- Body: none

### E. Products

1. Create Product
- Method: `POST`
- URL: `{{baseUrl}}/api/products`
- Auth: Bearer `{{adminAccessToken}}`
- Body:

```json
{
  "categoryId": "{{categoryId}}",
  "name": "Demo Product",
  "slug": "demo-product",
  "basePrice": 1500000,
  "description": "Product untuk test"
}
```

2. Get All Products
- Method: `GET`
- URL: `{{baseUrl}}/api/products?page=1&pageSize=10&sortBy=createdAt&sortOrder=desc`
- Auth: none
- Body: none

3. Get Product By Slug
- Method: `GET`
- URL: `{{baseUrl}}/api/products/{{productSlug}}`
- Auth: none
- Body: none

4. Update Product
- Method: `PATCH`
- URL: `{{baseUrl}}/api/products/{{productId}}`
- Auth: Bearer `{{adminAccessToken}}`
- Body:

```json
{
  "name": "Demo Product Updated",
  "slug": "demo-product-updated",
  "basePrice": 1600000,
  "categoryId": "{{categoryId}}",
  "description": "Updated product"
}
```

5. Deactivate Product
- Method: `PATCH`
- URL: `{{baseUrl}}/api/products/{{productId}}/deactivate`
- Auth: Bearer `{{adminAccessToken}}`
- Body: none

6. Activate Product
- Method: `PATCH`
- URL: `{{baseUrl}}/api/products/{{productId}}/activate`
- Auth: Bearer `{{adminAccessToken}}`
- Body: none

7. Delete Product
- Method: `DELETE`
- URL: `{{baseUrl}}/api/products/{{productId}}`
- Auth: Bearer `{{adminAccessToken}}`
- Body: none

8. Restore Product
- Method: `POST`
- URL: `{{baseUrl}}/api/products/{{productId}}/restore`
- Auth: Bearer `{{adminAccessToken}}`
- Body: none

### F. Product Variants

1. Add Variant
- Method: `POST`
- URL: `{{baseUrl}}/api/products/{{productId}}/variants`
- Auth: Bearer `{{adminAccessToken}}`
- Body:

```json
{
  "sku": "DEMO-SKU-001",
  "stock": 10,
  "priceAdjustment": 100000,
  "size": "42",
  "color": "Black"
}
```

2. Update Variant
- Method: `PATCH`
- URL: `{{baseUrl}}/api/products/variants/{{variantId}}`
- Auth: Bearer `{{adminAccessToken}}`
- Body:

```json
{
  "sku": "DEMO-SKU-001-UPDATED",
  "stock": 12,
  "priceAdjustment": 120000,
  "size": "42",
  "color": "Black"
}
```

3. Remove Variant
- Method: `DELETE`
- URL: `{{baseUrl}}/api/products/variants/{{variantId}}`
- Auth: Bearer `{{adminAccessToken}}`
- Body: none

4. Restore Variant
- Method: `POST`
- URL: `{{baseUrl}}/api/products/variants/{{variantId}}/restore`
- Auth: Bearer `{{adminAccessToken}}`
- Body: none

### G. Product Images

1. Add Image
- Method: `POST`
- URL: `{{baseUrl}}/api/products/{{productId}}/images`
- Auth: Bearer `{{adminAccessToken}}`
- Body:

```json
{
  "imageUrl": "https://example.com/product-1.jpg",
  "isPrimary": true,
  "displayOrder": 1
}
```

2. Update Image
- Method: `PATCH`
- URL: `{{baseUrl}}/api/products/images/{{imageId}}`
- Auth: Bearer `{{adminAccessToken}}`
- Body:

```json
{
  "imageUrl": "https://example.com/product-1-updated.jpg",
  "isPrimary": true,
  "displayOrder": 1
}
```

3. Remove Image
- Method: `DELETE`
- URL: `{{baseUrl}}/api/products/images/{{imageId}}`
- Auth: Bearer `{{adminAccessToken}}`
- Body: none

4. Restore Image
- Method: `POST`
- URL: `{{baseUrl}}/api/products/images/{{imageId}}/restore`
- Auth: Bearer `{{adminAccessToken}}`
- Body: none

## 5. Flow Email Verification (End-to-End)

Flow ini mengikuti implementasi terbaru di endpoint profile user.

1. Register user baru lewat `POST /api/auth/register`.
2. Login user lewat `POST /api/auth/login` untuk mendapatkan `userAccessToken`.
3. Kirim OTP verifikasi lewat `POST /api/users/me/email-verification/send` dengan Bearer token user.
4. Lihat terminal API yang sedang jalan, cari log kode verifikasi dari `ConsoleEmailSender`.
5. Simpan kode ke Postman variable `verificationCode`.
6. Verifikasi email lewat `POST /api/users/me/email-verification/verify` dengan body `{"code":"{{verificationCode}}"}`.
7. Cek status email lewat `GET /api/users/me`, pastikan `isEmailVerified` sudah `true`.

Contoh log OTP di terminal:

```text
Email verification code for user@mail.com (Demo User): 123456. Expires at 2026-02-27 12:34:56Z
```

Catatan:
- Endpoint verify email saat ini **tidak memblok login** jika belum verifikasi (sesuai requirement kamu).
- Jika salah kode, `Attempts` akan bertambah.
- Jika expired atau attempts habis, lakukan `send` ulang untuk generate OTP baru.

## 6. Contoh Payload JSON Cepat

### Register

```json
{
  "fullName": "John Doe",
  "email": "john@mail.com",
  "password": "Password123!"
}
```

### Login

```json
{
  "email": "john@mail.com",
  "password": "Password123!",
  "deviceName": "Web Chrome"
}
```

### Verify Email

```json
{
  "code": "123456"
}
```

### Create Category

```json
{
  "name": "Electronics",
  "slug": "electronics",
  "parentId": null,
  "imageUrl": "https://example.com/cat.jpg",
  "description": "Kategori elektronik"
}
```

### Create Product

```json
{
  "categoryId": "00000000-0000-0000-0000-000000000000",
  "name": "Sample Product",
  "slug": "sample-product",
  "basePrice": 1500000,
  "description": "Sample description"
}
```

### Create Address

```json
{
  "label": "Rumah",
  "recipientName": "John Doe",
  "recipientPhone": "081234567890",
  "fullAddress": "Jl. Merdeka No. 10",
  "city": "Bandung",
  "province": "Jawa Barat",
  "postalCode": "40123",
  "isDefault": true
}
```

## 7. Catatan Penting

1. Endpoint yang memakai `HasPermission(...)` umumnya lebih aman dites pakai akun admin.
2. Untuk demo `remove variant` dan `remove image`, buat minimal 2 data aktif dulu.
3. Jika dapat `401`, cek header `Authorization: Bearer <token>`.
4. Jika dapat `403`, berarti token valid tapi permission role tidak cukup.
5. Uji manual via Swagger tersedia di: `http://localhost:5006/swagger` atau `https://localhost:7091/swagger`.
6. Rate limiting aktif dan akan return HTTP `429` jika limit terlampaui.
7. Limit `POST /api/auth/register`: 5 request / 10 menit per IP.
8. Limit `POST /api/auth/login`: 10 request / 1 menit per IP.
9. Limit `POST /api/users/me/email-verification/send`: 3 request / 10 menit per user/IP.
10. Limit `POST /api/users/me/email-verification/verify`: 15 request / 10 menit per user/IP.
