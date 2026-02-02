using EComAPI.API.Auth.Swagger.Examples;
using EComAPI.API.Authorization;
using EComAPI.API.Categories.Dtos.Requests;
using EComAPI.API.Categories.Dtos.Responses;
using EComAPI.API.Categories.Swagger.Examples.CategoryExample.Request;
using EComAPI.API.Categories.Swagger.Examples.CategoryExample.Response;
using EComAPI.API.Categories.Swagger.Examples.DeleteCategory.Response;
using EComAPI.API.Categories.Swagger.Examples.RestoreCategory.Response;
using EComAPI.API.Categories.Swagger.Examples.UpdateCategory.Request;
using EComAPI.API.Categories.Swagger.Examples.UpdateCategory.Response;
using EComAPI.API.Common;
using EComAPI.Application.Categories.Commands.CreateCategory;
using EComAPI.Application.Categories.Commands.DeleteCategory;
using EComAPI.Application.Categories.Commands.RestoreCategory;
using EComAPI.Application.Categories.Commands.UpdateCategory;
using EComAPI.Application.Categories.Queries.GetCategories;
using EComAPI.Application.Common.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Categories.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : BaseController
    {
        private readonly CreateCategoryHandler _createCategoryHandler;
        private readonly GetCategoriesHandler _getCategoriesHandler;
        private readonly UpdateCategoryHandler _updateCategoryHandler;
        private readonly DeleteCategoryHandler _deleteCategoryHandler;
        private readonly RestoreCategoryHandler _restoreCategoryHandler;

        public CategoriesController(
            CreateCategoryHandler createCategoryHandler,
            GetCategoriesHandler getCategoriesHandler,
            UpdateCategoryHandler updateCategoryHandler,
            DeleteCategoryHandler deleteCategoryHandler,
            RestoreCategoryHandler restoreCategoryHandler)
        {
            _createCategoryHandler = createCategoryHandler;
            _getCategoriesHandler = getCategoriesHandler;
            _updateCategoryHandler = updateCategoryHandler;
            _deleteCategoryHandler = deleteCategoryHandler;
            _restoreCategoryHandler = restoreCategoryHandler;
        }

        /// <summary>
        /// New category product
        /// </summary>
        /// <remarks>
        ///     POST api/categories/createCategory
        /// </remarks>
        /// <response code="200">Category created success</response>
        /// <response code="400">Create category failed</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost]
        [HasPermission(Permissions.Categories.Create)]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerRequestExample(typeof(CreateCategoryRequest), typeof(CreateCategoryRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(CreateCategorySuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(CreateCategoryBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        public async Task<IActionResult> Create(
            CreateCategoryRequest createCategoryRequest,
            CancellationToken cancellationToken)
        {
            var createCategoryResult = await _createCategoryHandler.Handle(
                new CreateCategoryCommand(
                    createCategoryRequest.Name,
                    createCategoryRequest.Slug,
                    createCategoryRequest.ParentId),
                cancellationToken
            );

            if (!createCategoryResult.IsSuccess)
                return BadRequestResponse("Create category failed");

            return SuccesResponse(
                new CategoryIdResponse(createCategoryResult.Value),
                "Category created success"
            );
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        /// <remarks>
        ///     GET api/categories
        /// </remarks>
        /// <response code="200">Success list categories</response>
        /// <response code="400">Failed list categories</response>
        [HttpGet]
        [AllowAnonymous]
        [HasPermission(Permissions.Categories.Read)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(GetAllCategoriesSuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(GetAllCategoriesBadRequestExample))]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await _getCategoriesHandler.Handle(
                new GetCategoriesQuery(),
                cancellationToken
            );

            if (!result.IsSuccess)
                return BadRequestResponse("Failed list categories");

            var categories = result.Value!
                .Select(c => new CategoryResponse(
                    c.Id,
                    c.Name,
                    c.Slug,
                    c.ParentId,
                    c.CreatedAt,
                    c.CreatedBy,
                    c.UpdatedAt,
                    c.UpdatedBy
                ))
                .ToList();

            return SuccesResponse(new
            {
                product_categories = categories
            }, "Success list categories");
        }

        /// <summary>
        /// Update category product
        /// </summary>
        /// <remarks>
        ///     PUT api/categories/{id:guid}/update
        /// </remarks>
        /// <response code="200">Category updated successfully</response>
        /// <response code="400">Category update failed</response>
        /// <response code="401">Unauthorized</response>
        [HttpPut]
        [Route("{id:guid}/update")]
        [HasPermission(Permissions.Categories.Update)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerRequestExample(typeof(UpdateCategoryRequest), typeof(UpdateCategoryRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(UpdateCategorySuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(UpdateCategoryBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        public async Task<IActionResult> Update(
            Guid id,
            UpdateCategoryRequest updateCategoryRequest,
            CancellationToken cancellationToken)
        {
            var updateCategoryResult = await _updateCategoryHandler.Handle(
                new UpdateCategoryCommand(
                    id,
                    updateCategoryRequest.Name,
                    updateCategoryRequest.Slug,
                    updateCategoryRequest.ParentId),
                cancellationToken
            );

            if (!updateCategoryResult.IsSuccess)
                return BadRequestResponse("Category update failed");

            return SuccesResponse(
                new CategoryIdResponse(updateCategoryResult.Value),
                "Category updated successfully"
            );
        }

        /// <summary>
        /// Soft delete category
        /// </summary>
        /// <remarks>
        ///     DELETE api/categories/{id:guid}
        /// </remarks>
        /// <response code="200">Category deleted successfully</response>
        /// <response code="400">Category delete failed</response>
        /// <response code="401">Unauthorized</response>
        [HttpDelete]
        [Route("{id:guid}")]
        [HasPermission(Permissions.Categories.Delete)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(DeleteCategorySuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(DeleteCategoryBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        public async Task<IActionResult> Delete(
            Guid id,
            CancellationToken cancellationToken)
        {
            var deleteCategoryResult = await _deleteCategoryHandler.Handle(
                new DeleteCategoryCommand(id),
                cancellationToken
            );

            if (!deleteCategoryResult.IsSuccess)
                return BadRequestResponse(deleteCategoryResult.Error);

            return SuccesResponse(
                new CategoryIdResponse(deleteCategoryResult.Value),
                "Category deleted successfully"
            );
        }

        /// <summary>
        /// Restore soft deleted category
        /// </summary>
        /// <remarks>
        ///     PUT api/categories/{id:guid}/restore
        /// </remarks>
        /// <response code="200">Category restored successfully</response>
        /// <response code="400">Category restore failed</response>
        /// <response code="401">Unauthorized</response>
        [HttpPut]
        [Route("{id:guid}/restore")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(RestoreCategorySuccessExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(RestoreCategoryBadRequestExample))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnauthorizedExample))]
        public async Task<IActionResult> Restore(
            Guid id,
            CancellationToken cancellationToken)
        {
            var restoreCategoryResult = await _restoreCategoryHandler.Handle(
                new RestoreCategoryCommand(id),
                cancellationToken
            );

            if (!restoreCategoryResult.IsSuccess)
                return BadRequestResponse(restoreCategoryResult.Error);

            return SuccesResponse(
                new CategoryIdResponse(restoreCategoryResult.Value),
                "Category restored successfully"
            );
        }
    }
}