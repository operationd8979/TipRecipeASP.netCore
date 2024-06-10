using Amazon.Auth.AccessControlPolicy;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using TipRecipe.Services;
using TipRecipe.Models.Dto;

namespace TipRecipe.Filters
{
    public class AddSasBlobFilterAttribute : IAsyncResultFilter
    {
        private AzureBlobService _azureBlobService;

        public AddSasBlobFilterAttribute(AzureBlobService azureBlobService)
        {
            _azureBlobService = azureBlobService ?? throw new ArgumentNullException(nameof(azureBlobService));
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var objectResult = context.Result as ObjectResult;
            if (objectResult?.Value == null
                || objectResult.StatusCode < 200
                || objectResult.StatusCode >= 300)
            {
                await next();
                return;
            }
            if(objectResult.Value is DishDto sourceObject)
            {
                sourceObject.UrlPhoto = _azureBlobService.GenerateSasTokenPolicy(sourceObject.UrlPhoto!);
                objectResult.Value = sourceObject;
            }
            else if(objectResult.Value is IEnumerable<DishDto> sourceObjectList)
            {
                foreach (var dish in sourceObjectList)
                {
                    dish.UrlPhoto = _azureBlobService.GenerateSasTokenPolicy(dish.UrlPhoto!);
                }
                objectResult.Value = sourceObjectList;
            }
            await next();
        }
    }
}
