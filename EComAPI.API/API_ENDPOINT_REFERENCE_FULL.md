# API Endpoint Reference (Full)

Dokumen ini mencatat semua endpoint yang ada di controller API saat ini, beserta contoh body request dan contoh response hasil.

## Format Response Umum

Semua endpoint memakai wrapper berikut:

```json
{
  "success": true,
  "message": "Success",
  "data": {}
}
```

## Module: addresses

### GET /api/addresses

- Auth: `Authorize`

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "id":  "00000000-0000-0000-0000-000000000000",
                 "label":  "string",
                 "recipientName":  "string",
                 "recipientPhone":  "string",
                 "fullAddress":  "string",
                 "city":  "string",
                 "province":  "string",
                 "postalCode":  "string",
                 "isDefault":  true
             }
}
```

### POST /api/addresses

- Auth: `Authorize`

#### Request Body (`CreateAddressRequest`)

| Field | Type | Required |
|---|---|---|
| `label` | `string` | Yes |
| `recipientName` | `string` | Yes |
| `recipientPhone` | `string` | Yes |
| `fullAddress` | `string` | Yes |
| `city` | `string` | Yes |
| `province` | `string` | Yes |
| `postalCode` | `string` | Yes |
| `isDefault` | `bool` | Yes |

```json
{
    "label":  "string",
    "recipientName":  "string",
    "recipientPhone":  "string",
    "fullAddress":  "string",
    "city":  "string",
    "province":  "string",
    "postalCode":  "string",
    "isDefault":  true
}
```

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "id":  "00000000-0000-0000-0000-000000000000",
                 "label":  "string",
                 "recipientName":  "string",
                 "recipientPhone":  "string",
                 "fullAddress":  "string",
                 "city":  "string",
                 "province":  "string",
                 "postalCode":  "string",
                 "isDefault":  true
             }
}
```

### DELETE /api/addresses/{id}

- Auth: `Authorize`
- Params:
  - `id` (`Guid`, route)

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "addressId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

### PATCH /api/addresses/{id}

- Auth: `Authorize`
- Params:
  - `id` (`Guid`, route)

#### Request Body (`UpdateAddressRequest`)

| Field | Type | Required |
|---|---|---|
| `label` | `string` | Yes |
| `recipientName` | `string` | Yes |
| `recipientPhone` | `string` | Yes |
| `fullAddress` | `string` | Yes |
| `city` | `string` | Yes |
| `province` | `string` | Yes |
| `postalCode` | `string` | Yes |

```json
{
    "label":  "string",
    "recipientName":  "string",
    "recipientPhone":  "string",
    "fullAddress":  "string",
    "city":  "string",
    "province":  "string",
    "postalCode":  "string"
}
```

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "id":  "00000000-0000-0000-0000-000000000000",
                 "label":  "string",
                 "recipientName":  "string",
                 "recipientPhone":  "string",
                 "fullAddress":  "string",
                 "city":  "string",
                 "province":  "string",
                 "postalCode":  "string",
                 "isDefault":  true
             }
}
```

### POST /api/addresses/{id}/set-default

- Auth: `Authorize`
- Params:
  - `id` (`Guid`, route)

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "id":  "00000000-0000-0000-0000-000000000000",
                 "label":  "string",
                 "recipientName":  "string",
                 "recipientPhone":  "string",
                 "fullAddress":  "string",
                 "city":  "string",
                 "province":  "string",
                 "postalCode":  "string",
                 "isDefault":  true
             }
}
```

### GET /api/addresses/default

- Auth: `Authorize`

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "id":  "00000000-0000-0000-0000-000000000000",
                 "label":  "string",
                 "recipientName":  "string",
                 "recipientPhone":  "string",
                 "fullAddress":  "string",
                 "city":  "string",
                 "province":  "string",
                 "postalCode":  "string",
                 "isDefault":  true
             }
}
```

## Module: auth

### POST /api/auth/email-verification/send

- Auth: `AllowAnonymous`

#### Request Body (`SendEmailVerificationByEmailRequest`)

| Field | Type | Required |
|---|---|---|
| `email` | `string` | Yes |

```json
{
    "email":  "string"
}
```

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "expiresAt":  "2026-04-06T00:00:00Z"
             }
}
```

### POST /api/auth/email-verification/verify

- Auth: `AllowAnonymous`

#### Request Body (`VerifyEmailByEmailRequest`)

| Field | Type | Required |
|---|---|---|
| `email` | `string` | Yes |
| `code` | `string` | Yes |

```json
{
    "email":  "string",
    "code":  "string"
}
```

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "isEmailVerified":  true
             }
}
```

### POST /api/auth/forgot-password

- Auth: `AllowAnonymous`

#### Request Body (`ForgotPasswordRequest`)

| Field | Type | Required |
|---|---|---|
| `email` | `string` | Yes |

```json
{
    "email":  "string"
}
```

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "expiresAt":  "2026-04-06T00:00:00Z"
             }
}
```

### POST /api/auth/login

- Auth: `AllowAnonymous`

#### Request Body (`LoginRequest`)

| Field | Type | Required |
|---|---|---|
| `email` | `string` | Yes |
| `password` | `string` | Yes |
| `deviceName` | `string?` | No |

