using System.IO;
using Emby.Server.Implementations.Library.Resolvers.Books;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.Tests.Library
{
    public class BookResolverTests
    {
        private readonly BookResolver _bookResolver;

        public BookResolverTests()
        {
            _bookResolver = new BookResolver();
        }

        [Theory]
        [InlineData("book.azw")]
        [InlineData("book.azw3")]
        [InlineData("book.cb7")]
        [InlineData("book.cbr")]
        [InlineData("book.cbt")]
        [InlineData("book.cbz")]
        [InlineData("book.epub")]
        [InlineData("book.mobi")]
        [InlineData("book.pdf")]
        [InlineData("book.zip")]
        public void Resolve_ValidBookFile_ReturnsBook(string filename)
        {
            // Arrange
            var args = CreateItemResolveArgs(filename, false);

            // Act
            var result = _bookResolver.Resolve(args);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Book>(result);
            Assert.Equal(args.Path, result.Path);
            Assert.True(result.IsInMixedFolder);
        }

        [Theory]
        [InlineData("notabook.txt")]
        [InlineData("video.mp4")]
        [InlineData("audio.mp3")]
        [InlineData("image.jpg")]
        public void Resolve_InvalidBookFile_ReturnsNull(string filename)
        {
            // Arrange
            var args = CreateItemResolveArgs(filename, false);

            // Act
            var result = _bookResolver.Resolve(args);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Resolve_NotInBooksCollection_ReturnsNull()
        {
            // Arrange
            var args = CreateItemResolveArgs("book.pdf", false);
            args.CollectionType = CollectionType.movies; // Not books

            // Act
            var result = _bookResolver.Resolve(args);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Resolve_DirectoryWithSingleBookFile_ReturnsBook()
        {
            // Arrange
            var bookFile = CreateFileSystemMetadata("book.zip", false);
            var nonBookFile = CreateFileSystemMetadata("readme.txt", false);
            var args = CreateItemResolveArgs("bookfolder", true);
            args.FileSystemChildren = new[] { bookFile, nonBookFile };

            // Act
            var result = _bookResolver.Resolve(args);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Book>(result);
            Assert.Equal(bookFile.FullName, result.Path);
            Assert.False(result.IsInMixedFolder);
        }

        [Fact]
        public void Resolve_DirectoryWithMultipleBookFiles_ReturnsNull()
        {
            // Arrange
            var bookFile1 = CreateFileSystemMetadata("book1.zip", false);
            var bookFile2 = CreateFileSystemMetadata("book2.epub", false);
            var args = CreateItemResolveArgs("bookfolder", true);
            args.FileSystemChildren = new[] { bookFile1, bookFile2 };

            // Act
            var result = _bookResolver.Resolve(args);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Resolve_DirectoryWithNoBookFiles_ReturnsNull()
        {
            // Arrange
            var textFile = CreateFileSystemMetadata("readme.txt", false);
            var imageFile = CreateFileSystemMetadata("cover.jpg", false);
            var args = CreateItemResolveArgs("folder", true);
            args.FileSystemChildren = new[] { textFile, imageFile };

            // Act
            var result = _bookResolver.Resolve(args);

            // Assert
            Assert.Null(result);
        }

        private ItemResolveArgs CreateItemResolveArgs(string path, bool isDirectory)
        {
            var fullPath = Path.Combine("/books", path);
            var args = new ItemResolveArgs(
                Mock.Of<IServerApplicationPaths>(),
                null)
            {
                FileInfo = new FileSystemMetadata
                {
                    FullName = fullPath,
                    Name = path,
                    IsDirectory = isDirectory
                },
                CollectionType = CollectionType.books,
                FileSystemChildren = new FileSystemMetadata[0]
            };

            return args;
        }

        private FileSystemMetadata CreateFileSystemMetadata(string name, bool isDirectory)
        {
            var fullPath = Path.Combine("/books", name);
            return new FileSystemMetadata
            {
                FullName = fullPath,
                Name = name,
                IsDirectory = isDirectory
            };
        }
    }
}
