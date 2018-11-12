
using System;

using System.ComponentModel;
using System.Configuration;
using System.Configuration.Install;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace DBSetup {
    [RunInstaller(true)]
    public partial class DBSetup : Installer
    {
        public DBSetup() {
            InitializeComponent();
        }

        public override void Install(System.Collections.IDictionary stateSaver)
        {
            try
            {

                string lsServer = this.Context.Parameters["server"];

                lsServer = lsServer.Replace(@"\\", @"\");

                string lsDatabase = this.Context.Parameters["database"];
                string lsUserId = this.Context.Parameters["userid"];
                string lsPassword = this.Context.Parameters["password"];

                // construct connection string
                string connectionString = ConstructConnectionString(lsServer, lsDatabase, lsUserId, lsPassword);

                //make sure connection is possible with connectionstring
                CheckForDB(connectionString);

                // set db connection
                SetConnectionString(connectionString);

                base.Install(stateSaver);
            }
            catch (System.Exception ex)
            {
                //if either method call crashes, it's specific message will be thrown to installer
                throw new InstallException(ex.Message);
            }
        }

        private void SetConnectionString(string connectionString)
        {
            Configuration Config1 = WebConfigurationManager.OpenWebConfiguration("/VSInstallerDemoApp");
            ConnectionStringsSection conSetting = (ConnectionStringsSection)Config1.GetSection("connectionStrings");
            conSetting.ConnectionStrings["ConnectionName"].ConnectionString = connectionString;
            Config1.Save();
        }

        private string ConstructConnectionString(string asServer, string asDatabase, string asUserId, string asPassword)
        {
            if (asServer.Length == 0 || asDatabase.Length == 0 || asUserId.Length == 0 || asPassword.Length == 0)
            {
                string lsMissingFields = "Could not connect to database" + System.Environment.NewLine + "The following fields are missing: ";

                if (asServer.Length == 0)
                    lsMissingFields += "Server, ";
                if (asDatabase.Length == 0)
                    lsMissingFields += "Database, ";
                if (asUserId.Length == 0)
                    lsMissingFields += "User ID, ";
                if (asPassword.Length == 0)
                    lsMissingFields += "Password, ";
                lsMissingFields = lsMissingFields.Trim();
                lsMissingFields = lsMissingFields.TrimEnd(',');

                //throw exception with message about missing fields
                throw new Exception(lsMissingFields);
            }

            string[] lsConnParameters = new string[4] { asServer, asDatabase, asUserId, asPassword };
            return string.Format("data source={0};initial catalog={1};User ID={2};Password={3};", lsConnParameters);
        }

        private void CheckForDB(string lsConnCheck)
        {
            SqlConnection lconnCheck = new SqlConnection();

            try
            {
                //set connection string
                lconnCheck.ConnectionString = lsConnCheck;
                //open connection, if it fails, exception will occur
                lconnCheck.Open();
                lconnCheck.Close();
            }
            catch (System.Exception ex)
            {
                //throw exception with message about bad connection
                throw new Exception("Could not connect to database" + System.Environment.NewLine + ex.Message);
            }
            finally
            {
                //close connection
                if (lconnCheck.State == ConnectionState.Open)
                    lconnCheck.Close();
            }
        }

        //private void AddRegistryValue(string asKey, string asValue) {
        //    try{
        //        RegistryAPI regProtect = new RegistryAPI("zServices");

        //        string lsValueEncrypted = ProtectAPI.Protect(asValue, System.Security.Cryptography.DataProtectionScope.LocalMachine);

        //        Configuration configuration = WebConfigurationManager.OpenWebConfiguration("/zServices");
        //        AppSettingsSection appSettings = (AppSettingsSection)configuration.GetSection("appSettings");

        //        if (appSettings != null) {
        //            if (appSettings.Settings[asKey] == null)
        //                appSettings.Settings.Add(asKey, lsValueEncrypted);
        //            else
        //                appSettings.Settings[asKey].Value = lsValueEncrypted;
        //            configuration.Save();
        //        }
        //    }
        //    catch (System.Exception ex) {
        //        throw new Exception("Could not add registry value for " + asKey + System.Environment.NewLine + ex.Message);
        //    }
        //    finally {
        //    }
        //}
    }
}