```json
{
    "email":  "string",
    "password":  "string",
    "deviceName":  "string"
}
```

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "accessToken":  "string",
                 "refreshToken":  "string",
                 "accessTokenExpiresAt":  "2026-04-06T00:00:00Z",
                 "refreshTokenExpiresAt":  "2026-04-06T00:00:00Z"
             }
}
```

### POST /api/auth/logout

- Auth: `Authorize`

#### Request Body (`LogoutRequest`)

| Field | Type | Required |
|---|---|---|
| `refreshToken` | `string?` | No |

```json
{
    "refreshToken":  "string"
}
```

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "message":  "string"
             }
}
```

### POST /api/auth/refresh-token

- Auth: `AllowAnonymous`

#### Request Body (`RefreshTokenRequest`)

| Field | Type | Required |
|---|---|---|
| `refreshToken` | `string` | Yes |

```json
{
    "refreshToken":  "string"
}
```

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "accessToken":  "string",
                 "refreshToken":  "string",
                 "accessTokenExpiresAt":  "2026-04-06T00:00:00Z",
                 "refreshTokenExpiresAt":  "2026-04-06T00:00:00Z"
             }
}
```

### POST /api/auth/register

- Auth: `AllowAnonymous`

#### Request Body (`RegisterRequest`)

| Field | Type | Required |
|---|---|---|
| `fullName` | `string` | Yes |
| `email` | `string` | Yes |
| `password` | `string` | Yes |

```json
{
    "fullName":  "string",
    "email":  "string",
    "password":  "string"
}
```

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "userId":  "00000000-0000-0000-0000-000000000000",
                 "fullName":  "string",
                 "email":  "string",
                 "isEmailVerified":  true
             }
}
```

### POST /api/auth/reset-password

- Auth: `AllowAnonymous`

#### Request Body (`ResetPasswordRequest`)

| Field | Type | Required |
|---|---|---|
| `email` | `string` | Yes |
| `code` | `string` | Yes |
| `newPassword` | `string` | Yes |

```json
{
    "email":  "string",
    "code":  "string",
    "newPassword":  "string"
}
```

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "isPasswordReset":  true
             }
}
```

## Module: cart

### DELETE /api/cart

- Auth: `Authorize`

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "cartId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

### GET /api/cart

- Auth: `Authorize`

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "id":  "00000000-0000-0000-0000-000000000000",
                 "userId":  "00000000-0000-0000-0000-000000000000",
                 "items":  {
                               "id":  "00000000-0000-0000-0000-000000000000",
                               "productVariantId":  "00000000-0000-0000-0000-000000000000",
                               "productName":  "string",
                               "variantSku":  "string",
                               "variantSize":  "string",
                               "variantColor":  "string",
                               "unitPrice":  0,
                               "quantity":  0,
                               "subtotal":  0
                           },
                 "totalPrice":  0,
                 "totalItems":  0
             }
}
```

### POST /api/cart/items

- Auth: `Authorize`

#### Request Body (`AddToCartRequest`)

| Field | Type | Required |
|---|---|---|
| `productVariantId` | `Guid` | Yes |
| `quantity` | `int` | No |

```json
{
    "productVariantId":  "00000000-0000-0000-0000-000000000000",
    "quantity":  0
}
```

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "cartItemId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

### DELETE /api/cart/items/{id}

- Auth: `Authorize`
- Params:
  - `id` (`Guid`, route)

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "cartItemId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

### PATCH /api/cart/items/{id}

- Auth: `Authorize`
- Params:
  - `id` (`Guid`, route)

#### Request Body (`UpdateCartItemRequest`)

| Field | Type | Required |
|---|---|---|
| `quantity` | `int` | Yes |

```json
{
    "quantity":  0
}
```

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "cartItemId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

## Module: categories

### GET /api/categories

- Auth: `AllowAnonymous`

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "product_categories":  "string"
             }
}
```

### POST /api/categories

- Auth: `HasPermission(Permissions.Categories.Create)`
- Permission: `Permissions.Categories.Create`

#### Request Body (`CreateCategoryRequest`)

| Field | Type | Required |
|---|---|---|
| `name` | `string` | Yes |
| `slug` | `string` | Yes |
| `parentId` | `Guid?` | No |
| `imageUrl` | `string?` | No |
| `description` | `string?` | No |

```json
{
    "name":  "string",
    "slug":  "string",
    "parentId":  "00000000-0000-0000-0000-000000000000",
    "imageUrl":  "string",
    "description":  "string"
}
```

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "categoryId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

### DELETE /api/categories/{id}

- Auth: `HasPermission(Permissions.Categories.Delete)`
- Permission: `Permissions.Categories.Delete`
- Params:
  - `id` (`Guid`, route)

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  "string"
}
```

### PATCH /api/categories/{id}

- Auth: `HasPermission(Permissions.Categories.Update)`
- Permission: `Permissions.Categories.Update`
- Params:
  - `id` (`Guid`, route)

#### Request Body (`UpdateCategoryRequest`)

| Field | Type | Required |
|---|---|---|
| `name` | `string?` | No |
| `slug` | `string?` | No |
| `parentId` | `Guid?` | No |
| `imageUrl` | `string?` | No |
| `description` | `string?` | No |

