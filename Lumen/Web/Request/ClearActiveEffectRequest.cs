namespace Lumen.Web.Request
{
    public record ClearActiveEffectRequest(string Location)
    {
        public ClearActiveEffectRequest() : this(string.Empty)
        {

        }
    }
}