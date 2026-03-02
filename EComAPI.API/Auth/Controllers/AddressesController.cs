using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using EComAPI.API.Common;
using EComAPI.API.Auth.Dtos.Request;
using EComAPI.API.Auth.Dtos.Response;
using EComAPI.API.Auth.Swagger.Examples;
using EComAPI.API.Auth.Swagger.Examples.CreateAddress.Request;
using EComAPI.API.Auth.Swagger.Examples.CreateAddress.Response;
using EComAPI.API.Auth.Swagger.Examples.UpdateAddress.Request;
using EComAPI.API.Auth.Swagger.Examples.UpdateAddress.Response;
using EComAPI.API.Auth.Swagger.Examples.GetAddresses.Response;
using EComAPI.API.Auth.Swagger.Examples.GetDefaultAddress.Response;
using EComAPI.API.Auth.Swagger.Examples.SetDefaultAddress.Response;
using EComAPI.API.Auth.Swagger.Examples.DeleteAddress.Response;
using EComAPI.Application.Auth.Queries.GetDefaultAddress;
using EComAPI.Application.Auth.Queries.GetUserAddresses;
using EComAPI.Application.Auth.Commands.SetDefaultAddress;
using EComAPI.Application.Auth.Commands.CreateAddress;
using EComAPI.Application.Auth.Commands.DeleteAddress;
using EComAPI.Application.Auth.Commands.UpdateAddress;

namespace EComAPI.API.Auth.Controllers
{
    /// <summary>
    /// Endpoint manajemen alamat milik pengguna yang sedang login.
    /// </summary>
    /// <remarks>
    /// Alur umum (high-level flow):
    /// 1. Semua endpoint membutuhkan autentikasi ([Authorize]).
    /// 2. Controller menerima request lalu memetakan ke command/query.
    /// 3. Handler mengeksekusi business logic terkait alamat.
    /// 4. Hasil dikembalikan sebagai ApiResponse yang konsisten.
    /// </remarks>
    [Route("api/addresses")]
    [ApiController]
    [Authorize]
    public class AddressesController : BaseController
    {
        private readonly CreateAddressHandler _createAddressHandler;
        private readonly UpdateAddressHandler _updateAddressHandler;
        private readonly DeleteAddressHandler _deleteAddressHandler;
        private readonly SetDefaultAddressHandler _setDefaultAddressHandler;
        private readonly GetUserAddressesHandler _getUserAddressesHandler;
        private readonly GetDefaultAddressHandler _getDefaultAddressHandler;

        /// <summary>
        /// Inisialisasi AddressesController dengan seluruh handler untuk fitur alamat.
        /// </summary>
        /// <param name="createAddressHandler">Handler untuk membuat alamat baru.</param>
        /// <param name="updateAddressHandler">Handler untuk memperbarui alamat.</param>
        /// <param name="deleteAddressHandler">Handler untuk menghapus alamat (soft delete).</param>
        /// <param name="setDefaultAddressHandler">Handler untuk mengatur alamat default.</param>
        /// <param name="getUserAddressesHandler">Handler untuk mengambil semua alamat user login.</param>
        /// <param name="getDefaultAddressHandler">Handler untuk mengambil alamat default user login.</param>
        public AddressesController(
            CreateAddressHandler createAddressHandler,
            UpdateAddressHandler updateAddressHandler,
            DeleteAddressHandler deleteAddressHandler,
            SetDefaultAddressHandler setDefaultAddressHandler,
            GetUserAddressesHandler getUserAddressesHandler,
            GetDefaultAddressHandler getDefaultAddressHandler)
        {
            _createAddressHandler = createAddressHandler;
            _updateAddressHandler = updateAddressHandler;
            _deleteAddressHandler = deleteAddressHandler;
            _setDefaultAddressHandler = setDefaultAddressHandler;
            _getUserAddressesHandler = getUserAddressesHandler;
            _getDefaultAddressHandler = getDefaultAddressHandler;
        }

