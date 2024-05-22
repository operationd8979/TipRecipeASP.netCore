namespace TipRecipe.Models.HttpExceptions
{
    public class ValidationException : Exception
    {
        public ValidationException(string message = "Validation failed!") : base(message) { }
        public ValidationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
