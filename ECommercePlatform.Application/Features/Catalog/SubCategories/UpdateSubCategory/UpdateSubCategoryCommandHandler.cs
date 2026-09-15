using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Catalog.Dtos;
using ECommercePlatform.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Application.Features.Catalog.SubCategories.UpdateSubCategory;

public sealed class UpdateSubCategoryCommandHandler
    : ICommandHandler<UpdateSubCategoryCommand, Result<SubCategoryResponse>>
{
    private readonly ISubCategoryRepository _subCategories;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateSubCategoryCommandHandler> _logger;

    public UpdateSubCategoryCommandHandler(
        ISubCategoryRepository subCategories,
        IUnitOfWork unitOfWork,
        ILogger<UpdateSubCategoryCommandHandler> logger)
    {
        _subCategories = subCategories;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<SubCategoryResponse>> Handle(
        UpdateSubCategoryCommand request, CancellationToken cancellationToken)
    {
        var subCategory = await _subCategories.GetByIdAsync(request.SubCategoryId, cancellationToken);

        if (subCategory is null)
        {
            return Result.Failure<SubCategoryResponse>(CatalogErrors.SubCategoryNotFound);
        }

        var code = request.SubCategoryCode.Trim();

        if (await _subCategories.CodeExistsAsync(code, request.SubCategoryId, cancellationToken))
        {
            return Result.Failure<SubCategoryResponse>(CatalogErrors.SubCategoryCodeTaken(code));
        }

        subCategory.SubCategoryCode = code;
        subCategory.SubCategoryName = request.SubCategoryName.Trim();
        subCategory.Description = request.Description?.Trim();
        subCategory.IsActive = request.IsActive;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Sub-category {SubCategoryId} updated.", subCategory.SubCategoryId);

        return Result.Success(subCategory.ToResponse());
    }
}
