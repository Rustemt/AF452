using System;
using Nop.Core.Configuration;
using Nop.Core.Domain.Directory;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Nop.Services.Tasks;
using Nop.Services.Logging;

namespace Nop.Services.ExportImport
{

    public partial class NebimIntegrationTask : ITask
    {
        /// <summary>
        /// Executes a task
        /// </summary>
        public void Execute()
        {

            //IImportManager importManager = EngineContext.Current.Resolve<IImportManager>();
            //IExportManager exportManager = EngineContext.Current.Resolve<IExportManager>();
            INebimIntegrationImportService nebimIntegrationImportService = EngineContext.Current.Resolve<INebimIntegrationImportService>();
            var logger = EngineContext.Current.Resolve<ILogger>();
            var settingService = EngineContext.Current.Resolve<ISettingService>();
            var nebimIntegrationSettings = EngineContext.Current.Resolve<NebimIntegrationSettings>();
            
            //all products sync, task interval is 5 minutes!
            //products sync will be executed between 4:00-4:09 am(UTC)
            #region Products Sync
            //check product sync enabled
            if (!nebimIntegrationSettings.ProductsSyncEnabled)
                return;
            int intervalMinutes = 20;
            if (DateTime.UtcNow.TimeOfDay < new TimeSpan(0, nebimIntegrationSettings.ProductsSyncStartTimeMinutes, 0))
                return;
            if (DateTime.UtcNow.TimeOfDay > new TimeSpan(0, nebimIntegrationSettings.ProductsSyncStartTimeMinutes + intervalMinutes, 0))
                return;
            //ensure previous executaion is 1 hour ago!
            DateTime lastUpdateTime = DateTime.FromBinary(nebimIntegrationSettings.LastProductsSyncTime);
            lastUpdateTime = DateTime.SpecifyKind(lastUpdateTime, DateTimeKind.Utc);
            if (lastUpdateTime.AddHours(1) < DateTime.UtcNow)
            {
                logger.Information("Task:Product sync started");
                //do products sync
                nebimIntegrationImportService.ImportAllProducts();

                //save new update time value
                nebimIntegrationSettings.LastProductsSyncTime = DateTime.UtcNow.ToBinary();
                settingService.SaveSetting(nebimIntegrationSettings);
                logger.Information("Task:Product sync finished");
            }


            #endregion Products Sync


            #region Orders Sync

            //logger.Information("Task:Orders sync started");
            //nebimIntegrationService.AddUpdateOrdersToNebim();
            //logger.Information("Task:Orders sync finished");

            #endregion Orders Sync

        }

    }
}
