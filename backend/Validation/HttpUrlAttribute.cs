using System.ComponentModel.DataAnnotations;

namespace ProjectBrain.Api.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class HttpUrlAttribute : ValidationAttribute
{
    public HttpUrlAttribute() => ErrorMessage = "地址必须是完整的 HTTP 或 HTTPS URL";

    public override bool IsValid(object? value)
    {
        if (value is null) return true;
        if (value is not string text) return false;
        text = text.Trim();
        if (text.Length == 0) return true;

        return Uri.TryCreate(text, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) &&
               !string.IsNullOrWhiteSpace(uri.Host);
    }
}
