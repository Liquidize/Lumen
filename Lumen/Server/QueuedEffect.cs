namespace Lumen.Server
{
    public class QueuedEffect
    {
        public string Effect { get; set; } = string.Empty;
        public string Id { get; set; } = new Guid().ToString("N").Substring(0, 8);
        public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();


        public QueuedEffect()
        {
        }

        public QueuedEffect(string effect, string id, Dictionary<string, object> settings)
        {
            Effect = effect;
            Id = id;
            Settings = settings;
        }

    }
}
