using Avalonia.Media.Imaging;
using BlackSharp.UI.Avalonia.TreeDataGrid.Demo.Utils;
using System.Text.Json.Serialization;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Demo.Models
{
    internal class OnThisDay
    {
        #region Properties

        public OnThisDayEvent[]? Selected { get; set; }

        #endregion
    }

    internal class OnThisDayEvent
    {
        #region Properties

        public string? Text { get; set; }

        public int Year { get; set; }

        public OnThisDayArticle[]? Pages { get; set; }

        #endregion
    }

    internal class OnThisDayArticle : IDisposable
    {
        #region Fields

        private Bitmap?                          _image;

        private Task<Bitmap?>?                   _imageTask;

        private readonly CancellationTokenSource _lifetime  = new();

        private bool                             _disposed;

        #endregion

        #region Properties

        public string? Type { get; set; }

        public OnThisDayTitles? Titles { get; set; }

        public OnThisDayImage? Thumbnail { get; set; }

        public string? Description { get; set; }

        public string? Extract { get; set; }

        [JsonIgnore]
        public Task<Bitmap?> Image => _disposed ? Task.FromResult<Bitmap?>(null) : _imageTask ??= LoadImageAsync();

        [JsonIgnore]
        public string? DisplayTitle => Titles?.Normalized;

        #endregion

        #region Public

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _lifetime.Cancel();
            _lifetime.Dispose();
            _image?.Dispose();
            _image     = null;
            _imageTask = null;
        }

        #endregion

        #region Private

        private async Task<Bitmap?> LoadImageAsync()
        {
            if (Thumbnail?.Source is not { } uri)
            {
                return null;
            }

            var cancellation = _lifetime.Token;

            try
            {
                using var client = new HttpClientEx();
                var bytes        = await client.GetByteArrayAsync(uri, cancellation);
                using var stream = new MemoryStream(bytes);
                var bitmap       = new Bitmap(stream);

                if (_disposed)
                {
                    bitmap.Dispose();

                    return null;
                }

                return _image = bitmap;
            }
            catch (Exception exception) when (exception is HttpRequestException or IOException or OperationCanceledException or ArgumentException)
            {
                return null;
            }
        }

        #endregion
    }

    internal class OnThisDayTitles
    {
        #region Properties

        public string? Normalized { get; set; }

        #endregion
    }

    internal class OnThisDayImage
    {
        #region Properties

        public string? Source { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        #endregion
    }
}
