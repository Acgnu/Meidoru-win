using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace AcgnuX.Source.Utils
{
    /// <summary>
    /// SQLite查询类
    /// </summary>
    class SQLite
    {
        public readonly static string dbPath = @"Assets/data/";
        public readonly static string dbfile = "master.db";
        public readonly static string initfile = "init.sql";

        /// <summary>
        /// 创建数据库文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        public static void CreateDBFile(string fileName)
        {
            //string path = Environment.CurrentDirectory + dbPath;
            if (!Directory.Exists(dbPath))
            {
                Directory.CreateDirectory(dbPath);
            }
            var databaseFileName = dbPath + fileName;
            if (!File.Exists(databaseFileName))
            {
                SQLiteConnection.CreateFile(databaseFileName);
            }
        }
        /// <summary>
        /// 删除数据库
        /// </summary>
        /// <param name="fileName">文件名</param>
        public static void DeleteDBFile(string fileName)
        {
            var path = Environment.CurrentDirectory + dbPath;
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        /// <summary>
        /// 连接到数据库
        /// </summary>
        /// <returns></returns>
        private static SQLiteConnection GetConnection()
        {
            var connectionString = new SQLiteConnectionStringBuilder
            {
                DataSource = string.Format("{0}{1}", dbPath, dbfile)
            };
            var connection = new SQLiteConnection(connectionString.ToString());
            connection.Open();
            return connection;
        }

        /// <summary>
        /// 在指定数据库中创建一个table
        /// </summary>
        /// <param name="sql">sql语言，如：create table highscores (name varchar(20), score int)</param>
        /// <returns></returns>
        public static bool CreateTable(string sql)
        {
            var connection = GetConnection();
            try
            {
                var command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ExecuteNonQuery(" + sql + ")Err:" + ex);
                return false;
            }
            finally
            {
                CloseConnection(connection);
            }
        }

        /// <summary>
        /// 在指定数据库中删除一个table
        /// </summary>
        /// <param name="tablename">表名称</param>
        /// <returns></returns>
        public static bool DeleteTable(string tablename)
        {
            var connection = GetConnection();
            try
            {
                var cmd = new SQLiteCommand("DROP TABLE IF EXISTS " + tablename, connection);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ExecuteNonQuery(DROP TABLE IF EXISTS " + tablename + ")Err:" + ex);
                return false;
            }
            finally
            {
                CloseConnection(connection);
            }
        }

        /// <summary>
        /// 在指定表中添加列
        /// </summary>
        /// <param name="tablename">表名</param>
        /// <param name="columnname">列名</param>
        /// <param name="ctype">列的数值类型</param>
        /// <returns></returns>
        public static bool AddColumn(string tablename, string columnname, string ctype)
        {
            var connection = GetConnection();
            try
            {
                var cmd = new SQLiteCommand("ALTER TABLE " + tablename + " ADD COLUMN " + columnname + " " + ctype, connection);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ExecuteNonQuery(ALTER TABLE " + tablename + " ADD COLUMN " + columnname + " " + ctype + ")Err:" + ex);
                return false;
            }
            finally
            {
                CloseConnection(connection);
            }
        }

        /// <summary>
        /// 执行增删改查操作
        /// </summary>
        /// <param name="sql">查询语言</param>
        /// <returns></returns>
        public static int ExecuteNonQuery(string sql)
        {
            var connection = GetConnection();
            try
            {
                var cmd = new SQLiteCommand(sql, connection);
                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ExecuteNonQuery(" + sql + ")Err:" + ex);
                return 0;
            }
            finally
            {
                CloseConnection(connection);
            }
        }

        /// <summary>
        /// 返回一条记录查询
        /// </summary>
        /// <param name="sql">sql查询语言</param>
        /// <returns>返回字符串数组</returns>
        public static string[] SqlRow(string sql)
        {
            var connection = GetConnection();
            try
            {
                var sqlcmd = new SQLiteCommand(sql, connection);//sql语句
                var reader = sqlcmd.ExecuteReader();
                if (!reader.Read())
                {
                    return null;
                }
                string[] Row = new string[reader.FieldCount];
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    Row[i] = (reader[i].ToString());
                }
                reader.Close();
                return Row;
            }
            catch (Exception ex)
            {
                Console.WriteLine("SqlRow(" + sql + ")Err:" + ex);
                return null;
            }
            finally
            {
                CloseConnection(connection);
            }
        }

        /// <summary>
        /// 唯一结果查询
        /// </summary>
        /// <param name="sql">sql查询语言</param>
        /// <returns>返回一个字符串</returns>
        public static string sqlone(string sql)
        {
            var connection = GetConnection();
            try
            {
                var sqlcmd = new SQLiteCommand(sql, connection);//sql语句
                var obj = sqlcmd.ExecuteScalar();
                if(null == obj)
                {
                    return string.Empty;
                }
                return obj.ToString();
            }
            catch
            {
                return "";
            }
            finally
            {
                CloseConnection(connection);
            }
        }

        /// <summary>
        /// 获取一列数据
        /// </summary>
        /// <param name="sql">单列查询</param>
        /// <param name="count">返回结果数量</param>
        /// <returns>返回一个数组</returns>
        public static List<string> sqlcolumn(string sql)
        {
            var connection = GetConnection();
            try
            {
                var columnList = new List<string>();
                var sqlcmd = new SQLiteCommand(sql, connection);//sql语句
                var reader = sqlcmd.ExecuteReader();
                while (reader.Read())
                {
                    columnList.Add(reader[0].ToString());
                }
                reader.Close();
                return columnList;
            }
            catch (Exception ex)
            {
                Console.WriteLine("sqlcolumn(" + sql + ")Err:" + ex);
                return null;
            }
            finally
            {
                CloseConnection(connection);
            }
        }

        /// <summary>
        /// 返回记录集查询
        /// </summary>
        /// <param name="sql">sql查询语言</param>
        /// <returns>返回查询结果集</returns>
        public static DataTable SqlTable(string sql)
        {
            var connection = GetConnection();
            try
            {
                var sqlcmd = new SQLiteCommand()
                {
                    CommandText = sql,
                    Connection = connection,
                    CommandTimeout = 120
                };
                var reader = sqlcmd.ExecuteReader();
                var dt = new DataTable();
                if (reader != null)
                {
                    dt.Load(reader, LoadOption.PreserveChanges, null);
                }
                return dt;
            }
            catch (Exception ex)
            {
                Console.WriteLine("SqlReader(" + sql + ")Err:" + ex);
                return null;
            }
            finally
            {
                CloseConnection(connection);
            }
        }
        /// <summary>
        /// 关闭数据库连接
        /// </summary>
        public static void CloseConnection(SQLiteConnection connection)
        {
            try
            {
                if (connection.State == ConnectionState.Open || connection.State == ConnectionState.Broken)
                {
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("closeConnErr:" + ex);
            }
        }
    }
}
