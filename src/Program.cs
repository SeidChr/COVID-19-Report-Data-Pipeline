#nullable enable
namespace Corona
{
    using System.Threading.Tasks;

    public class Program
    {
        public static async Task Main()
        {
            await DailyReportPipeline.RunAsync();
        }
    }
}