```json
{
    "name":  "string",
    "slug":  "string",
    "parentId":  "00000000-0000-0000-0000-000000000000",
    "imageUrl":  "string",
    "description":  "string"
}
```

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "categoryId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

### POST /api/categories/{id}/restore

- Auth: `HasPermission(Permissions.Categories.Restore)`
- Permission: `Permissions.Categories.Restore`
- Params:
  - `id` (`Guid`, route)

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  "string"
}
```

### GET /api/categories/{slug}

- Auth: `AllowAnonymous`
- Params:
  - `slug` (`string`, route)

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "id":  "00000000-0000-0000-0000-000000000000",
                 "name":  "string",
                 "slug":  "string",
                 "parentId":  "00000000-0000-0000-0000-000000000000",
                 "imageUrl":  "string",
                 "description":  "string",
                 "productCount":  0,
                 "createdAt":  "2026-04-06T00:00:00Z",
                 "updatedAt":  "2026-04-06T00:00:00Z"
             }
}
```

## Module: coupons

### GET /api/coupons

- Auth: `Authorize`

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "id":  "00000000-0000-0000-0000-000000000000",
                 "code":  "string",
                 "discountAmount":  0,
                 "discountType":  "string",
                 "minimumPurchase":  0,
                 "maxDiscount":  0,
                 "maxUsage":  0,
                 "usedCount":  0,
                 "validFrom":  "2026-04-06T00:00:00Z",
                 "validUntil":  "2026-04-06T00:00:00Z",
                 "isActive":  true
             }
}
```

### POST /api/coupons

- Auth: `HasPermission(Permissions.Orders.UpdateAny)`
- Permission: `Permissions.Orders.UpdateAny`

#### Request Body (`CreateCouponRequest`)

| Field | Type | Required |
|---|---|---|
| `code` | `string` | Yes |
| `discountAmount` | `decimal` | Yes |
| `discountType` | `string` | Yes |
| `maxUsage` | `int` | Yes |
| `validFrom` | `DateTime` | Yes |
| `validUntil` | `DateTime` | Yes |
| `minimumPurchase` | `decimal` | No |
| `maxDiscount` | `decimal?` | No |

```json
{
    "code":  "string",
    "discountAmount":  0,
    "discountType":  "string",
    "maxUsage":  0,
    "validFrom":  "2026-04-06T00:00:00Z",
    "validUntil":  "2026-04-06T00:00:00Z",
    "minimumPurchase":  0,
    "maxDiscount":  0
}
```

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "couponId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

## Module: orders

### GET /api/orders

- Auth: `Authorize`
- Params:
  - `page` (`int`, query)
  - `pageSize` (`int`, query)

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "id":  "00000000-0000-0000-0000-000000000000",
                 "orderNumber":  "string",
                 "status":  "string",
                 "shippingRecipientName":  "string",
                 "shippingPhone":  "string",
                 "shippingFullAddress":  "string",
                 "shippingCity":  "string",
                 "shippingPostalCode":  "string",
                 "totalAmount":  0,
                 "shippingCost":  0,
                 "discountAmount":  0,
                 "finalAmount":  0,
                 "courier":  "string",
                 "trackingNumber":  "string",
                 "customerNote":  "string",
                 "adminNote":  "string",
                 "createdAt":  "2026-04-06T00:00:00Z",
                 "items":  {
                               "id":  "00000000-0000-0000-0000-000000000000",
                               "productId":  "00000000-0000-0000-0000-000000000000",
                               "productVariantId":  "00000000-0000-0000-0000-000000000000",
                               "snapshotProductName":  "string",
                               "snapshotVariantName":  "string",
                               "snapshotPrice":  0,
                               "quantity":  0,
                               "subtotal":  0
                           },
                 "payment":  {
                                 "id":  "00000000-0000-0000-0000-000000000000",
                                 "paymentMethod":  "string",
                                 "amount":  0,
                                 "status":  "string",
                                 "proofImageUrl":  "string",
                                 "adminNote":  "string",
                                 "confirmedAt":  "2026-04-06T00:00:00Z"
                             },
                 "reviews":  {
                                 "id":  "00000000-0000-0000-0000-000000000000",
                                 "userId":  "00000000-0000-0000-0000-000000000000",
                                 "orderId":  "00000000-0000-0000-0000-000000000000",
                                 "productId":  "00000000-0000-0000-0000-000000000000",
                                 "rating":  0,
                                 "comment":  "string",
                                 "createdAt":  "2026-04-06T00:00:00Z",
                                 "images":  {
                                                "id":  "...",
                                                "imageUrl":  "...",
                                                "displayOrder":  "..."
                                            }
                             },
                 "returns":  {
                                 "id":  "00000000-0000-0000-0000-000000000000",
                                 "orderId":  "00000000-0000-0000-0000-000000000000",
                                 "userId":  "00000000-0000-0000-0000-000000000000",
                                 "returnNumber":  "string",
                                 "reason":  "string",
                                 "status":  "string",
                                 "requestedAt":  "2026-04-06T00:00:00Z",
                                 "approvedAt":  "2026-04-06T00:00:00Z",
                                 "approvedBy":  "00000000-0000-0000-0000-000000000000",
                                 "refundAmount":  0,
                                 "bankName":  "string",
                                 "bankAccountNumber":  "string",
                                 "accountHolderName":  "string",
                                 "refundDate":  "2026-04-06T00:00:00Z",
                                 "items":  {
                                               "id":  "...",
                                               "orderItemId":  "...",
                                               "quantity":  "...",
                                               "condition":  "...",
                                               "adminNote":  "..."
                                           },
                                 "images":  {
                                                "id":  "...",
                                                "imageUrl":  "...",
                                                "description":  "...",
                                                "createdAt":  "..."
                                            }
                             }
             }
}
```

