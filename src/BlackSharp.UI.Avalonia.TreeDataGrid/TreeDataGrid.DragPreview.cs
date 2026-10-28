using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Logging;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using BlackSharp.UI.Avalonia.TreeDataGrid.Primitives;

namespace BlackSharp.UI.Avalonia.TreeDataGrid
{
    public partial class TreeDataGrid
    {
        #region Fields

        public static readonly StyledProperty<IDataTemplate?> DragPreviewTemplateProperty =
            AvaloniaProperty.Register<TreeDataGrid, IDataTemplate?>(nameof(DragPreviewTemplate));

        public static readonly StyledProperty<double>         DragPreviewOpacityProperty  =
            AvaloniaProperty.Register<TreeDataGrid, double>(nameof(DragPreviewOpacity), 0.88,
                validate: value => double.IsFinite(value) && value >= 0 && value <= 1);

        public static readonly StyledProperty<bool>           ShowDragPreviewProperty     =
            AvaloniaProperty.Register<TreeDataGrid, bool>(nameof(ShowDragPreview), true);

        private const double RowDragPreviewOffset = 12;

        private Border?              _rowDragPreview;

        private RenderTargetBitmap?  _rowDragPreviewBitmap;

        private TopLevel?            _rowDragPreviewTopLevel;

        private DispatcherOperation? _rowDragPreviewHide;

        private bool                 _rowDragActive;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the template for the dragged row's preview. Its data context is the original row model.
        /// When null, the preview is a snapshot of the visible columns of the row that initiated the drag.
        /// </summary>
        public IDataTemplate? DragPreviewTemplate
        {
            get => GetValue(DragPreviewTemplateProperty);
            set => SetValue(DragPreviewTemplateProperty, value);
        }

        /// <summary>
        /// Gets or sets the preview opacity, between zero and one. The default is 0.88.
        /// </summary>
        public double DragPreviewOpacity
        {
            get => GetValue(DragPreviewOpacityProperty);
            set => SetValue(DragPreviewOpacityProperty, value);
        }

        /// <summary>
        /// Gets or sets whether row dragging shows a preview in the source window's overlay layer.
        /// The default is true. This does not enable or disable row dragging itself.
        /// </summary>
        public bool ShowDragPreview
        {
            get => GetValue(ShowDragPreviewProperty);
            set => SetValue(ShowDragPreviewProperty, value);
        }

        #endregion

        #region Protected

        /// <summary>
        /// Starts the platform drag operation. The grid owns preview creation and cleanup around this operation.
        /// </summary>
        protected virtual Task<DragDropEffects> DoRowDragDropAsync(
            PointerPressedEventArgs trigger, IDataTransfer dataTransfer, DragDropEffects allowedEffects) =>
            DragDrop.DoDragDropAsync(trigger, dataTransfer, allowedEffects);

        #endregion

        #region Private

        private async Task DragRowsAsync(PointerPressedEventArgs trigger, Point position,
            TreeDataGridRow? row, IDataTransfer dataTransfer, DragDropEffects allowedEffects)
        {
            _rowDragActive = true;

            try
            {
                TryShowRowDragPreview(row, position);
                await DoRowDragDropAsync(trigger, dataTransfer, allowedEffects);
            }
            catch (OperationCanceledException)
            {
                // Cancellation is a normal completion path for a native drag operation.
            }
            catch (Exception exception)
            {
                Logger.TryGet(LogEventLevel.Error, LogArea.Control)?.Log(this,
                    "Row drag-and-drop failed: {Exception}", exception);
            }
            finally
            {
                StopDrag();
                _hideDragAdornerTimer?.Dispose();
                _hideDragAdornerTimer = null;
                RemoveDragAdorner();
                RemoveRowDragPreview();
                _rowDragActive = false;
            }
        }

