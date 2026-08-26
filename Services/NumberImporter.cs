using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace DianxiaoMaui.Services;

/// <summary>号码导入服务（支持文本粘贴/CSV）</summary>
public static class NumberImporter
{
    public static List<string> ImportFromText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return new();
        var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in lines)
        {
            var phone = ExtractPhone(line);
            if (!string.IsNullOrWhiteSpace(phone))
                result.Add(phone);
        }
        return result.ToList();
    }

    public static async Task<List<string>> ImportFromCsvAsync(Stream stream, string? phoneColumn = null)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
            MissingFieldFound = null
        });

        await csv.ReadAsync();
        csv.ReadHeader();

        int phoneIndex = -1;
        if (!string.IsNullOrEmpty(phoneColumn))
        {
            phoneIndex = csv.GetFieldIndex(phoneColumn);
        }

        while (await csv.ReadAsync())
        {
            string phone;
            if (phoneIndex >= 0)
            {
                phone = csv.GetField(phoneIndex) ?? "";
            }
            else
            {
                // 尝试常见列名
                phone = csv.GetField("phone") ?? csv.GetField("手机号") ?? csv.GetField("电话") ?? csv.GetField("mobile") ?? "";
            }
            phone = ExtractPhone(phone);
            if (!string.IsNullOrWhiteSpace(phone))
                result.Add(phone);
        }
        return result.ToList();
    }

    private static string ExtractPhone(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";
        var digits = new string(input.Where(char.IsDigit).ToArray());
        // 简单校验：11 位手机号
        return digits.Length >= 11 ? digits[^11..] : "";
    }
}