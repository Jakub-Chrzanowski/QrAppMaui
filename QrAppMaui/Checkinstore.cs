using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
namespace QrAppMaui
{
    internal class Checkinstore
    {
        static SQLiteAsyncConnection? _db;
        static readonly SemaphoreSlim _lock = new(1, 1);

        public static ObservableCollection<Checkinentry> Entries { get; } = new();

        public static string DbPath =>
            Path.Combine(FileSystem.AppDataDirectory, "checkins.db3");

        public static async Task InitAsync()
        {
            if (_db is not null)
                return;

            await _lock.WaitAsync();
            try
            {
                if (_db is not null)
                    return;

                var db = new SQLiteAsyncConnection(DbPath);
                await db.CreateTableAsync<Checkinentry>();

                var all = await db.Table<Checkinentry>()
                                  .OrderByDescending(x => x.Timestamp)
                                  .ToListAsync();

                Entries.Clear();
                foreach (var entry in all)
                    Entries.Add(entry);

                _db = db;
            }
            finally
            {
                _lock.Release();
            }
        }

        public static async Task AddAsync(Checkinentry entry)
        {
            await InitAsync();
            await _db!.InsertAsync(entry);
            Entries.Insert(0, entry);
        }
    }
}
