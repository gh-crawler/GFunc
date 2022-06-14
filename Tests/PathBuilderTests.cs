using System;
using FluentAssertions;
using GFunc.Photos.Model;
using NUnit.Framework;

namespace Tests;

[TestFixture]
public class PathBuilderTests
{
    [Test]
    public void BuildPath()
    {
        // Arrange
        var testItem1 = new MediaItem("1", "http://test.url", "image/gif", "file1.gif", new MediaItemMeta(new DateTime(2022, 11, 20, 14, 44, 36)));
        var testItem2 = new MediaItem("2", "http://test.url", "image/jpeg", "file1.jpg", new MediaItemMeta(new DateTime(2022, 11, 20, 14, 44, 36)));

        // Act
        string path1 = testItem1.BuildPath();
        string path2 = testItem2.BuildPath();

        // Assert
        path1.Should().Be("2022/11/20221120144436.gif");
        path2.Should().Be("2022/11/20221120144436.jpg");
    }
}