### POST /api/orders

- Auth: `Authorize`

#### Request Body (`CreateOrderRequest`)

| Field | Type | Required |
|---|---|---|
| `addressId` | `Guid` | Yes |
| `shippingType` | `ShippingType` | Yes |
| `items` | `List<CreateOrderItemRequest>` | Yes |
| `couponCode` | `string?` | No |
| `customerNote` | `string?` | No |

```json
{
    "addressId":  "00000000-0000-0000-0000-000000000000",
    "shippingType":  "string_enum_value",
    "items":  {
                  "productVariantId":  "00000000-0000-0000-0000-000000000000",
                  "quantity":  0
              },
    "couponCode":  "string",
    "customerNote":  "string"
}
```

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "orderNumber":  "string"
             }
}
```

### GET /api/orders/{id:guid}

- Auth: `Authorize`
- Params:
  - `id` (`Guid`, route)

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "id":  "00000000-0000-0000-0000-000000000000",
                 "orderNumber":  "string",
                 "status":  "string",
                 "shippingRecipientName":  "string",
                 "shippingPhone":  "string",
                 "shippingFullAddress":  "string",
                 "shippingCity":  "string",
                 "shippingPostalCode":  "string",
                 "totalAmount":  0,
                 "shippingCost":  0,
                 "discountAmount":  0,
                 "finalAmount":  0,
                 "courier":  "string",
                 "trackingNumber":  "string",
                 "customerNote":  "string",
                 "adminNote":  "string",
                 "createdAt":  "2026-04-06T00:00:00Z",
                 "items":  {
                               "id":  "00000000-0000-0000-0000-000000000000",
                               "productId":  "00000000-0000-0000-0000-000000000000",
                               "productVariantId":  "00000000-0000-0000-0000-000000000000",
                               "snapshotProductName":  "string",
                               "snapshotVariantName":  "string",
                               "snapshotPrice":  0,
                               "quantity":  0,
                               "subtotal":  0
                           },
                 "payment":  {
                                 "id":  "00000000-0000-0000-0000-000000000000",
                                 "paymentMethod":  "string",
                                 "amount":  0,
                                 "status":  "string",
                                 "proofImageUrl":  "string",
                                 "adminNote":  "string",
                                 "confirmedAt":  "2026-04-06T00:00:00Z"
                             },
                 "reviews":  {
                                 "id":  "00000000-0000-0000-0000-000000000000",
                                 "userId":  "00000000-0000-0000-0000-000000000000",
                                 "orderId":  "00000000-0000-0000-0000-000000000000",
                                 "productId":  "00000000-0000-0000-0000-000000000000",
                                 "rating":  0,
                                 "comment":  "string",
                                 "createdAt":  "2026-04-06T00:00:00Z",
                                 "images":  {
                                                "id":  "00000000-0000-0000-0000-000000000000",
                                                "imageUrl":  "string",
                                                "displayOrder":  0
                                            }
                             },
                 "returns":  {
                                 "id":  "00000000-0000-0000-0000-000000000000",
                                 "orderId":  "00000000-0000-0000-0000-000000000000",
                                 "userId":  "00000000-0000-0000-0000-000000000000",
                                 "returnNumber":  "string",
                                 "reason":  "string",
                                 "status":  "string",
                                 "requestedAt":  "2026-04-06T00:00:00Z",
                                 "approvedAt":  "2026-04-06T00:00:00Z",
                                 "approvedBy":  "00000000-0000-0000-0000-000000000000",
                                 "refundAmount":  0,
                                 "bankName":  "string",
                                 "bankAccountNumber":  "string",
                                 "accountHolderName":  "string",
                                 "refundDate":  "2026-04-06T00:00:00Z",
                                 "items":  {
                                               "id":  "00000000-0000-0000-0000-000000000000",
                                               "orderItemId":  "00000000-0000-0000-0000-000000000000",
                                               "quantity":  0,
                                               "condition":  "string",
                                               "adminNote":  "string"
                                           },
                                 "images":  {
                                                "id":  "00000000-0000-0000-0000-000000000000",
                                                "imageUrl":  "string",
                                                "description":  "string",
                                                "createdAt":  "2026-04-06T00:00:00Z"
                                            }
                             }
             }
}
```

### POST /api/orders/{id:guid}/payment/confirm

- Auth: `HasPermission(Permissions.Orders.UpdateAny)`
- Permission: `Permissions.Orders.UpdateAny`
- Params:
  - `id` (`Guid`, route)

#### Request Body (`ConfirmPaymentRequest`)

| Field | Type | Required |
|---|---|---|
| `adminNote` | `string?` | No |

```json
{
    "adminNote":  "string"
}
```

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "paymentId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

