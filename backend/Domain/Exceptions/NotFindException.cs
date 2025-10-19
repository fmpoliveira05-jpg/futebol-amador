namespace Domain.Exceptions
{
    public class NotFindException : Exception
    {
        public NotFindException()
        {
        }

        public NotFindException(string message)
            : base(message)
        {
        }

        public NotFindException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
