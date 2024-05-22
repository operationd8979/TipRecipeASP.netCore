namespace TipRecipe.Models.HttpExceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message = "Resource not found!") : base(message) { }
        public NotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}
