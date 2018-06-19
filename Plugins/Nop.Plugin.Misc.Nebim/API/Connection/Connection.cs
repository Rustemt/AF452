using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using Nop.Plugin.Misc.NebimIntegration;
using Nop.Services.ExportImport;
namespace Nop.Plugin.Misc.NebimIntegration.API
{
    internal static class ConnectionManager
    {
        private static bool isConnected = false;
        public static bool IsConnected
        {
            get { return isConnected; }
        }

        public static void ConnectDB(NebimIntegrationSettings settings)
        {
            #region settings
            //UserGroup         : Dm
            //UserName          : Web
            //Password          : 1235
            //ServerName/IP     : XXX.XXX.XXX.XXX.
            //Database          : V3_Daiama Moda

            //Herry
            //User Group: hr_by
            //User Name: k150
            //Password: 1234
            //Server Name IP: 212.156.133.18
            //Database: Herry_v3
            #endregion settings

            if (IsConnected && NebimV3.ApplicationCommon.V3Application.Context.Globals.SqlConnector.SqlConnection.State == System.Data.ConnectionState.Open)
            { return; }
            if (IsConnected && NebimV3.ApplicationCommon.V3Application.Context.Globals.SqlConnector.SqlConnection.State != System.Data.ConnectionState.Open)
            {
                ResetConnection();
            }

            var userGroup = settings.UserGroup;
            var userName  = settings.UserName;
            var password  = settings.Password;
            var serverName = settings.ServerNameIP;
            var database = settings.Database;

            NebimV3.ApplicationCommon.V3Application.Context.Globals.Initialize(userGroup,userName,password,serverName);
            NebimV3.ApplicationCommon.V3Application.Context.Globals.CurrentUser.SelectCompany(database);
            NebimV3.ApplicationCommon.LocalizationHelper.Load();
            isConnected = true;
        }

        public static void ResetConnection()
        {
            NebimV3.ApplicationCommon.V3Application.Context.Globals.SqlConnector.Reset();
            isConnected = false;
        }

        //public static string GetVersion
        //{
        //    get
        //    {
        //        if (!IsConnected) ConnectDB();
        //        return NebimV3.ApplicationCommon.V3Application.Context.CurrentCompany.VersionInfo.Version;
        //    }
        //}

    }



}
