using AF.Nop.Plugins.RssFeed.Services;
using Nop.Core.Domain;
using Nop.Core.Infrastructure;
using Nop.Services.Logging;
using Nop.Services.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace AF.Nop.Plugins.RssFeed
{
    public class RssFeedTask : ITask
    {
        private readonly ILogger _logger = EngineContext.Current.Resolve<ILogger>();
        private readonly RssFeedSetting _rssFeedSetting = EngineContext.Current.Resolve<RssFeedSetting>();

        protected void RunTaskByHttpRequest()
        {
            var _storeInformationSettings = EngineContext.Current.Resolve<StoreInformationSettings>();
            string url = _storeInformationSettings.StoreUrl + "RssFeed/ScheduleTask";
            //url = "http://localhost:56764/RssFeed/ScheduleTask";
            using (var wc = new WebClient())
            {
                var form = new System.Collections.Specialized.NameValueCollection() {
                        {"username",  RssFeedHelper.TASK_USERNAME},
                        {"password",  RssFeedHelper.TASK_PASSWORD},
                    };
                //wc.Credentials = new NetworkCredential(RssFeedHelper.TASK_USERNAME, RssFeedHelper.TASK_PASSWORD) ;
                //wc.DownloadString(url);
                wc.UploadValuesAsync(new Uri(url), form);
                //var b = wc.UploadValues(new Uri(url), form);
                //var s = Encoding.UTF8.GetString(b);
            }
        }

        public void Execute()
        {

            if (!_rssFeedSetting.EnabledSchedule)
                return;
            var diff = (DateTime.Now - _rssFeedSetting.LastRunTime) - TimeSpan.FromDays(1);
            if (diff.Days >= 0 && diff.Days < 1)
                return;

            var gap = TimeSpan.FromSeconds(_rssFeedSetting.TaskCheckTime);
            var t1 = DateTime.Now.TimeOfDay;
            var t2 = DateTime.Now.TimeOfDay+gap;
            var run = _rssFeedSetting.TaskRunTime.TimeOfDay;

            if (run < t1 || run > t2)
                return;

            try
            {

                _logger.Information("RssFeed: Start daily Schualed Task");

                RunTaskByHttpRequest();

                #region This shit did not work because new HttpContext() always throw an exception 
                //var _storeContext = EngineContext.Current.Resolve<StoreInformationSettings>();
                //HttpRequest request = new HttpRequest(null, _storeContext.StoreUrl, null);
                //request.Browser = new HttpBrowserCapabilities();
                //request.ContentEncoding = Encoding.UTF8;

                //HttpResponse response = new HttpResponse(new StringWriter(new StringBuilder()));
                //var physicalDir = AppDomain.CurrentDomain.BaseDirectory;
                //HttpWorkerRequest initWorkerRequest = new System.Web.Hosting.SimpleWorkerRequest(/*"~/", physicalDir,*/ "", "", new StringWriter(System.Globalization.CultureInfo.InvariantCulture));

                //var ctx = new System.Web.HttpContext(initWorkerRequest/*request, response*/);

                //var t = System.Threading.Tasks.Task.Factory.StartNew(() =>
                //{
                //    System.Web.HttpContext.Current = ctx;

                //    RunTaskByHttpRequest();

                //    var _rssFeedService = EngineContext.Current.Resolve<IRssFeedService>();
                //    _rssFeedService.GeneratRssFeedFiles();
                //    _rssFeedService.SaveUpdateTime(DateTime.Now);
                //});
                //t.Wait(); 
                #endregion


                //var _rssFeedService = EngineContext.Current.Resolve<IRssFeedService>();
                //_rssFeedService.GeneratRssFeedFiles();
                //_rssFeedService.SaveUpdateTime(DateTime.Now);

                //_logger.Information("RssFeed: Finish daily Schualed Task");
            }
            catch (Exception ex)
            {
                _logger.Information("RssFeed: Error occured during the schualed task", ex);
            }
        }
    }
}
