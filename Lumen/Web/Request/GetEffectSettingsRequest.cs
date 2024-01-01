namespace Lumen.Web.Request
{
        public record GetEffectSettingsRequest(string Location, string Id)
        {
            public GetEffectSettingsRequest() : this(string.Empty, string.Empty)
            {

            }
        }
}
