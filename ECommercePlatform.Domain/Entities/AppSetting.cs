namespace ECommercePlatform.Domain.Entities
{
    /// <summary>
    /// Key/value admin settings. Values are stored as raw JSON text so any
    /// settings shape round-trips without schema changes.
    /// </summary>
    public class AppSetting : AdminEntity
    {
        public string Key { get; set; } = string.Empty;

        public string? Value { get; set; }
    }
}
