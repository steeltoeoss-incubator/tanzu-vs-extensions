using System.Threading.Tasks;
using Tanzu.Toolkit.VisualStudio.Services.CmdProcess;
using static Tanzu.Toolkit.VisualStudio.Services.OutputHandler.OutputHandler;

namespace Tanzu.Toolkit.VisualStudio.Services.CfCli
{
    public interface ICfCliService
    {
        Output ExecuteCfCliCommand(string arguments, string workingDir = null);
        Task<DetailedResult> InvokeCfCliAsync(string arguments, StdOutDelegate stdOutHandler = null, string workingDir = null);
    }
}