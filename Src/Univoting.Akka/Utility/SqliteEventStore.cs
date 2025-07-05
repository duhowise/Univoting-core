using System.Data.SQLite;

namespace Univoting.Akka.Utility;

public static class SqliteEventStore
{
    public static void InitializeDatabase()
    {
        var dbPath = "univoting_akka.db";
        var directory = Path.GetDirectoryName(Path.GetFullPath(dbPath));
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Create database file if it doesn't exist
        if (!File.Exists(dbPath))
        {
            SQLiteConnection.CreateFile(dbPath);
        }
    }
}
