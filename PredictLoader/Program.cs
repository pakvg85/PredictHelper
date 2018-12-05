using NLog;

namespace PredictLoader
{
    public class Program
    {
        public static Logger GlobalLogger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            var model = new PredictHelper.Models.MainModel(
                    System.Configuration.ConfigurationManager.AppSettings["FileNameConnectionPredicates"],
                    System.Configuration.ConfigurationManager.AppSettings["FileNameConnectionContentTypes"]);
            model.EventMessageOccured += Model_EventMessageOccured;

            model.DbLoad();
            model.ProcessPredicates();
            //model.DbSave();
        }

        private static void Model_EventMessageOccured(object sender, PredictHelper.Common.MessageOccuredEventArgs e)
        {
            if (e.Ex != null)
                GlobalLogger.Error(e.Ex);
            else
                GlobalLogger.Info(e.Message);
        }
    }
}