        /// <summary>
        /// Mengambil semua alamat milik pengguna yang sedang login.
        /// </summary>
        /// <remarks>
        /// Endpoint: GET api/addresses
        ///
        /// Flow get all addresses:
        /// 1. Controller memanggil query handler daftar alamat user.
        /// 2. Jika gagal, kembalikan HTTP 400.
        /// 3. Jika berhasil, mapping DTO alamat ke AddressResponse.
        /// 4. Kembalikan daftar alamat dalam response sukses.
        /// </remarks>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi daftar alamat pengguna.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AddressResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(GetAddressesSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(GetAddressesBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            // Step 1: Ambil seluruh alamat user login.
            var getUserAddressesResult = await _getUserAddressesHandler.Handle(cancellationToken);

            // Step 2: Jika gagal, kirim response error.
            if (!getUserAddressesResult.IsSuccess)
                return BadRequestResponse("Failed to get addresses");

            // Step 3: Mapping hasil query ke response DTO API.
            var addressResponses = getUserAddressesResult.Value!
                .Select(addressDto => new AddressResponse(
                    addressDto.Id,
                    addressDto.Label,
                    addressDto.RecipientName,
                    addressDto.RecipientPhone,
                    addressDto.FullAddress,
                    addressDto.City,
                    addressDto.Province,
                    addressDto.PostalCode,
                    addressDto.IsDefault
                ))
                .ToList();

            // Step 4: Kembalikan response sukses.
            return SuccessResponse(addressResponses, "Success get addresses");
        }

