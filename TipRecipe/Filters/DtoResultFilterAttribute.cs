using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TipRecipe.Filters
{
    public class DtoResultFilterAttribute<TSource, TDestination> : IAsyncResultFilter
    {
        private IMapper _mapper;

        public DtoResultFilterAttribute(IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var objectResult = context.Result as ObjectResult;
            if(objectResult?.Value == null 
                || objectResult.StatusCode < 200 
                || objectResult.StatusCode >= 300 
                || objectResult.Value is not TSource sourceObject)
            {
                await next();
                return;
            }
            objectResult.Value = _mapper.Map<TDestination>(sourceObject);
            await next();
        }
    }
}
