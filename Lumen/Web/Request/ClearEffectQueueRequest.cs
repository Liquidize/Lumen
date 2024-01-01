namespace Lumen.Web.Request
{
    public record ClearEffectQueueRequest(string Location)
    {
        public ClearEffectQueueRequest() : this(string.Empty)
        {

        }
    }
}
