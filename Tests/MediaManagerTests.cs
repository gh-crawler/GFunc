using System;
using System.Threading.Tasks;
using GFunc.Photos;
using GFunc.Photos.Model;
using Moq;
using NUnit.Framework;

namespace Tests;

[TestFixture]
public class MediaManagerTests
{
    private const string AlbumId = "testAlbum";
    
    [Test]
    public async Task NoItems_CallsProviderOnly()
    {
        // Arrange
        var action = new Mock<IMediaAction>();
        var provider = new Mock<IMediaProvider>();

        provider.Setup(x => x.GetMediaAsync(It.IsAny<string>())).ReturnsAsync(Array.Empty<MediaItem>());
        
        var manager = new MediaManager(AlbumId, provider.Object, action.Object);

        // Act
        await manager.InvokeAsync();

        // Assert
        provider.Verify(x => x.GetMediaAsync(AlbumId), Times.Once);
        action.Verify(x => x.InvokeAsync(It.IsAny<MediaItem>()), Times.Never);
    }

    [Test]
    public async Task OneItem_CallsAction()
    {
        // Arrange
        var action = new Mock<IMediaAction>();
        var provider = new Mock<IMediaProvider>();

        var testItem = new MediaItem("1", "http://test.url", "image/gif", "file1.gif", new MediaItemMeta(DateTime.UtcNow.AddHours(1)));
        provider.Setup(x => x.GetMediaAsync(AlbumId)).ReturnsAsync(new[] {testItem});
        
        var manager = new MediaManager(AlbumId, provider.Object, action.Object);

        // Act
        await manager.InvokeAsync();

        // Assert
        action.Verify(x => x.InvokeAsync(testItem), Times.Once);
    }

    [Test]
    public async Task OneItemMultipleTimes_CallsActionOnce()
    {
        // Arrange
        var action = new Mock<IMediaAction>();
        var provider = new Mock<IMediaProvider>();

        var testItem = new MediaItem("1", "http://test.url", "image/gif", "file1.gif", new MediaItemMeta(DateTime.UtcNow.AddHours(1)));
        provider.Setup(x => x.GetMediaAsync(AlbumId)).ReturnsAsync(new[] {testItem});
        
        var manager = new MediaManager(AlbumId, provider.Object, action.Object);

        // Act
        await manager.InvokeAsync();
        await manager.InvokeAsync();

        // Assert
        provider.Verify(x => x.GetMediaAsync(AlbumId), Times.Exactly(2));
        action.Verify(x => x.InvokeAsync(testItem), Times.Once);
    }
    
    [Test]
    public async Task MultipleItems_SkipsOldAndHandledItems()
    {
        // Arrange
        var action = new Mock<IMediaAction>();
        var provider = new Mock<IMediaProvider>();

        var testItem1 = new MediaItem("1", "http://test.url", "image/gif", "file1.gif", new MediaItemMeta(DateTime.UtcNow.AddHours(-1)));
        var testItem2 = new MediaItem("2", "http://test.url", "image/gif", "file2.gif", new MediaItemMeta(DateTime.UtcNow.AddHours(1)));
        var testItem3 = new MediaItem("3", "http://test.url", "image/gif", "file3.gif", new MediaItemMeta(DateTime.UtcNow.AddHours(1)));
        
        provider.Setup(x => x.GetMediaAsync(AlbumId)).ReturnsAsync(new[] {testItem1, testItem2, testItem3});
        
        var manager = new MediaManager(AlbumId, provider.Object, action.Object);

        // Act
        await manager.InvokeAsync();
        await manager.InvokeAsync();

        // Assert
        provider.Verify(x => x.GetMediaAsync(AlbumId), Times.Exactly(2));
        action.Verify(x => x.InvokeAsync(testItem1), Times.Never);
        action.Verify(x => x.InvokeAsync(testItem2), Times.Once);
        action.Verify(x => x.InvokeAsync(testItem3), Times.Once);
    }
}