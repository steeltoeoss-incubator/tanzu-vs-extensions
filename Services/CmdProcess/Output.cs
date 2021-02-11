namespace Tanzu.Toolkit.VisualStudio.Services.CmdProcess
{
    public class Output
    {
        public string StdOut { get; }
        public string StdErr { get; }

        public Output(string stdOut, string stdErr)
        {
            StdOut = stdOut;
            StdErr = stdErr;
        }
    }
}
