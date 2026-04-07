namespace GoAir.Web.ModelBinding
{
    using Microsoft.AspNetCore.Mvc.ModelBinding;

    public class FlexibleDecimalModelBinderProvider : IModelBinderProvider
    {
        private readonly IModelBinder binder = new FlexibleDecimalModelBinder();
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            Type? modelType = context.Metadata.UnderlyingOrModelType;
            return modelType == typeof(decimal) ? binder : null;
        }
    }
}