        private void TryShowRowDragPreview(TreeDataGridRow? row, Point position)
        {
            if (!ShowDragPreview || row?.Model is not { } model ||
                OverlayLayer.GetOverlayLayer(this) is not { } layer || TopLevel.GetTopLevel(this) is not { } topLevel)
            {
                return;
            }

            try
            {
                Control content;

                if (DragPreviewTemplate is { } template)
                {
                    content = new ContentControl
                    {
                        Content         = model,
                        ContentTemplate = template,
                    };
                }
                else
                {
                    _rowDragPreviewBitmap = CreateRowDragSnapshot(row, topLevel.RenderScaling);
                    content = new Image
                    {
                        Source  = _rowDragPreviewBitmap,
                        Width   = _rowDragPreviewBitmap.PixelSize.Width / topLevel.RenderScaling,
                        Height  = _rowDragPreviewBitmap.PixelSize.Height / topLevel.RenderScaling,
                        Stretch = Stretch.Fill,
                    };
                }

                _rowDragPreview = new Border
                {
                    Background       = Background ?? topLevel.Background,
                    BorderBrush      = Foreground,
                    BorderThickness  = new Thickness(1),
                    Child            = content,
                    ClipToBounds     = true,
                    DataContext      = model,
                    IsHitTestVisible = false,
                    MaxHeight        = Math.Max(1, topLevel.Bounds.Height),
                    MaxWidth         = Math.Max(1, topLevel.Bounds.Width),
                    Opacity          = DragPreviewOpacity,
                };
                TextElement.SetFontFamily(_rowDragPreview, FontFamily);
                TextElement.SetFontSize(_rowDragPreview, FontSize);
                TextElement.SetFontStyle(_rowDragPreview, FontStyle);
                TextElement.SetFontWeight(_rowDragPreview, FontWeight);
                TextElement.SetFontStretch(_rowDragPreview, FontStretch);
                TextElement.SetForeground(_rowDragPreview, Foreground);
                _rowDragPreview.Classes.Add("tree-data-grid-row-drag-preview");
                layer.Children.Add(_rowDragPreview);
                _rowDragPreview.Measure(topLevel.Bounds.Size);
                _rowDragPreviewTopLevel = topLevel;
                topLevel.AddHandler(DragDrop.DragEnterEvent, OnRowDragPreviewOver, RoutingStrategies.Bubble, handledEventsToo: true);
                topLevel.AddHandler(DragDrop.DragOverEvent, OnRowDragPreviewOver, RoutingStrategies.Bubble, handledEventsToo: true);
                topLevel.AddHandler(DragDrop.DragLeaveEvent, OnRowDragPreviewLeave, RoutingStrategies.Bubble, handledEventsToo: true);
                topLevel.AddHandler(DragDrop.DropEvent, OnRowDragPreviewDrop, RoutingStrategies.Bubble, handledEventsToo: true);

                if (this.TranslatePoint(position, layer) is { } translated)
                {
                    PositionRowDragPreview(translated);
                }
            }
            catch (Exception exception)
            {
                // A missing render backend or a broken user template must not disable the actual drag operation.
                RemoveRowDragPreview();
                Logger.TryGet(LogEventLevel.Warning, LogArea.Control)?.Log(this,
                    "Could not create row drag preview: {Exception}", exception);
            }
        }

        private RenderTargetBitmap CreateRowDragSnapshot(TreeDataGridRow row, double scale)
        {
            var area = new Rect(row.Bounds.Size);

            if (row.FindAncestorOfType<ScrollContentPresenter>() is { } viewport &&
                viewport.TranslatePoint(default, row) is { } origin)
            {
                area = area.Intersect(new Rect(origin.X, 0, viewport.Bounds.Width, row.Bounds.Height));
            }

            if (area.Width <= 0 || area.Height <= 0 || !double.IsFinite(area.Width) || !double.IsFinite(area.Height))
            {
                throw new InvalidOperationException("The dragged row has no visible bounds.");
            }

            var pixels = PixelSize.FromSize(area.Size, scale);
            var capture = new Border
            {
                Width                 = area.Width,
                Height                = area.Height,
                HorizontalAlignment   = HorizontalAlignment.Left,
                VerticalAlignment     = VerticalAlignment.Top,
                RenderTransform       = new ScaleTransform(scale, scale),
                RenderTransformOrigin = RelativePoint.TopLeft,
                Background            = new VisualBrush(row)
                {
                    SourceRect      = new RelativeRect(area, RelativeUnit.Absolute),
                    DestinationRect = new RelativeRect(area.Size, RelativeUnit.Absolute),
                    Stretch         = Stretch.Fill,
                },
            };
            // Store physical pixels at 96 DPI so Image draws the whole bitmap without a second DPI crop.
            var host = new Panel { Children = { capture } };
            var size = pixels.ToSize(1);

            host.Measure(size);
            host.Arrange(new Rect(size));
            var bitmap = new RenderTargetBitmap(pixels, new Vector(96, 96));

            try
            {
                bitmap.Render(host);

                return bitmap;
            }
            catch
            {
                bitmap.Dispose();
                throw;
            }
            finally
            {
                capture.Background = null;
                host.Children.Clear();
            }
        }