        /// <summary>
        /// Mengambil alamat default milik pengguna yang sedang login.
        /// </summary>
        /// <remarks>
        /// Endpoint: GET api/addresses/default
        ///
        /// Flow get default address:
        /// 1. Controller memanggil query handler alamat default.
        /// 2. Jika gagal, kembalikan HTTP 400.
        /// 3. Jika data ada, mapping DTO ke AddressResponse.
        /// 4. Jika belum ada alamat default, data akan bernilai null.
        /// </remarks>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi alamat default atau null.</returns>
        [HttpGet("default")]
        [ProducesResponseType(typeof(ApiResponse<AddressResponse?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(GetDefaultAddressSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(GetDefaultAddressBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        public async Task<IActionResult> GetDefault(CancellationToken cancellationToken)
        {
            // Step 1: Ambil alamat default user login.
            var getDefaultAddressResult = await _getDefaultAddressHandler.Handle(cancellationToken);

            // Step 2: Jika gagal, kirim response error.
            if (!getDefaultAddressResult.IsSuccess)
                return BadRequestResponse("Failed to get default address");

            // Step 3: Mapping DTO ke response (jika data tersedia).
            var getDefaultAddressResponse = getDefaultAddressResult.Value != null
                ? new AddressResponse(
                    getDefaultAddressResult.Value.Id,
                    getDefaultAddressResult.Value.Label,
                    getDefaultAddressResult.Value.RecipientName,
                    getDefaultAddressResult.Value.RecipientPhone,
                    getDefaultAddressResult.Value.FullAddress,
                    getDefaultAddressResult.Value.City,
                    getDefaultAddressResult.Value.Province,
                    getDefaultAddressResult.Value.PostalCode,
                    getDefaultAddressResult.Value.IsDefault
                )
                : null;

            // Step 4: Kembalikan response sukses.
            return SuccessResponse(getDefaultAddressResponse, "Success get default address");
        }

        /// <summary>
        /// Membuat alamat baru untuk pengguna yang sedang login.
        /// </summary>
        /// <remarks>
        /// Endpoint: POST api/addresses
        ///
        /// Flow create address:
        /// 1. Terima payload alamat dari request body.
        /// 2. Mapping payload ke CreateAddressCommand.
        /// 3. Handler memvalidasi lalu menyimpan alamat baru.
        /// 4. Jika gagal, kembalikan HTTP 400.
        /// 5. Jika berhasil, kembalikan data alamat yang dibuat.
        ///
        /// Catatan:
        /// - Jika isDefault=true, handler akan menyesuaikan alamat default user.
        /// </remarks>
        /// <param name="createAddressRequest">Payload data alamat baru.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi alamat yang berhasil dibuat.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<AddressResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerRequestExample(typeof(CreateAddressRequest), typeof(CreateAddressRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(CreateAddressSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(CreateAddressBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        public async Task<IActionResult> Create(
            CreateAddressRequest createAddressRequest,
            CancellationToken cancellationToken)
        {
            // Step 1: Mapping request ke command aplikasi.
            var createAddressCommand = new CreateAddressCommand(
                createAddressRequest.Label,
                createAddressRequest.RecipientName,
                createAddressRequest.RecipientPhone,
                createAddressRequest.FullAddress,
                createAddressRequest.City,
                createAddressRequest.Province,
                createAddressRequest.PostalCode,
                createAddressRequest.IsDefault
            );

            // Step 2: Eksekusi proses pembuatan alamat.
            var createAddressResult = await _createAddressHandler.Handle(createAddressCommand, cancellationToken);

            // Step 3: Jika gagal, kirim response error.
            if (!createAddressResult.IsSuccess)
                return BadRequestResponse(createAddressResult.Error ?? "Create address failed");

            // Step 4: Mapping hasil ke response DTO.
            var addressResponse = new AddressResponse(
                createAddressResult.Value!.Id,
                createAddressResult.Value.Label,
                createAddressResult.Value.RecipientName,
                createAddressResult.Value.RecipientPhone,
                createAddressResult.Value.FullAddress,
                createAddressResult.Value.City,
                createAddressResult.Value.Province,
                createAddressResult.Value.PostalCode,
                createAddressResult.Value.IsDefault
            );

            // Step 5: Kembalikan response sukses.
            return SuccessResponse(addressResponse, "Address created successfully");
        }

        /// <summary>
        /// Memperbarui alamat yang sudah ada.
        /// </summary>
        /// <remarks>
        /// Endpoint: PATCH api/addresses/{id}
        ///
        /// Flow update address:
        /// 1. Terima id alamat dari route dan payload perubahan dari body.
        /// 2. Mapping ke UpdateAddressCommand.
        /// 3. Handler memvalidasi lalu memperbarui data alamat.
        /// 4. Jika gagal, kembalikan HTTP 400.
        /// 5. Jika berhasil, kembalikan data alamat terbaru.
        /// </remarks>
        /// <param name="id">Id alamat yang akan diperbarui.</param>
        /// <param name="updateAddressRequest">Payload perubahan data alamat.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi alamat setelah diperbarui.</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(ApiResponse<AddressResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerRequestExample(typeof(UpdateAddressRequest), typeof(UpdateAddressRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(UpdateAddressSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(UpdateAddressBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(UpdateAddressNotFoundExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        public async Task<IActionResult> Update(
            Guid id,
            UpdateAddressRequest updateAddressRequest,
            CancellationToken cancellationToken)
        {
            // Step 1: Mapping route + request ke command aplikasi.
            var updateAddressCommand = new UpdateAddressCommand(
                id,
                updateAddressRequest.Label,
                updateAddressRequest.RecipientName,
                updateAddressRequest.RecipientPhone,
                updateAddressRequest.FullAddress,
                updateAddressRequest.City,
                updateAddressRequest.Province,
                updateAddressRequest.PostalCode
            );

            // Step 2: Eksekusi proses update alamat.
            var updateAddressResult = await _updateAddressHandler.Handle(updateAddressCommand, cancellationToken);

            // Step 3: Jika gagal, kirim response error.
            if (!updateAddressResult.IsSuccess)
                return BadRequestResponse(updateAddressResult.Error ?? "Update address failed");

            // Step 4: Mapping hasil update ke response DTO.
            var addressResponse = new AddressResponse(
                updateAddressResult.Value!.Id,
                updateAddressResult.Value.Label,
                updateAddressResult.Value.RecipientName,
                updateAddressResult.Value.RecipientPhone,
                updateAddressResult.Value.FullAddress,
                updateAddressResult.Value.City,
                updateAddressResult.Value.Province,
                updateAddressResult.Value.PostalCode,
                updateAddressResult.Value.IsDefault
            );

            // Step 5: Kembalikan response sukses.
            return SuccessResponse(addressResponse, "Address updated successfully");
        }

        /// <summary>
        /// Mengatur satu alamat sebagai alamat default.
        /// </summary>
        /// <remarks>
        /// Endpoint: POST api/addresses/{id}/set-default
        ///
        /// Flow set default address:
        /// 1. Terima id alamat dari route.
        /// 2. Bentuk SetDefaultAddressCommand.
        /// 3. Handler mengubah status default pada alamat terkait.
        /// 4. Jika gagal, kembalikan HTTP 400.
        /// 5. Jika berhasil, kembalikan alamat yang menjadi default.
        /// </remarks>
        /// <param name="id">Id alamat yang akan dijadikan default.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse berisi alamat default terbaru.</returns>
        [HttpPost("{id}/set-default")]
        [ProducesResponseType(typeof(ApiResponse<AddressResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(SetDefaultAddressSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(SetDefaultAddressBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(SetDefaultAddressNotFoundExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        public async Task<IActionResult> SetDefault(
            Guid id,
            CancellationToken cancellationToken)
        {
            // Step 1: Bentuk command set alamat default.
            var setDefaultAddressCommand = new SetDefaultAddressCommand(id);

            // Step 2: Eksekusi proses set default.
            var setDefaultAddressResult = await _setDefaultAddressHandler.Handle(setDefaultAddressCommand, cancellationToken);

            // Step 3: Jika gagal, kirim response error.
            if (!setDefaultAddressResult.IsSuccess)
                return BadRequestResponse(setDefaultAddressResult.Error ?? "Set default address failed");

            // Step 4: Mapping hasil ke response DTO.
            var addressResponse = new AddressResponse(
                setDefaultAddressResult.Value!.Id,
                setDefaultAddressResult.Value.Label,
                setDefaultAddressResult.Value.RecipientName,
                setDefaultAddressResult.Value.RecipientPhone,
                setDefaultAddressResult.Value.FullAddress,
                setDefaultAddressResult.Value.City,
                setDefaultAddressResult.Value.Province,
                setDefaultAddressResult.Value.PostalCode,
                setDefaultAddressResult.Value.IsDefault
            );

            // Step 5: Kembalikan response sukses.
            return SuccessResponse(addressResponse, "Default address updated successfully");
        }

        /// <summary>
        /// Menghapus alamat pengguna (soft delete).
        /// </summary>
        /// <remarks>
        /// Endpoint: DELETE api/addresses/{id}
        ///
        /// Flow delete address:
        /// 1. Terima id alamat dari route.
        /// 2. Bentuk DeleteAddressCommand.
        /// 3. Handler menjalankan proses soft delete alamat.
        /// 4. Jika gagal, kembalikan HTTP 400.
        /// 5. Jika berhasil, kembalikan id alamat yang dihapus.
        /// </remarks>
        /// <param name="id">Id alamat yang akan dihapus.</param>
        /// <param name="cancellationToken">Token pembatalan request async.</param>
        /// <returns>ApiResponse status penghapusan alamat.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(DeleteAddressSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(DeleteAddressBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(DeleteAddressNotFoundExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        public async Task<IActionResult> Delete(
            Guid id,
            CancellationToken cancellationToken)
        {
            // Step 1: Bentuk command delete alamat.
            var deleteAddressCommand = new DeleteAddressCommand(id);

            // Step 2: Eksekusi proses soft delete.
            var deleteAddressResult = await _deleteAddressHandler.Handle(deleteAddressCommand, cancellationToken);

            // Step 3: Jika gagal, kirim response error.
            if (!deleteAddressResult.IsSuccess)
                return BadRequestResponse(deleteAddressResult.Error ?? "Delete address failed");

            // Step 4: Jika sukses, kirim id alamat yang dihapus.
            return SuccessResponse(
                new { addressId = deleteAddressResult.Value },
                "Address deleted successfully"
            );
        }
    }
}
