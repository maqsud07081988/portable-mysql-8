using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySql.Data.MySqlClient;

namespace PortableMySQL8
{
    public static class SQLTools
    {
        /// <summary>
        /// Check for the --initialize string in a given MySQL start parameter string
        /// </summary>
        /// <param name="prams">MySQL start parameters</param>
        /// <returns>
        /// true if MySQL needs to be initialized, false if not
        /// </returns>
        public static bool NeedsInit(string prams)
        {
            return prams.Contains("--initialize");
        }

        /// <summary>
        /// Get the MySQL start paramters given a my.ini path and MySQL data path
        /// </summary>
        /// <param name="myIni">Path to my.ini (MUST be full path)</param>
        /// <param name="myDataPath">Path to MySQL data directory</param>
        /// <returns>
        /// MySQL start parameters string
        /// </returns>
        public static string GetStartParams(string myIni, string myDataPath)
        {
            string prams = "--defaults-file=" + "\"" + myIni + "\" --standalone --explicit_defaults_for_timestamp";

            //No MySQL data directory found, let's initialize it.
            //Doing an insecure initialization because we will set
            //a password for it immediately after.
            if (!Directory.Exists(myDataPath))
                prams += " --initialize-insecure";

            return prams;
        }

        /// <summary>
        /// Creates a new my.ini at the path specified
        /// </summary>
        /// <param name="myIni">Path to new my.ini</param>
        /// <param name="myBase">Base directory for MySQL install (MUST be full path)</param>
        /// <param name="myData">Data directory for MySQL database (MUST be full path)</param>
        /// <returns></returns>
        public static bool CreateNewMyIni(string myIni, string myBase, string myData)
        {
            try
            {
                //Set up default my.ini file contents
                //I'm sure there's probably a better way to do this but this is simple enough for now.
                List<string> contents = new List<string>()
                {
                    "[client]",
                    "",
                    "port=3306",
                    "",
                    "# The character set MySQL client will use",
                    "# MySQL defaults to utf8mb4 but OpenSim needs utf8mb3",
                    "default-character-set=utf8mb3",
                    "",
                    "[mysqld]",
                    "",
                    "# The TCP/IP Port the MySQL Server will listen on",
                    "port=3306",
                    "",
                    "# The character set MySQL client will use",
                    "# MySQL defaults to utf8mb4 but OpenSim needs utf8mb3",
                    "character-set-server=utf8mb3",
                    "",
                    "#Path to installation directory. All paths are usually resolved relative to this.",
                    "basedir=" + "\"" + myBase + "\"",
                    "",
                    "#Path to the database root",
                    "datadir=" + "\"" + myData + "\"",
                    "",
                    "#OpenSim needs this on MySQL 8.0.4+",
                    "default-authentication-plugin=mysql_native_password",
                    "",
                    "#Max packetlength to send/receive from to server.",
                    "#MySQL's default seems to be 1, 4, or 16 MB depending on version, but OpenSim needs more than that",
                    "max_allowed_packet=128M"
                };

                //Dump the new my.ini file to the proper location
                File.WriteAllLines(myIni, contents);

                return true;
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Updates my.ini configuration with values critical to running MySQL
        /// </summary>
        /// <param name="myIni">Path to my.ini config to update</param>
        /// <param name="port">Port to run MySQL on</param>
        /// <param name="myBase">Base directory for MySQL install (MUST be full path)</param>
        /// <param name="myData">Data directory for MySQL database (MUST be full path)</param>
        /// <returns>
        /// true if successful, false if not
        /// </returns>
        public static bool UpdateMyIni(string myIni, int port, string myBase, string myData)
        {
            try
            {
                Console.WriteLine($"Writing port config to {myIni}...");
                IniFile.WriteValue("client", "port", port.ToString(), myIni);
                IniFile.WriteValue("mysqld", "port", port.ToString(), myIni);

                Console.WriteLine($"Updating basedir and datadir in {myIni}...");
                IniFile.WriteValue("mysqld", "basedir", "\"" + myBase + "\"", myIni);
                IniFile.WriteValue("mysqld", "datadir", "\"" + myData + "\"", myIni);

                return true;
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Sets the password for a MySQL user
        /// </summary>
        /// <param name="user">The user to set password for</param>
        /// <param name="server">The server name to connect to</param>
        /// <param name="port">The port number to use</param>
        /// <param name="curPass">Current password</param>
        /// <param name="newPass">New password</param>
        /// <returns>
        /// true if successful, false if not
        /// </returns>
        public static bool SetUserPassword(string user, string server, int port, string curPass, string newPass)
        {
            bool success;
            string connectString = $"server={server};user={user};database=mysql;port={port};password={curPass}";
            MySqlConnection connection = new MySqlConnection(connectString);

            try
            {
                connection.Open();

                string sql = $"alter user '{user}'@'{server}' identified with mysql_native_password by '{newPass}'; flush privileges;";
                MySqlCommand myCmd = new MySqlCommand(sql, connection);

                int rows = myCmd.ExecuteNonQuery();
                myCmd.Dispose();

                //Console.WriteLine($"Password set to '{pass}', {rows} rows affected");
                Console.WriteLine("Password set sucessfully");

                success = true;
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                success = false;
            }

            connection.Close();
            connection.Dispose();

            return success;
        }

        /// <summary>
        /// Check if a given database exists by name
        /// </summary>
        /// <param name="user">The user to set password for</param>
        /// <param name="server">The server name to connect to</param>
        /// <param name="port">The port number to use</param>
        /// <param name="password">User's password</param>
        /// <param name="name">Name of database to check for</param>
        /// <returns>
        /// true if database exists, false if not, and null if there was an error checking it
        /// </returns>
        public static bool? DatabaseExists(string user, string server, int port, string password, string name)
        {
            bool? exists;
            string connectString = $"server={server};user={user};database=mysql;port={port};password={password}";
            MySqlConnection connection = new MySqlConnection(connectString);

            try
            {
                connection.Open();

                string sql = $"select count(schema_name) from information_schema.SCHEMATA where schema_name like '{name}';";
                //string sql = $"select schema_name from information_schema.SCHEMATA where schema_name like '{databaseName}';";

                MySqlCommand cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.Add(name, MySqlDbType.VarChar).Value = name;

                int numLikeName = Convert.ToInt32(cmd.ExecuteScalar());

                cmd.Dispose();

                exists = numLikeName > 0;
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                exists = null;
            }

            connection.Close();
            connection.Dispose();

            return exists;
        }

        /// <summary>
        /// Creates a database by name
        /// </summary>
        /// <param name="user">The user to set password for</param>
        /// <param name="server">The server name to connect to</param>
        /// <param name="port">The port number to use</param>
        /// <param name="password">User's password</param>
        /// <param name="name">Name of database to create</param>
        /// <returns>
        /// true if database creation successful, and false if not
        /// </returns>
        public static bool CreateDatabase(string user, string server, int port, string password, string name)
        {
            bool success;
            string connectString = $"server={server};user={user};database=mysql;port={port};password={password}";
            MySqlConnection connection = new MySqlConnection(connectString);

            try
            {
                connection.Open();

                string sql = $"create database if not exists `{name}`;";

                MySqlCommand cmd = new MySqlCommand(sql, connection);
                cmd.ExecuteNonQuery();
                cmd.Dispose();

                return true;
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                success = false;
            }

            connection.Close();
            connection.Dispose();

            return success;
        }

        /// <summary>
        /// Creates a database by name if it doesn't already exist
        /// </summary>
        /// <param name="user">The user to set password for</param>
        /// <param name="server">The server name to connect to</param>
        /// <param name="port">The port number to use</param>
        /// <param name="password">User's password</param>
        /// <param name="dbName">Name of database to create</param>
        /// <returns>
        /// true if database creation successful, and false if not
        /// </returns>
        public static bool CreateDatabaseIfNotExists(string user, string server, int port, string password, string dbName)
        {
            if (String.IsNullOrWhiteSpace(dbName))
                return false;

            bool? exists = DatabaseExists(user, server, port, password, dbName);

            if (exists != null && exists == false)
                return CreateDatabase(user, server, port, password, dbName);

            return false;
        }
    }
}
