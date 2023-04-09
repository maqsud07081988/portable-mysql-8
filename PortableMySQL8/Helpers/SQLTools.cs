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
        /// <param name="myIni">Path to my.ini</param>
        /// <param name="myDataPath">Path to MySQL data directory</param>
        /// <returns>
        /// MySQL start parameters string
        /// </returns>
        public static string GetStartParams(string myIni, string myDataPath)
        {
            string prams = "--defaults-file=" + "\"" + Path.Combine(Environment.CurrentDirectory, myIni) + "\" --standalone --explicit_defaults_for_timestamp";

            //No MySQL data directory found, let's initialize it.
            //Doing an insecure initialization because we will set
            //a password for it immediately after.
            if (!Directory.Exists(myDataPath))
                prams += " --initialize-insecure";

            return prams;
        }

        /// <summary>
        /// Updates my.ini configuration with values critical to running MySQL
        /// </summary>
        /// <param name="myIni">Path to my.ini config to update</param>
        /// <param name="port">Port to run MySQL on</param>
        /// <param name="myBase">Base directory for MySQL install</param>
        /// <param name="myData">Data directory for MySQL database</param>
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
                IniFile.WriteValue("mysqld", "basedir", "\"" + Path.GetFullPath(myBase) + "\"", myIni);
                IniFile.WriteValue("mysqld", "datadir", "\"" + Path.GetFullPath(myData) + "\"", myIni);

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
        public static bool SetUserPassword(string user, string server, string port, string curPass, string newPass)
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

                //Console.WriteLine($"Password set to '{pass}', {rows} rows affected");
                Console.Write("Password set sucessfully");

                success = true;
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                success = false;
            }

            connection.Close();
            return success;
        }
    }
}
