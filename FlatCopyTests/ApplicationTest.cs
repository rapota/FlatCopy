using FlatCopy;
using FlatCopy.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlatCopyTests;

public class ApplicationTest
{
    private readonly Mock<IOptions<CopyOptions>> _copyOptionsMock;
    private readonly Mock<IFlatCopyService> _flatCopyMock;
    private readonly Application _application;

    public ApplicationTest()
    {
        _copyOptionsMock = new Mock<IOptions<CopyOptions>>();
        _flatCopyMock = new Mock<IFlatCopyService>();
        _application = new Application(_copyOptionsMock.Object, _flatCopyMock.Object, Mock.Of<ILogger<Application>>());
    }

    [Fact]
    public void Test()
    {
        CopyOptions copyOptions = new()
        {
            TargetFolder = @"C:\out",
            SourceFolders = [@"C:\inp1", @"C:\inp2"],
            Sources =
            {
                {"Name", new CopySource
                {
                    SourceFolder = @"C:\inp11"
                }}
            }

        };

        _copyOptionsMock.Setup(x => x.Value).Returns(copyOptions);

        _flatCopyMock.Setup(x => x.FlatCopy(It.IsAny<FlatCopyParams>())).Returns(["result"]);

        _application.Run();
    }
}