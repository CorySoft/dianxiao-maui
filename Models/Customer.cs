using SQLite;

namespace DianxiaoMaui.Models;

/// <summary>客户实体</summary>
[Table("customers")]
public class Customer
{
    [PrimaryKey, AutoIncrement, Column("id")]
    public int Id { get; set; }

    [Column("name"), NotNull, MaxLength(64)]
    public string Name { get; set; } = string.Empty;

    [Column("phone"), NotNull, MaxLength(32)]
    public string Phone { get; set; } = string.Empty;

    [Column("tags"), MaxLength(256)]
    public string? Tags { get; set; }

    [Column("note"), MaxLength(512)]
    public string? Note { get; set; }

    [Column("intent_level")]
    public int IntentLevel { get; set; } = 0; // 0=未分类, 1=低意向, 2=中意向, 3=高意向

    [Column("created_at")]
    public long CreatedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    [Column("updated_at")]
    public long UpdatedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    [Ignore]
    public string DisplayPhone => Phone.Length >= 7
        ? Phone[..3] + "****" + Phone[^4..]
        : Phone;

    [Ignore]
    public List<string> TagList =>
        string.IsNullOrWhiteSpace(Tags)
            ? new List<string>()
            : Tags.Split(new[] { ',', '，', ';', '；', '|', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

    [Ignore]
    public string IntentText => IntentLevel switch
    {
        1 => "低意向",
        2 => "中意向",
        3 => "高意向",
        _ => "未分类"
    };

    [Ignore]
    public Color IntentColor => IntentLevel switch
    {
        3 => (Color)Application.Current!.Resources["SuccessColor"]!,
        2 => (Color)Application.Current!.Resources["BrandColor"]!,
        1 => (Color)Application.Current!.Resources["PrimaryColor"]!,
        _ => (Color)Application.Current!.Resources["TextSecondaryColor"]!
    };
}