### POST /api/orders/{id:guid}/payment/proof

- Auth: `Authorize`
- Params:
  - `id` (`Guid`, route)

#### Request Body (`SubmitPaymentProofRequest`)

| Field | Type | Required |
|---|---|---|
| `proofImageUrl` | `string` | Yes |
| `paymentMethod` | `string?` | No |

```json
{
    "proofImageUrl":  "string",
    "paymentMethod":  "string"
}
```

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "paymentId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

### PATCH /api/orders/{id:guid}/status

- Auth: `HasPermission(Permissions.Orders.UpdateAny)`
- Permission: `Permissions.Orders.UpdateAny`
- Params:
  - `id` (`Guid`, route)

#### Request Body (`UpdateOrderStatusRequest`)

| Field | Type | Required |
|---|---|---|
| `status` | `string` | Yes |
| `courier` | `string?` | No |
| `trackingNumber` | `string?` | No |
| `adminNote` | `string?` | No |

```json
{
    "status":  "string",
    "courier":  "string",
    "trackingNumber":  "string",
    "adminNote":  "string"
}
```

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "orderId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

### GET /api/orders/{orderId:guid}/status-logs

- Auth: `Authorize`
- Params:
  - `orderId` (`Guid`, route)

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "id":  "00000000-0000-0000-0000-000000000000",
                 "orderId":  "00000000-0000-0000-0000-000000000000",
                 "status":  "string",
                 "note":  "string",
                 "changedAt":  "2026-04-06T00:00:00Z",
                 "createdBy":  "00000000-0000-0000-0000-000000000000"
             }
}
```

## Module: products

### GET /api/products

- Auth: `AllowAnonymous`
- Params:
  - `page` (`int`, query)
  - `pageSize` (`int`, query)
  - `categorySlug` (`string?`, query)
  - `includeSubcategories` (`bool`, query)
  - `includeInactive` (`bool`, query)
  - `minPrice` (`decimal?`, query)
  - `maxPrice` (`decimal?`, query)
  - `inStock` (`bool?`, query)
  - `searchTerm` (`string?`, query)
  - `sortBy` (`string`, query)
  - `sortOrder` (`string`, query)

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  "string"
}
```

### POST /api/products

- Auth: `HasPermission(Permissions.Products.Create)`
- Permission: `Permissions.Products.Create`

#### Request Body (`CreateProductRequest`)

| Field | Type | Required |
|---|---|---|
| `categoryId` | `Guid` | Yes |
| `name` | `string` | Yes |
| `slug` | `string` | Yes |
| `basePrice` | `decimal` | Yes |
| `description` | `string?` | No |

```json
{
    "categoryId":  "00000000-0000-0000-0000-000000000000",
    "name":  "string",
    "slug":  "string",
    "basePrice":  0,
    "description":  "string"
}
```

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "productId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

### DELETE /api/products/{id}

- Auth: `HasPermission(Permissions.Products.Delete)`
- Permission: `Permissions.Products.Delete`
- Params:
  - `id` (`Guid`, route)

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "productId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

### PATCH /api/products/{id}

- Auth: `HasPermission(Permissions.Products.Update)`
- Permission: `Permissions.Products.Update`
- Params:
  - `id` (`Guid`, route)

#### Request Body (`UpdateProductRequest`)

| Field | Type | Required |
|---|---|---|
| `name` | `string?` | No |
| `slug` | `string?` | No |
| `basePrice` | `decimal?` | No |
| `categoryId` | `Guid?` | No |
| `description` | `string?` | No |

```json
{
    "name":  "string",
    "slug":  "string",
    "basePrice":  0,
    "categoryId":  "00000000-0000-0000-0000-000000000000",
    "description":  "string"
}
```

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "productId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

### PATCH /api/products/{id}/activate

- Auth: `HasPermission(Permissions.Products.Update)`
- Permission: `Permissions.Products.Update`
- Params:
  - `id` (`Guid`, route)

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "productId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

### PATCH /api/products/{id}/deactivate

- Auth: `HasPermission(Permissions.Products.Update)`
- Permission: `Permissions.Products.Update`
- Params:
  - `id` (`Guid`, route)

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "productId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

### POST /api/products/{id}/restore

- Auth: `HasPermission(Permissions.Products.Restore)`
- Permission: `Permissions.Products.Restore`
- Params:
  - `id` (`Guid`, route)

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "productId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

### POST /api/products/{productId}/images

- Auth: `HasPermission(Permissions.Products.Create)`
- Permission: `Permissions.Products.Create`
- Params:
  - `productId` (`Guid`, route)

#### Request Body (`AddProductImageRequest`)

| Field | Type | Required |
|---|---|---|
| `imageUrl` | `string` | Yes |
| `isPrimary` | `bool` | No |
| `displayOrder` | `int` | No |

```json
{
    "imageUrl":  "string",
    "isPrimary":  true,
    "displayOrder":  0
}
```

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "imageId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

### POST /api/products/{productId}/variants

- Auth: `HasPermission(Permissions.Products.Create)`
- Permission: `Permissions.Products.Create`
- Params:
  - `productId` (`Guid`, route)

#### Request Body (`AddProductVariantRequest`)

