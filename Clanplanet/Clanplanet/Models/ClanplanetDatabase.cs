using Clanplanet.Dependencies;
using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Clanplanet.Models
{
    internal class ClanplanetDatabase
    {
        static readonly object locker = new object();
        readonly SQLiteAsyncConnection database;

        public ClanplanetDatabase(string dbPath)
        {
            database = new SQLiteAsyncConnection(dbPath);
            database.CreateTableAsync<ClanEventAppointment>().Wait();
        }

        public Task<List<ClanEventAppointment>> GetItemsAsync()
        {
            return database.Table<ClanEventAppointment>().ToListAsync();
        }

        public Task<List<ClanEventAppointment>> GetByEventID(string eventID)
        {
            return database.QueryAsync<ClanEventAppointment>("SELECT * FROM [ClanEventAppointment] WHERE [EventID] = '" + eventID + "'");
        }

        public Task<ClanEventAppointment> GetItemAsync(int id)
        {
            return database.Table<ClanEventAppointment>().Where(i => i.ID == id).FirstOrDefaultAsync();
        }

        public Task<int> SaveItemAsync(ClanEventAppointment item)
        {
            if (item.ID != 0)
            {
                return database.UpdateAsync(item);
            }
            else
            {
                return database.InsertAsync(item);
            }
        }

        public Task<int> DeleteItemAsync(ClanEventAppointment item)
        {
            return database.DeleteAsync(item);
        }

        public Task<int> DeleteEventItemsAsync(string eventID)
        {
            return database.ExecuteAsync("DELETE FROM [ClanEventAppointment] WHERE [EventID] = '" + eventID + "'");
        }
    }

    internal class ClanEventAppointment
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Name { get; set; }
        public string EventID { get; set; }
        public string TerminID { get; set; }
        public string ReminderID { get; set; }
    }
}