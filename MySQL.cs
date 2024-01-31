using DBMonitorFileTracker;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace YunYan
{
    internal class MySQL : IDisposable
    {
        private MySqlConnection _conn;

        public MySQL()
        {
            var connectionString = ConfigurationManager.AppSettings["connectString"];
            _conn = new MySqlConnection(connectionString);
        }

        private void ExecuteAction(Action action)
        {
            try
            {
                _conn.Open();
                action();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _conn.Close();
            }
        }

        public DataTable SelectTable(string query, Dictionary<string, object> parameters = null)
        {
            var dt = new DataTable();
            ExecuteAction(() =>
            {
                using (var cmd = new MySqlCommand(query, _conn))
                {
                    // 檢查並處理參數
                    if (parameters?.Count > 0)
                    {
                        var conditions = parameters.Select(x => $"{x.Key} = @{x.Key}").ToList();
                        cmd.CommandText += " WHERE " + string.Join(" AND ", conditions);

                        foreach (var param in parameters)
                        {
                            cmd.Parameters.AddWithValue($"@{param.Key}", param.Value);
                        }
                    }

                    // 使用 MySqlDataAdapter 填充 DataTable
                    using (var adapter = new MySqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            });
            return dt;
        }



        public void InsertTable(string tableName, Dictionary<string, object> parameters)
        {
            var sql = $"INSERT INTO {tableName} ({string.Join(",", parameters.Keys)}) VALUES ({string.Join(",", parameters.Keys.Select(x => $"@{x}"))})";
            ExecuteNonQuery(sql, parameters);
        }

        public void UpdateTable(string tableName, Dictionary<string, object> parameters, string whereClause)
        {
            var setClause = string.Join(", ", parameters.Keys.Select(key => $"{key} = @{key}"));
            var sql = $"UPDATE {tableName} SET {setClause} WHERE {whereClause}";
            ExecuteNonQuery(sql, parameters);
        }

        private void ExecuteNonQuery(string sql, Dictionary<string, object> parameters)
        {
            ExecuteAction(() =>
            {
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand(sql, _conn))
                    {
                        AddParameters(cmd, parameters);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Log.LogMsg(ex.Message);

                    var sqlStr = sql + "WHERE " + string.Join(" AND ", parameters.Select(kvp => $"{kvp.Key} = {kvp.Value}")); ;
                    Log.LogMsg(sqlStr);
                }
            });
        }

        private void AddParameters(MySqlCommand command, Dictionary<string, object> parameters)
        {
            foreach (var p in parameters)
            {
                command.Parameters.AddWithValue(p.Key, p.Value);
            }
        }

        public void Dispose()
        {
            _conn?.Close();
            _conn?.Dispose();
            _conn = null;
            GC.SuppressFinalize(this);
        }
    }
}
