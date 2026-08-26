using SQLite;
using System.Windows.Input;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace DianxiaoMaui.Models;

/// <summary>通话记录实体</summary>
[Table("call_logs")]
public class CallLog
{
    [PrimaryKey, AutoIncrement, Column("id")]
    public int Id { get; set; }

    [Column("phone_number"), NotNull, MaxLength(32)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Column("customer_name"), MaxLength(64)]
    public string? CustomerName { get; set; }

    [Column("connected")]
    public bool Connected { get; set; }

    [Column("duration_sec")]
    public int DurationSec { get; set; }

    [Column("start_time")]
    public long StartTime { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    [Column("end_time")]
    public long EndTime { get; set; }

    [Column("recording_path"), MaxLength(256)]
    public string? RecordingPath { get; set; }

    [Column("task_id")]
    public int TaskId { get; set; }

    [Column("sim_slot")]
    public int SimSlot { get; set; }

    [Ignore]
    public string DurationText =>
        $"{DurationSec / 60:D2}:{DurationSec % 60:D2}";

    [Ignore]
    public string TimeText =>
        DateTimeOffset.FromUnixTimeMilliseconds(StartTime).LocalDateTime.ToString("MM-dd HH:mm");

    [Ignore]
    public string ConnectedText => Connected ? "已接通" : "未接通";

    [Ignore]
    public bool HasRecording => !string.IsNullOrWhiteSpace(RecordingPath) && File.Exists(RecordingPath);

    [Ignore]
    public ICommand PlayCommand => new Command(async () =>
    {
        if (HasRecording)
        {
            try
            {
                await Launcher.Default.OpenAsync(new OpenApiOptions
                {
                    Target = Target.Uri(RecordingPath)
                });
            }
            catch { }
        }
    });
}