        private void OnRowDragPreviewOver(object? sender, DragEventArgs e)
        {
            if (_rowDragPreview?.Parent is OverlayLayer layer)
            {
                _rowDragPreviewHide?.Abort();
                _rowDragPreviewHide = null;
                _rowDragPreview.IsVisible = true;
                PositionRowDragPreview(e.GetPosition(layer));
            }
        }

        private void OnRowDragPreviewDrop(object? sender, DragEventArgs e)
        {
            _rowDragPreviewHide?.Abort();
            _rowDragPreviewHide = null;

            if (_rowDragPreview is { } preview)
            {
                preview.IsVisible = false;
            }
        }

        private void OnRowDragPreviewLeave(object? sender, DragEventArgs e)
        {
            _rowDragPreviewHide?.Abort();

            // Avalonia raises Leave followed by Enter when the drop target changes inside the same window.
            // Let that Enter cancel the hide before rendering another frame.
            _rowDragPreviewHide = Dispatcher.UIThread.InvokeAsync(() =>
            {
                _rowDragPreviewHide = null;

                if (_rowDragPreview is { } preview)
                {
                    preview.IsVisible = false;
                }
            }, DispatcherPriority.Input);
        }

        private void PositionRowDragPreview(Point position)
        {
            if (_rowDragPreview is { Parent: OverlayLayer layer } preview)
            {
                var size   = preview.DesiredSize;
                var bounds = _rowDragPreviewTopLevel?.Bounds.Size ?? layer.Bounds.Size;

                // Full-width row snapshots may extend beyond the window; fitting the whole image pins its X coordinate.
                Canvas.SetLeft(preview, position.X + RowDragPreviewOffset);
                Canvas.SetTop(preview, Math.Clamp(position.Y + RowDragPreviewOffset, 0, Math.Max(0, bounds.Height - size.Height)));
            }
        }

        private void RemoveRowDragPreview()
        {
            _rowDragPreviewHide?.Abort();
            _rowDragPreviewHide = null;

            if (_rowDragPreviewTopLevel is { } topLevel)
            {
                topLevel.RemoveHandler(DragDrop.DragEnterEvent, OnRowDragPreviewOver);
                topLevel.RemoveHandler(DragDrop.DragOverEvent, OnRowDragPreviewOver);
                topLevel.RemoveHandler(DragDrop.DragLeaveEvent, OnRowDragPreviewLeave);
                topLevel.RemoveHandler(DragDrop.DropEvent, OnRowDragPreviewDrop);
                _rowDragPreviewTopLevel = null;
            }

            if (_rowDragPreview is { } preview)
            {
                if (preview.Parent is Panel layer)
                {
                    layer.Children.Remove(preview);
                }

                if (preview.Child is Image image)
                {
                    image.Source = null;
                }
                else if (preview.Child is ContentControl content)
                {
                    content.Content         = null;
                    content.ContentTemplate = null;
                }

                preview.Child       = null;
                preview.DataContext = null;
                _rowDragPreview     = null;
            }

            _rowDragPreviewBitmap?.Dispose();
            _rowDragPreviewBitmap = null;
        }

        #endregion
    }
}