| Field | Type | Required |
|---|---|---|
| `sku` | `string` | Yes |
| `stock` | `int` | Yes |
| `priceAdjustment` | `decimal` | No |
| `size` | `string?` | No |
| `color` | `string?` | No |

```json
{
    "sku":  "string",
    "stock":  0,
    "priceAdjustment":  0,
    "size":  "string",
    "color":  "string"
}
```

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "variantId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

### GET /api/products/{slug}

- Auth: `AllowAnonymous`
- Params:
  - `slug` (`string`, route)

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "id":  "00000000-0000-0000-0000-000000000000",
                 "name":  "string",
                 "slug":  "string",
                 "basePrice":  0,
                 "description":  "string",
                 "viewCount":  0,
                 "totalStock":  0,
                 "isActive":  true,
                 "category":  {
                                  "id":  "00000000-0000-0000-0000-000000000000",
                                  "name":  "string",
                                  "slug":  "string",
                                  "parent":  {
                                                 "id":  "00000000-0000-0000-0000-000000000000",
                                                 "name":  "string",
                                                 "slug":  "string",
                                                 "parent":  {
                                                                "id":  "00000000-0000-0000-0000-000000000000",
                                                                "name":  "string",
                                                                "slug":  "string",
                                                                "parent":  {
                                                                               "id":  "00000000-0000-0000-0000-000000000000",
                                                                               "name":  "string",
                                                                               "slug":  "string",
                                                                               "parent":  {
                                                                                              "id":  "...",
                                                                                              "name":  "...",
                                                                                              "slug":  "...",
                                                                                              "parent":  "..."
                                                                                          }
                                                                           }
                                                            }
                                             }
                              },
                 "productVariantResponses":  {
                                                 "id":  "00000000-0000-0000-0000-000000000000",
                                                 "sku":  "string",
                                                 "stock":  0,
                                                 "priceAdjustment":  0,
                                                 "finalPrice":  0,
                                                 "size":  "string",
                                                 "color":  "string",
                                                 "isActive":  true
                                             },
                 "productImageResponses":  {
                                               "id":  "00000000-0000-0000-0000-000000000000",
                                               "imageUrl":  "string",
                                               "isPrimary":  true,
                                               "displayOrder":  0
                                           }
             }
}
```

### DELETE /api/products/images/{id}

- Auth: `HasPermission(Permissions.Products.Delete)`
- Permission: `Permissions.Products.Delete`
- Params:
  - `id` (`Guid`, route)

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "imageId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

### PATCH /api/products/images/{id}

- Auth: `HasPermission(Permissions.Products.Update)`
- Permission: `Permissions.Products.Update`
- Params:
  - `id` (`Guid`, route)

#### Request Body (`UpdateProductImageRequest`)

| Field | Type | Required |
|---|---|---|
| `imageUrl` | `string?` | No |
| `isPrimary` | `bool?` | No |
| `displayOrder` | `int?` | No |

```json
{
    "imageUrl":  "string",
    "isPrimary":  true,
    "displayOrder":  0
}
```

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "imageId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

### POST /api/products/images/{id}/restore

- Auth: `HasPermission(Permissions.Products.Update)`
- Permission: `Permissions.Products.Update`
- Params:
  - `id` (`Guid`, route)

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "imageId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

### DELETE /api/products/variants/{id}

- Auth: `HasPermission(Permissions.Products.Delete)`
- Permission: `Permissions.Products.Delete`
- Params:
  - `id` (`Guid`, route)

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "variantId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

### PATCH /api/products/variants/{id}

- Auth: `HasPermission(Permissions.Products.Update)`
- Permission: `Permissions.Products.Update`
- Params:
  - `id` (`Guid`, route)

#### Request Body (`UpdateProductVariantRequest`)

| Field | Type | Required |
|---|---|---|
| `sku` | `string?` | No |
| `stock` | `int?` | No |
| `priceAdjustment` | `decimal?` | No |
| `size` | `string?` | No |
| `color` | `string?` | No |

```json
{
    "sku":  "string",
    "stock":  0,
    "priceAdjustment":  0,
    "size":  "string",
    "color":  "string"
}
```

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "variantId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

### POST /api/products/variants/{id}/restore

- Auth: `HasPermission(Permissions.Products.Update)`
- Permission: `Permissions.Products.Update`
- Params:
  - `id` (`Guid`, route)

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "variantId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

## Module: returns

### GET /api/returns

- Auth: `Authorize`
- Params:
  - `page` (`int`, query)
  - `pageSize` (`int`, query)

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  "string"
}
```

### POST /api/returns

- Auth: `Authorize`

#### Request Body (`CreateReturnRequest`)

| Field | Type | Required |
|---|---|---|
| `orderId` | `Guid` | Yes |
| `reason` | `string` | Yes |
| `items` | `List<CreateReturnItemRequest>` | Yes |
| `images` | `List<CreateReturnImageRequest>?` | No |
| `bankName` | `string?` | No |
| `bankAccountNumber` | `string?` | No |
| `accountHolderName` | `string?` | No |

```json
{
    "orderId":  "00000000-0000-0000-0000-000000000000",
    "reason":  "string",
    "items":  {
                  "orderItemId":  "00000000-0000-0000-0000-000000000000",
                  "quantity":  0
              },
    "images":  {
                   "imageUrl":  "string",
                   "description":  "string"
               },
    "bankName":  "string",
    "bankAccountNumber":  "string",
    "accountHolderName":  "string"
}
```

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "returnId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

