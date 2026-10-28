using Avalonia;
using Avalonia.Collections;
using Avalonia.Styling;
using Avalonia.Threading;
using BenchmarkDotNet.Attributes;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using BlackSharp.UI.Avalonia.TreeDataGrid.Primitives;
using BlackSharp.UI.Avalonia.TreeDataGrid.Tests;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Benchmark;

public class Benchmarks
{
    #region Fields

    private const int RowCount    = 500;
    private const int ColumnCount = 40;

    private TreeDataGrid _target = default!;

    #endregion

    #region Public

    [GlobalSetup]
    public void Setup()
    {
        TestApplication.BuildAvaloniaApp().SetupWithoutStarting();
        _target = CreateTarget();
    }

    [Benchmark]
    public void Scrolling_Down()
    {
        for (int i = 0; i < RowCount; i++)
        {
            _target.Scroll!.Offset = new Vector(0, 10 * i);
            Layout(_target);
        }

        for (int i = RowCount - 1; i >= 0; i--)
        {
            _target.Scroll!.Offset = new Vector(0, 10 * i);
            Layout(_target);
        }
    }

    #endregion

    #region Private

    private static TreeDataGrid CreateTarget()
    {
        var items = new AvaloniaList<Model>([.. CreateModels("Item 0-", RowCount)]);

        var source = new FlatTreeDataGridSource<Model>(items);
        source.WithTextColumn("ID", x => x.Id);

        for (int i = 0; i < ColumnCount; i++)
        {
            source.WithTextColumn("Title", x => x.Title);
        }

        var target = new TreeDataGrid
        {
            Template = TestTemplates.TreeDataGridTemplate(),
            Source   = source,
        };

        var root = new TestWindow(target)
        {
            Styles =
                {
                    TestTemplates.TreeDataGridExpanderCellStyle,
                    TestTemplates.TreeDataGridTemplateCellStyle,
                    new Style(x => x.Is<TreeDataGridRow>())
                    {
                        Setters =
                        {
                            new Setter(TreeDataGridRow.TemplateProperty, TestTemplates.TreeDataGridRowTemplate()),
                        }
                    },
                    new Style(x => x.Is<TreeDataGridCell>())
                    {
                        Setters =
                        {
                            new Setter(TreeDataGridCell.HeightProperty, 10.0),
                        }
                    }
                }
        };

        root.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        return target;
    }

    private static void Layout(TreeDataGrid target)
    {
        target.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
    }

    private static IEnumerable<Model> CreateModels(
        string titlePrefix,
        int count,
        int firstIndex = 0,
        int firstId    = 100)
    {
        return Enumerable.Range(0, count).Select(x =>
            new Model
            {
                Id    = firstId + firstIndex + x,
                Title = titlePrefix + (firstIndex + x),
            });
    }

    #endregion

    #region Nested Types

    private class Model : NotifyingBase
    {
        #region Properties

        public int Id { get; set; }
        public string? Title { get; set; }

        #endregion
    }

    #endregion
}
