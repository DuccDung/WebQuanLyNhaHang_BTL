using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebQuanLyNhaHang.Models;

public sealed class OnlineOrderMetadata
{
    public const string OnlineType = "online";
    public const string StatusCart = "cart";
    public const string StatusPending = "pending";
    public const string StatusPreparing = "preparing";
    public const string StatusShipping = "shipping";
    public const string StatusDelivered = "delivered";
    public const string StatusCancelled = "cancelled";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        StatusCart,
        StatusPending,
        StatusPreparing,
        StatusShipping,
        StatusDelivered,
        StatusCancelled
    };

    [JsonPropertyName("t")]
    public string Type { get; set; } = OnlineType;

    [JsonPropertyName("s")]
    public string DeliveryStatus { get; set; } = StatusCart;

    [JsonPropertyName("n")]
    public string? RecipientName { get; set; }

    [JsonPropertyName("p")]
    public string? Phone { get; set; }

    [JsonPropertyName("c")]
    public string? City { get; set; }

    [JsonPropertyName("d")]
    public string? District { get; set; }

    [JsonPropertyName("w")]
    public string? Ward { get; set; }

    [JsonPropertyName("a")]
    public string? AddressLine { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }

    [JsonPropertyName("at")]
    public DateTime? SubmittedAt { get; set; }

    [JsonIgnore]
    public string FullAddress => string.Join(", ", new[]
    {
        AddressLine,
        Ward,
        District,
        City
    }.Where(item => !string.IsNullOrWhiteSpace(item)));

    public static OnlineOrderMetadata CreateCart()
    {
        return new OnlineOrderMetadata
        {
            Type = OnlineType,
            DeliveryStatus = StatusCart
        };
    }

    public static OnlineOrderMetadata? TryParse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.TrimStart().StartsWith("{", StringComparison.Ordinal))
        {
            return null;
        }

        try
        {
            var metadata = JsonSerializer.Deserialize<OnlineOrderMetadata>(value, JsonOptions);
            return string.Equals(metadata?.Type, OnlineType, StringComparison.OrdinalIgnoreCase)
                ? metadata
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static bool IsOnlineOrder(DonHang order)
    {
        return order.VanChuyen == true && TryParse(order.GhiChu) != null;
    }

    public static bool IsOnlineCart(DonHang order)
    {
        var metadata = TryParse(order.GhiChu);
        return order.VanChuyen == true
            && order.TrangThai != true
            && string.Equals(metadata?.DeliveryStatus, StatusCart, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsValidStatus(string? status)
    {
        return !string.IsNullOrWhiteSpace(status) && ValidStatuses.Contains(status.Trim());
    }

    public static (string Key, string Label, string CssClass) ResolveStatusDisplay(string? status)
    {
        return NormalizeStatus(status) switch
        {
            StatusPreparing => ("preparing", "Đang chuẩn bị", "is-processing"),
            StatusShipping => ("shipping", "Đang giao", "is-shipping"),
            StatusDelivered => ("delivered", "Đã giao", "is-completed"),
            StatusCancelled => ("cancelled", "Đã hủy", "is-cancelled"),
            StatusCart => ("cart", "Giỏ hàng", "is-pending"),
            _ => ("pending", "Chờ xác nhận", "is-pending")
        };
    }

    public static string NormalizeStatus(string? status)
    {
        var value = status?.Trim().ToLowerInvariant();
        return IsValidStatus(value) ? value! : StatusPending;
    }

    public string ToJson()
    {
        NormalizeFields();

        var json = JsonSerializer.Serialize(this, JsonOptions);
        if (json.Length <= 500)
        {
            return json;
        }

        Note = Trim(Note, 60);
        json = JsonSerializer.Serialize(this, JsonOptions);
        if (json.Length <= 500)
        {
            return json;
        }

        AddressLine = Trim(AddressLine, 120);
        Ward = Trim(Ward, 40);
        District = Trim(District, 40);
        City = Trim(City, 40);
        json = JsonSerializer.Serialize(this, JsonOptions);
        if (json.Length <= 500)
        {
            return json;
        }

        var compact = new OnlineOrderMetadata
        {
            Type = Type,
            DeliveryStatus = DeliveryStatus,
            RecipientName = Trim(RecipientName, 60),
            Phone = Trim(Phone, 20),
            AddressLine = Trim(FullAddress, 260),
            SubmittedAt = SubmittedAt
        };

        return JsonSerializer.Serialize(compact, JsonOptions);
    }

    private void NormalizeFields()
    {
        Type = OnlineType;
        DeliveryStatus = NormalizeStatus(DeliveryStatus);
        RecipientName = Trim(RecipientName, 80);
        Phone = Trim(Phone, 20);
        City = Trim(City, 60);
        District = Trim(District, 60);
        Ward = Trim(Ward, 60);
        AddressLine = Trim(AddressLine, 180);
        Note = Trim(Note, 120);
    }

    private static string? Trim(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}
