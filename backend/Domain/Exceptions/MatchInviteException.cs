namespace Domain.Exceptions
{
    public class MatchInviteException : Exception
    {
        public MatchInviteException()
        {
        }

        public MatchInviteException(string message)
            : base(message)
        {
        }

        public MatchInviteException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}