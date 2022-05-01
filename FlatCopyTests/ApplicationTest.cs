using FlatCopy;
using Xunit;

namespace FlatCopyTests;

public class ApplicationTest
{
    [Theory]
    [InlineData(@"C:\a\f.ext", @"D:\a_f.ext")]
    [InlineData(@"C:\a\b\f.ext", @"D:\a_b_f.ext")]
    public void CalculateTargetFile(string filePath, string expectedFileName)
    {
        string calculateTargetFile = Application.CalculateTargetFile(filePath, @"C:\a\", @"D:");
        Assert.Equal(expectedFileName, calculateTargetFile);
    }
}