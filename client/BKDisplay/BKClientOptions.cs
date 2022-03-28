namespace BKDisplay;

public sealed record class BKClientOptions
{
    public string Key { get; set; }
    public string AdditionalData { get; set; }
    public string Host { get; set; }
    public int Port { get; set; }
}
