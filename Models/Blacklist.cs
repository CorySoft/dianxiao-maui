using SQLite;

namespace DianxiaoMaui.Models;

/// <summary>黑名单实体</summary>
[Table("blacklist")]
public class Blacklist
{
    [PrimaryKey, AutoIncrement, Column("id")]
    public int Id { get; set; }

    [Column("phone_number"), NotNull, MaxLength(32), Unique]
    public string PhoneNumber { get; set; } = string.Empty;

    [Column("reason"), MaxLength(128)]
    public string? Reason { get; set; }

    [Column("created_at")]
    public long CreatedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    [Ignore]
    public string MaskedNumber => PhoneNumber.Length >= 7
        ? PhoneNumber[..3] + "****" + PhoneNumber[^4..]
        : PhoneNumber;
}