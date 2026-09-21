namespace KindRaise.Domain.Exceptions
{
    public class CampaignDeletionNotAllowedException : Exception
    {
        public CampaignDeletionNotAllowedException(string message) : base(message)
        {
        }
    }
}
