using SQLite;

namespace DianxiaoMaui.Models;

/// <summary>自动拨号任务实体</summary>
[Table("call_tasks")]
public class CallTask
{
    public const int STATUS_PENDING = 0;
    public const int STATUS_DIALING = 1;
    public const int STATUS_CALLED = 2;
    public const int STATUS_DONE = 3;
    public const int STATUS_SKIPPED = 4;

    [PrimaryKey, AutoIncrement, Column("id")]
    public int Id { get; set; }

    [Column("phone_number"), NotNull, MaxLength(32)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Column("customer_id")]
    public int CustomerId { get; set; }

    [Column("status")]
    public int Status { get; set; } = STATUS_PENDING;

    [Column("created_at")]
    public long CreatedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    [Column("started_at")]
    public long StartedAt { get; set; }

    [Column("finished_at")]
    public long FinishedAt { get; set; }

    [Column("duration_sec")]
    public int DurationSec { get; set; }

    [Column("result")]
    public int Result { get; set; } = -1; // -1=未知, 0=未接通, 1=接通

    [Column("recording_path"), MaxLength(256)]
    public string? RecordingPath { get; set; }

    [Column("sim_slot")]
    public int SimSlot { get; set; } = 0; // 0=默认, 1=卡1, 2=卡2

    [Ignore]
    public string MaskedNumber => PhoneNumber.Length >= 7
        ? PhoneNumber[..3] + "****" + PhoneNumber[^4..]
        : PhoneNumber;

    [Ignore]
    public string StatusText => Status switch
    {
        STATUS_PENDING => "待拨打",
        STATUS_DIALING => "拨打中",
        STATUS_CALLED => "已呼出",
        STATUS_DONE => "完成",
        STATUS_SKIPPED => "已跳过",
        _ => "未知"
    };

    [Ignore]
    public string ResultText => Result switch
    {
        1 => "接通",
        0 => "未接通",
        _ => ""
    };
}