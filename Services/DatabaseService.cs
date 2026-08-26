using DianxiaoMaui.Models;
using SQLite;
using System.Collections.Concurrent;

namespace DianxiaoMaui.Services;

/// <summary>本地 SQLite 数据库服务（离线存储）</summary>
public sealed class DatabaseService
{
    private static readonly Lazy<DatabaseService> _instance = new(() => new DatabaseService());
    public static DatabaseService Instance => _instance.Value;

    private SQLiteAsyncConnection? _db;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    private DatabaseService() { }

    public async Task InitAsync()
    {
        if (_db is not null) return;
        await _initLock.WaitAsync();
        try
        {
            if (_db is not null) return;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "dianxiao.db3");
            _db = new SQLiteAsyncConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
            await _db.CreateTableAsync<CallTask>();
            await _db.CreateTableAsync<Customer>();
            await _db.CreateTableAsync<CallLog>();
            await _db.CreateTableAsync<Blacklist>();
        }
        finally
        {
            _initLock.Release();
        }
    }

    #region CallTask
    public async Task<int> AddCallTaskAsync(string phone, int simSlot = 0)
    {
        await InitAsync();
        var existing = await _db!.Table<CallTask>()
            .Where(t => t.PhoneNumber == phone && t.Status == CallTask.STATUS_PENDING)
            .FirstOrDefaultAsync();
        if (existing is not null) return 0;
        var task = new CallTask { PhoneNumber = phone, SimSlot = simSlot };
        return await _db.InsertAsync(task);
    }

    public async Task<int> ImportNumbersAsync(IEnumerable<string> numbers, int simSlot = 0)
    {
        await InitAsync();
        int added = 0;
        foreach (var phone in numbers)
        {
            if (string.IsNullOrWhiteSpace(phone)) continue;
            var existing = await _db!.Table<CallTask>()
                .Where(t => t.PhoneNumber == phone && t.Status == CallTask.STATUS_PENDING)
                .FirstOrDefaultAsync();
            if (existing is not null) continue;
            await _db.InsertAsync(new CallTask { PhoneNumber = phone, SimSlot = simSlot });
            added++;
        }
        return added;
    }

    public async Task<List<CallTask>> GetActiveTasksAsync()
    {
        await InitAsync();
        return await _db!.Table<CallTask>()
            .OrderByDescending(t => t.Id)
            .ToListAsync();
    }

    public async Task<CallTask?> GetPendingTaskAsync()
    {
        await InitAsync();
        return await _db!.Table<CallTask>()
            .Where(t => t.Status == CallTask.STATUS_PENDING)
            .OrderBy(t => t.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<int> UpdateTaskAsync(CallTask task)
    {
        await InitAsync();
        return await _db!.UpdateAsync(task);
    }

    public async Task<int> DeleteTaskAsync(int id)
    {
        await InitAsync();
        return await _db!.DeleteAsync<CallTask>(id);
    }

    public async Task<int> GetPendingCountAsync()
    {
        await InitAsync();
        return await _db!.Table<CallTask>()
            .Where(t => t.Status == CallTask.STATUS_PENDING)
            .CountAsync();
    }

    public async Task<List<CallTask>> GetDoneTasksAsync()
    {
        await InitAsync();
        return await _db!.Table<CallTask>()
            .Where(t => t.Status == CallTask.STATUS_DONE)
            .OrderByDescending(t => t.Id)
            .ToListAsync();
    }
    #endregion

    #region Customer
    public async Task<int> AddCustomerAsync(Customer c)
    {
        await InitAsync();
        c.UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return await _db!.InsertAsync(c);
    }

    public async Task<int> UpdateCustomerAsync(Customer c)
    {
        await InitAsync();
        c.UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return await _db!.UpdateAsync(c);
    }

    public async Task<int> DeleteCustomerAsync(int id)
    {
        await InitAsync();
        return await _db!.DeleteAsync<Customer>(id);
    }

    public async Task<List<Customer>> GetAllCustomersAsync()
    {
        await InitAsync();
        return await _db!.Table<Customer>().OrderByDescending(c => c.Id).ToListAsync();
    }

    public async Task<List<Customer>> SearchCustomersAsync(string keyword)
    {
        await InitAsync();
        if (string.IsNullOrWhiteSpace(keyword))
            return await GetAllCustomersAsync();
        return await _db!.Table<Customer>()
            .Where(c => c.Name.Contains(keyword) || c.Phone.Contains(keyword))
            .OrderByDescending(c => c.Id)
            .ToListAsync();
    }

    public async Task<Customer?> GetCustomerByPhoneAsync(string phone)
    {
        await InitAsync();
        return await _db!.Table<Customer>().Where(c => c.Phone == phone).FirstOrDefaultAsync();
    }

    public async Task<Customer?> GetCustomerByIdAsync(int id)
    {
        await InitAsync();
        return await _db!.Table<Customer>().Where(c => c.Id == id).FirstOrDefaultAsync();
    }
    #endregion

    #region CallLog
    public async Task<int> AddCallLogAsync(CallLog log)
    {
        await InitAsync();
        return await _db!.InsertAsync(log);
    }

    public async Task<List<CallLog>> GetCallLogsAsync(int filter = 0)
    {
        await InitAsync();
        var query = _db!.Table<CallLog>();
        if (filter == 1) query = query.Where(l => l.Connected);
        else if (filter == 2) query = query.Where(l => !l.Connected);
        return await query.OrderByDescending(l => l.Id).ToListAsync();
    }

    public async Task<List<CallLog>> GetCallLogsByPhoneAsync(string phone)
    {
        await InitAsync();
        return await _db!.Table<CallLog>()
            .Where(l => l.PhoneNumber == phone)
            .OrderByDescending(l => l.Id)
            .ToListAsync();
    }

    public async Task<(int total, int connected, int unconnected, int totalDuration)> GetStatsAsync()
    {
        await InitAsync();
        var logs = await _db!.Table<CallLog>().ToListAsync();
        int total = logs.Count;
        int connected = logs.Count(l => l.Connected);
        int unconnected = total - connected;
        int totalDuration = logs.Sum(l => l.DurationSec);
        return (total, connected, unconnected, totalDuration);
    }
    #endregion

    #region Blacklist
    public async Task<int> AddBlacklistAsync(string phone, string? reason = null)
    {
        await InitAsync();
        var existing = await _db!.Table<Blacklist>().Where(b => b.PhoneNumber == phone).FirstOrDefaultAsync();
        if (existing is not null) return 0;
        return await _db.InsertAsync(new Blacklist { PhoneNumber = phone, Reason = reason });
    }

    public async Task<int> RemoveBlacklistAsync(int id)
    {
        await InitAsync();
        return await _db!.DeleteAsync<Blacklist>(id);
    }

    public async Task<List<Blacklist>> GetBlacklistAsync()
    {
        await InitAsync();
        return await _db!.Table<Blacklist>().OrderByDescending(b => b.Id).ToListAsync();
    }

    public async Task<bool> IsBlacklistedAsync(string phone)
    {
        await InitAsync();
        return await _db!.Table<Blacklist>().Where(b => b.PhoneNumber == phone).FirstOrDefaultAsync() is not null;
    }
    #endregion
}