### GET /api/returns/{id:guid}

- Auth: `Authorize`
- Params:
  - `id` (`Guid`, route)

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "id":  "00000000-0000-0000-0000-000000000000",
                 "orderId":  "00000000-0000-0000-0000-000000000000",
                 "userId":  "00000000-0000-0000-0000-000000000000",
                 "returnNumber":  "string",
                 "reason":  "string",
                 "status":  "string",
                 "requestedAt":  "2026-04-06T00:00:00Z",
                 "approvedAt":  "2026-04-06T00:00:00Z",
                 "approvedBy":  "00000000-0000-0000-0000-000000000000",
                 "refundAmount":  0,
                 "bankName":  "string",
                 "bankAccountNumber":  "string",
                 "accountHolderName":  "string",
                 "refundDate":  "2026-04-06T00:00:00Z",
                 "items":  {
                               "id":  "00000000-0000-0000-0000-000000000000",
                               "orderItemId":  "00000000-0000-0000-0000-000000000000",
                               "quantity":  0,
                               "condition":  "string",
                               "adminNote":  "string"
                           },
                 "images":  {
                                "id":  "00000000-0000-0000-0000-000000000000",
                                "imageUrl":  "string",
                                "description":  "string",
                                "createdAt":  "2026-04-06T00:00:00Z"
                            }
             }
}
```

### POST /api/returns/{id:guid}/approve

- Auth: `HasPermission(Permissions.Orders.UpdateAny)`
- Permission: `Permissions.Orders.UpdateAny`
- Params:
  - `id` (`Guid`, route)

#### Request Body (`ApproveReturnRequest`)

| Field | Type | Required |
|---|---|---|
| `refundAmount` | `decimal` | Yes |
| `itemConditions` | `List<ApproveReturnItemRequest>?` | No |

```json
{
    "refundAmount":  0,
    "itemConditions":  {
                           "returnItemId":  "00000000-0000-0000-0000-000000000000",
                           "condition":  "string",
                           "adminNote":  "string"
                       }
}
```

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "returnId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

### POST /api/returns/{id:guid}/refund

- Auth: `HasPermission(Permissions.Orders.UpdateAny)`
- Permission: `Permissions.Orders.UpdateAny`
- Params:
  - `id` (`Guid`, route)

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "returnId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

### POST /api/returns/{id:guid}/reject

- Auth: `HasPermission(Permissions.Orders.UpdateAny)`
- Permission: `Permissions.Orders.UpdateAny`
- Params:
  - `id` (`Guid`, route)

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "returnId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

## Module: reviews

### POST /api/reviews

- Auth: `Authorize`

#### Request Body (`CreateReviewRequest`)

| Field | Type | Required |
|---|---|---|
| `orderId` | `Guid` | Yes |
| `productId` | `Guid` | Yes |
| `rating` | `int` | Yes |
| `comment` | `string?` | No |
| `images` | `List<CreateReviewImageRequest>?` | No |

```json
{
    "orderId":  "00000000-0000-0000-0000-000000000000",
    "productId":  "00000000-0000-0000-0000-000000000000",
    "rating":  0,
    "comment":  "string",
    "images":  {
                   "imageUrl":  "string",
                   "displayOrder":  0
               }
}
```

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "reviewId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

### DELETE /api/reviews/{id:guid}

- Auth: `Authorize`
- Params:
  - `id` (`Guid`, route)

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "reviewId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

### PUT /api/reviews/{id:guid}

- Auth: `Authorize`
- Params:
  - `id` (`Guid`, route)

#### Request Body (`UpdateReviewRequest`)

| Field | Type | Required |
|---|---|---|
| `rating` | `int` | Yes |
| `comment` | `string?` | No |

```json
{
    "rating":  0,
    "comment":  "string"
}
```

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "reviewId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

### GET /api/reviews/product/{productId:guid}

- Auth: `AllowAnonymous`
- Params:
  - `productId` (`Guid`, route)
  - `page` (`int`, query)
  - `pageSize` (`int`, query)

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  "string"
}
```

## Module: stock-logs

### GET /api/stock-logs/variant/{variantId:guid}

- Auth: `HasPermission(Permissions.Orders.UpdateAny)`
- Permission: `Permissions.Orders.UpdateAny`
- Params:
  - `variantId` (`Guid`, route)

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "id":  "00000000-0000-0000-0000-000000000000",
                 "productVariantId":  "00000000-0000-0000-0000-000000000000",
                 "type":  "string",
                 "quantityChange":  0,
                 "stockBefore":  0,
                 "stockAfter":  0,
                 "referenceType":  "string",
                 "referenceId":  "00000000-0000-0000-0000-000000000000",
                 "note":  "string",
                 "createdAt":  "2026-04-06T00:00:00Z",
                 "createdBy":  "00000000-0000-0000-0000-000000000000"
             }
}
```

## Module: uploads

### POST /api/uploads/confirm

- Auth: `Authorize`

#### Request Body (`ConfirmUploadRequest`)

| Field | Type | Required |
|---|---|---|
| `purpose` | `string` | Yes |
| `blobPath` | `string` | Yes |
| `expectedFileSizeBytes` | `long?` | No |
| `expectedContentType` | `string?` | No |

```json
{
    "purpose":  "string",
    "blobPath":  "string",
    "expectedFileSizeBytes":  0,
    "expectedContentType":  "string"
}
```

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "purpose":  "string",
                 "blobPath":  "string",
                 "blobUrl":  "string",
                 "contentLength":  "string",
                 "contentType":  "string",
                 "eTag":  "string"
             }
}
```

