using Avalonia.Collections;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using BlackSharp.UI.Avalonia.TreeDataGrid.Primitives;
using BlackSharp.UI.Avalonia.TreeDataGrid.Selection;
using Xunit;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Tests.Selection;

public class TreeDataGridCellSelectionTests
{
    #region Public

    [AvaloniaFact]
    public void F2_Should_Edit_Current_Cell_After_Arrow_Navigation()
    {
        var target        = CreateTarget();
        var source        = (EngineFlatTreeDataGridSource<Model>)target.CoreSource!;
        var cellSelection = new EngineTreeDataGridCellSelectionModel<Model>(source)
        {
            SingleSelect = true
        };
        source.Selection = cellSelection;

        target.GetVisualDescendants()
            .OfType<TreeDataGridTextCell>()
            .First(x => x.RowIndex == 0)
            .Focus();

        cellSelection.SetSelectedRange(new(0, new(0)), 1, 1);
        Assert.Single(cellSelection.SelectedIndexes);
        Assert.Equal(0, cellSelection.SelectedIndexes[0].RowIndex);

        target.RaiseEvent(new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Key         = Key.Down,
        });

        Assert.Single(cellSelection.SelectedIndexes);
        Assert.Equal(1, cellSelection.SelectedIndexes[0].RowIndex);

        target.RaiseEvent(new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Key         = Key.F2,
        });

        var editingElement = target.GetVisualDescendants()
            .OfType<TreeDataGridTextCell>()
            .FirstOrDefault(x => x.IsFocused && x.IsVisible);

        Assert.NotNull(editingElement);
        Assert.Equal(1, editingElement.RowIndex);
    }

    #endregion

    #region Private

    private static TreeDataGrid CreateTarget(
        int itemCount  = 10,
        bool runLayout = true)
    {
        AvaloniaList<Model>? items = [.. Enumerable.Range(0, itemCount).Select(x =>
            new Model
            {
                Title = "Item " + x,
            })];

        var source = new EngineFlatTreeDataGridSource<Model>(items);
        source.Columns.Add(new TextColumn<Model, string?>("Title", x => x.Title, (o, v) => o.Title = v));

        var target = new TreeDataGrid
        {
            Template   = TestTemplates.TreeDataGridTemplate(),
            CoreSource = source,
        };

        var root = new TestWindow(target)
        {
            Styles =
                {
                    new Style(x => x.Is<TreeDataGridRow>())
                    {
                        Setters =
                        {
                            new Setter(TreeDataGridRow.TemplateProperty, TestTemplates.TreeDataGridRowTemplate()),
                        }
                    },
                }
        };

        if (runLayout)
        {
            root.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
        }

        return target;
    }

    #endregion

    #region Nested Types

    private class Model
    {
        #region Properties

        public string? Title { get; set; }

        #endregion
    }

    #endregion
}
