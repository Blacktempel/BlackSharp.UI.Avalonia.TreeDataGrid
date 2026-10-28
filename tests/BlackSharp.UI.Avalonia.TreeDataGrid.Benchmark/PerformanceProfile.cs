using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using BlackSharp.UI.Avalonia.TreeDataGrid.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Benchmark;

internal static class PerformanceProfile
{
    #region Public

    public static void Run()
    {
        AppBuilder.Configure<Application>().UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
            .UseSkia().WithInterFont().SetupWithoutStarting();

        var results = new List<Measurement>();
        var model = new Node { Name = "CPU package temperature: 42.5 °C" };
        using var textSource = new FlatTreeDataGridSource<Node>(new[] { model })
            .WithTextColumn(x => x.Name, options => { options.IsReadOnly = true; options.UseDrawnCells = true; });
        var core = (ITreeDataGridSource)textSource;
        var cell = (ITextCell)core.Rows.RealizeCell(core.Columns[0], 0, 0);
        var renderer = (TreeDataGridTextCellRenderer)textSource.Columns[0].CellRenderer!;
        var owner = new Border();
        TextElement.SetForeground(owner, Brushes.White);
        TextElement.SetFontSize(owner, 14);
        var constraint = new Size(240, 32);
        results.Add(Measure("drawn-text-measure", 10000, () =>
        {
            for (var i = 0; i < 10000; ++i)
            {
                _ = renderer.MeasureCell(owner, cell, constraint);
            }
        }));
        using var bitmap = new RenderTargetBitmap(new PixelSize(240, 32));
        results.Add(Measure("drawn-text-render", 10000, () =>
        {
            using var context = bitmap.CreateDrawingContext();
            for (var i = 0; i < 10000; ++i)
            {
                renderer.RenderCell(owner, context, cell, new Rect(constraint));
            }
        }));
        core.Rows.UnrealizeCell(cell, 0, 0);

        var random = new Random(42);
        var items = Enumerable.Range(0, 5000).Select(i => new Node { Number = random.Next() }).ToArray();
        using var sorted = new FlatTreeDataGridSource<Node>(items).WithTextColumn(x => x.Number);
        _ = sorted.Rows[0];
        results.Add(Measure("sort-5000-fluent-rows", 3, () =>
        {
            for (var i = 0; i < 3; ++i)
            {
                sorted.SortBy(sorted.Columns[0], ListSortDirection.Ascending);
            }
        }));
        using var filtered = new FlatTreeDataGridSource<Node>(Enumerable.Range(0, 10000).Select(i => new Node { Number = i }));
        filtered.Filter(x => x.Number % 2 == 0);
        _ = filtered.Rows.Count;
        results.Add(Measure("filtered-row-lookup", 10000, () =>
        {
            for (var i = 0; i < 10000; ++i)
            {
                _ = filtered.ModelIndexToRowIndex(new IndexPath(i));
            }
        }));

        var root = new Node { Children = { new Node() } };
        using var hierarchy = new HierarchicalTreeDataGridSource<Node>(new[] { root })
            .WithHierarchicalExpanderTextColumn(x => x.Name, x => x.Children);
        var expander = (TreeDataGridHierarchicalExpanderColumn)hierarchy.Columns[0];
        results.Add(Measure("hierarchy-child-query-pair", 10000, () =>
        {
            for (var i = 0; i < 10000; ++i)
            {
                _ = expander.HasChildren(root);
                _ = expander.GetChildModels(root);
            }
        }));
        var branches = Enumerable.Range(0, 100).Select(i => new Node
        {
            Children = new ObservableCollection<Node>(Enumerable.Range(0, 20).Select(j => new Node())),
        }).ToArray();
        using var tree = new HierarchicalTreeDataGridSource<Node>(branches)
            .WithHierarchicalExpanderTextColumn(x => x.Name, x => x.Children);
        results.Add(Measure("expand-collapse-2100-rows", 3, () =>
        {
            for (var i = 0; i < 3; ++i)
            {
                tree.ExpandAll();
                tree.CollapseAll();
            }
        }));

        var wideRoot = new Node
        {
            Children = new ObservableCollection<Node>(Enumerable.Range(0, 2500).Select(_ => new Node())),
        };
        var wideItems = new[] { wideRoot }.Concat(Enumerable.Range(0, 2500).Select(_ => new Node())).ToArray();
        using var wideTree = new HierarchicalTreeDataGridSource<Node>(wideItems)
            .WithHierarchicalExpanderTextColumn(x => x.Name, x => x.Children);
        _ = wideTree.Rows.Count;
        results.Add(Measure("expand-collapse-wide-subtree", 3, () =>
        {
            for (var i = 0; i < 3; ++i)
            {
                wideTree.Expand(0);
                wideTree.Collapse(0);
            }
        }));

        Console.WriteLine(JsonSerializer.Serialize(new
        {
            Runtime = Environment.Version.ToString(),
            Samples = 5,
            Method = "Median elapsed time and managed allocations on the UI thread after three warmups; Skia; fixed seed 42.",
            Results = results,
        }, new JsonSerializerOptions { WriteIndented = true }));
    }

    #endregion

    #region Private

    private static Measurement Measure(string name, int operations, Action action)
    {
        for (var i = 0; i < 3; ++i)
        {
            action();
        }

        var elapsed = new double[5];
        var allocated = new long[5];
        for (var i = 0; i < 5; ++i)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var before = GC.GetAllocatedBytesForCurrentThread();
            var started = Stopwatch.GetTimestamp();
            action();
            elapsed[i] = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
            allocated[i] = GC.GetAllocatedBytesForCurrentThread() - before;
        }

        Array.Sort(elapsed);
        Array.Sort(allocated);
        return new Measurement(name, operations, elapsed[2], allocated[2], (double)allocated[2] / operations);
    }

    #endregion

    #region Nested Types

    private sealed record Measurement(string Name, int Operations, double Milliseconds, long AllocatedBytes, double BytesPerOperation);

    private sealed class Node
    {
        public string Name { get; set; } = "Sensor";

        public int Number { get; set; }

        public ObservableCollection<Node> Children { get; init; } = [];
    }

    #endregion
}
