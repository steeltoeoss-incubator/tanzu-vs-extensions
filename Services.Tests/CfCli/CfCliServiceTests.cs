using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using Tanzu.Toolkit.VisualStudio.Services;
using Tanzu.Toolkit.VisualStudio.Services.CfCli;
using static Tanzu.Toolkit.VisualStudio.Services.OutputHandler.OutputHandler;

namespace Tanzu.Toolkit.VisualStudio.Services.Tests.CfCli
{
    [TestClass()]
    public class CfCliServiceTests : ServicesTestSupport
    {
        private CfCliService _sut;
        private string _fakeArguments = "fake args";
        private string _fakePathToCfExe = "this\\is\\a\\fake\\path";

        [TestInitialize]
        public void TestInit()
        {
            _sut = new CfCliService(services);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            mockFileLocatorService.VerifyAll();
            mockCmdProcessService.VerifyAll();
        }

        [TestMethod()]
        public async Task ExecuteCfCliCommandAsync_ReturnsTrueResult_WhenProcessSucceeded()
        {
            mockFileLocatorService.SetupGet(mock => mock.FullPathToCfExe).Returns(_fakePathToCfExe);

            mockCmdProcessService.Setup(mock => mock.InvokeWindowlessCommandAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<StdOutDelegate>()))
                .ReturnsAsync(true);

            DetailedResult result = await _sut.InvokeCfCliAsync(_fakeArguments, stdOutHandler: null);

            Assert.AreEqual(true, result.Succeeded);
        }

        [TestMethod()]
        public async Task ExecuteCfCliCommandAsync_ReturnsFalseResult_WhenProcessFails()
        {
            mockFileLocatorService.SetupGet(mock => mock.FullPathToCfExe).Returns(_fakePathToCfExe);

            mockCmdProcessService.Setup(mock => mock.InvokeWindowlessCommandAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<StdOutDelegate>()))
                .ReturnsAsync(false);

            DetailedResult result = await _sut.InvokeCfCliAsync(_fakeArguments, stdOutHandler: null);

            Assert.IsFalse(result.Succeeded);
        }

        [TestMethod()]
        public async Task ExecuteCfCliCommandAsync_ReturnsFalseResult_WhenCfExeCouldNotBeFound()
        {
            mockFileLocatorService.SetupGet(mock => mock.FullPathToCfExe).Returns((string)null);

            DetailedResult result = await _sut.InvokeCfCliAsync(_fakeArguments, stdOutHandler: null);

            Assert.IsFalse(result.Succeeded);
            Assert.IsTrue(result.Explanation.Contains("Unable to locate cf.exe"));
        }

        [TestMethod()]
        public async Task ExecuteCfCliCommandAsync_UsesDefaultDir_WhenNotSpecified()
        {
            mockFileLocatorService.SetupGet(mock => mock.FullPathToCfExe).Returns(_fakePathToCfExe);

            mockCmdProcessService.Setup(mock => mock.InvokeWindowlessCommandAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<StdOutDelegate>()))
                .ReturnsAsync(true);

            DetailedResult result = await _sut.InvokeCfCliAsync(_fakeArguments, stdOutHandler: null);

            string expectedCmdStr = $"\"{_fakePathToCfExe}\" {_fakeArguments}";
            string expectedWorkingDir = null;
            mockCmdProcessService.Verify(mock => mock.InvokeWindowlessCommandAsync(expectedCmdStr, expectedWorkingDir, null), Times.Once());
        }

        [TestMethod]
        public void GetOAuthToken_ReturnsNull_WhenStdErrWasCaptured()
        {
            Assert.Fail("TODO");
        }

        [TestMethod]
        public void GetOAuthToken_TrimsPrefix_WhenResultStartsWithBearer()
        {
            Assert.Fail("TODO");
        }

    }
}