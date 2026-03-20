namespace techviet_be.Common.Options;

public class VnPayOptions
{
    public const string SectionName = "VnPay";

    public string TmnCode { get; set; } = string.Empty;
    public string HashSecret { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
}