### POST /api/uploads/sas

- Auth: `Authorize`

#### Request Body (`GenerateUploadSasRequest`)

| Field | Type | Required |
|---|---|---|
| `purpose` | `string` | Yes |
| `fileName` | `string` | Yes |
| `contentType` | `string` | Yes |
| `fileSizeBytes` | `long` | Yes |

```json
{
    "purpose":  "string",
    "fileName":  "string",
    "contentType":  "string",
    "fileSizeBytes":  0
}
```

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "uploadId":  "00000000-0000-0000-0000-000000000000",
                 "purpose":  "string",
                 "blobPath":  "string",
                 "blobUrl":  "string",
                 "uploadUrl":  "string",
                 "expiresAtUtc":  "2026-04-06T00:00:00Z",
                 "requiredHeaders":  "string"
             }
}
```

## Module: users

### DELETE /api/users/me

- Auth: `HasPermission(Permissions.Users.DeleteOwn)`
- Permission: `Permissions.Users.DeleteOwn`

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "userId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

### GET /api/users/me

- Auth: `HasPermission(Permissions.Users.ReadOwn)`
- Permission: `Permissions.Users.ReadOwn`

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "id":  "00000000-0000-0000-0000-000000000000",
                 "fullName":  "string",
                 "email":  "string",
                 "phone":  "string",
                 "isEmailVerified":  true,
                 "isActive":  true,
                 "roleName":  "string",
                 "avatar":  "string",
                 "dateOfBirth":  "2026-04-06T00:00:00Z",
                 "gender":  "string",
                 "lastLoginAt":  "2026-04-06T00:00:00Z",
                 "defaultAddress":  {
                                        "id":  "00000000-0000-0000-0000-000000000000",
                                        "label":  "string",
                                        "recipientName":  "string",
                                        "recipientPhone":  "string",
                                        "fullAddress":  "string",
                                        "city":  "string",
                                        "province":  "string",
                                        "postalCode":  "string",
                                        "isDefault":  true
                                    },
                 "memberSince":  "2026-04-06T00:00:00Z"
             }
}
```

### PATCH /api/users/me

- Auth: `HasPermission(Permissions.Users.UpdateOwn)`
- Permission: `Permissions.Users.UpdateOwn`

#### Request Body (`UpdateOwnProfileRequest`)

| Field | Type | Required |
|---|---|---|
| `fullName` | `string?` | No |
| `email` | `string?` | No |
| `phone` | `string?` | No |
| `avatar` | `string?` | No |
| `dateOfBirth` | `DateTime?` | No |
| `gender` | `Gender?` | No |

```json
{
    "fullName":  "string",
    "email":  "string",
    "phone":  "string",
    "avatar":  "string",
    "dateOfBirth":  "2026-04-06T00:00:00Z",
    "gender":  "string_enum_value"
}
```

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "userId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

### POST /api/users/me/email-verification/send

- Auth: `HasPermission(Permissions.Users.UpdateOwn)`
- Permission: `Permissions.Users.UpdateOwn`

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "expiresAt":  "2026-04-06T00:00:00Z"
             }
}
```

### POST /api/users/me/email-verification/verify

- Auth: `HasPermission(Permissions.Users.UpdateOwn)`
- Permission: `Permissions.Users.UpdateOwn`

#### Request Body (`VerifyEmailRequest`)

| Field | Type | Required |
|---|---|---|
| `code` | `string` | Yes |

```json
{
    "code":  "string"
}
```

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "isEmailVerified":  true
             }
}
```

## Module: wishlist

### GET /api/wishlist

- Auth: `Authorize`

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "id":  "00000000-0000-0000-0000-000000000000",
                 "productId":  "00000000-0000-0000-0000-000000000000",
                 "productName":  "string",
                 "productSlug":  "string",
                 "basePrice":  0,
                 "primaryImageUrl":  "string",
                 "isActive":  true,
                 "addedAt":  "2026-04-06T00:00:00Z"
             }
}
```

### POST /api/wishlist

- Auth: `Authorize`

#### Request Body (`AddToWishlistRequest`)

| Field | Type | Required |
|---|---|---|
| `productId` | `Guid` | Yes |

```json
{
    "productId":  "00000000-0000-0000-0000-000000000000"
}
```

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "wishlistItemId":  "00000000-0000-0000-0000-000000000000"
             }
}
```

### DELETE /api/wishlist/{wishlistItemId}

- Auth: `Authorize`
- Params:
  - `wishlistItemId` (`Guid`, route)

#### Request Body

Tidak ada body request.

#### Response Success (`200`)

```json
{
    "success":  true,
    "message":  "Success",
    "data":  {
                 "wishlistItemId":  "00000000-0000-0000-0000-000000000000"
             }
}
```


