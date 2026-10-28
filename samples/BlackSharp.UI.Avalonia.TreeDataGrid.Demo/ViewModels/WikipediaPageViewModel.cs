using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Media;
using BlackSharp.UI.Avalonia.TreeDataGrid.Demo.Models;
using BlackSharp.UI.Avalonia.TreeDataGrid.Demo.Utils;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Demo.ViewModels
{
    internal class WikipediaPageViewModel : NotifyingBase, IDisposable
    {
        #region Constructor

        public WikipediaPageViewModel()
        {
            Source = new FlatTreeDataGridSource<OnThisDayArticle>(_data)
                .WithTemplateColumnFromResourceKeys("Image", "WikipediaImageCell")
                .WithTextColumn("Title", x => x.DisplayTitle)
                .WithTextColumn("Extract", x => x.Extract, x =>
                {
                    x.Width        = GridLength.Star;
                    x.TextTrimming = TextTrimming.None;
                    x.TextWrapping = TextWrapping.Wrap;
                });

            _ = Reload();
        }

        #endregion

        #region Fields

        private readonly AvaloniaList<OnThisDayArticle> _data     = [];

        private readonly CancellationTokenSource        _lifetime = new();

        private bool                                    _disposed;

        private Task?                                   _loadTask;

        private string                                  _status   = "Loading Wikipedia…";

        #endregion

        #region Properties

        public FlatTreeDataGridSource<OnThisDayArticle> Source { get; }

        public string Status
        {
            get => _status;
            private set => RaiseAndSetIfChanged(ref _status, value);
        }

        #endregion

        #region Public

        public Task Reload() => _disposed ? Task.CompletedTask : _loadTask is { IsCompleted: false } ? _loadTask : _loadTask = LoadContent();

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _lifetime.Cancel();
            Source.Dispose();

            foreach (var article in _data)
            {
                article.Dispose();
            }

            _data.Clear();
            _lifetime.Dispose();
        }

        #endregion

        #region Private

        private async Task LoadContent()
        {
            var cancellation = _lifetime.Token;

            try
            {
                Status           = "Loading Wikipedia…";
                using var client = new HttpClientEx();
                var d            = DateTimeOffset.Now.Day;
                var m            = DateTimeOffset.Now.Month;
                var uri          = $"https://api.wikimedia.org/feed/v1/wikipedia/en/onthisday/all/{m:00}/{d:00}";
                var json         = await client.GetStringAsync(uri, cancellation);
                var data         = JsonSerializer.Deserialize(json, OnThisDayJsonContext.Default.OnThisDay);

                if (data?.Selected is not null)
                {
                    cancellation.ThrowIfCancellationRequested();

                    foreach (var article in _data)
                    {
                        article.Dispose();
                    }

                    _data.Clear();
                    _data.AddRange(data.Selected.SelectMany(x => x.Pages ?? []));
                    Status = $"Loaded {_data.Count} articles.";
                }
                else
                {
                    Status = "Wikipedia returned no articles.";
                }
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            {
            }
            catch (Exception exception) when (exception is HttpRequestException or JsonException or OperationCanceledException)
            {
                Status = $"Wikipedia is unavailable: {exception.Message}";
            }
        }

        #endregion
    }

    [JsonSerializable(typeof(OnThisDay))]
    [JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
    internal partial class OnThisDayJsonContext : JsonSerializerContext
    {
    }
}
