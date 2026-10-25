using DuckDB.NET.Data;

namespace FileSorter.Extensions;

public static class DbExtensions
{
    public static int ExecuteNonQuery(this DuckDBConnection connection, string sql)
    {
        using DuckDBCommand cmd = connection.CreateCommand();
        cmd.CommandText = sql;
    
        return cmd.ExecuteNonQuery();
    }
}
