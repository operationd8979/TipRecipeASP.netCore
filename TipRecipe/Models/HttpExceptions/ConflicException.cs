namespace TipRecipe.Models.HttpExceptions
{
    public class ConflicException : Exception
    {
        public ConflicException(string message = "Resource conflic current base!") : base(message) { }
        public ConflicException(string message, Exception innerException) : base(message, innerException) { }
    }
}
