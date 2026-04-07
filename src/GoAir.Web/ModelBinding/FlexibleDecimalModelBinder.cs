namespace GoAir.Web.ModelBinding
{
    using System.Globalization;

    using Microsoft.AspNetCore.Mvc.ModelBinding;
    public class FlexibleDecimalModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }
            ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }
            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);
            string? rawValue = valueProviderResult.FirstValue?.Trim();
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return Task.CompletedTask;
            }
            if (TryParseDecimal(rawValue, out decimal parsedValue))
            {
                bindingContext.Result = ModelBindingResult.Success(parsedValue);
                return Task.CompletedTask;
            }
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Please enter a valid decimal amount.");
            return Task.CompletedTask;
        }
        private static bool TryParseDecimal(string value, out decimal parsedValue)
        {
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            if (decimal.TryParse(value, NumberStyles.Number, currentCulture, out parsedValue) ||
            decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out parsedValue))
            {
                return true;
            }
            string normalizedValue = value.Replace(',', '.');
            return decimal.TryParse(normalizedValue, NumberStyles.Number, CultureInfo.InvariantCulture, out parsedValue);
        }
    }
}