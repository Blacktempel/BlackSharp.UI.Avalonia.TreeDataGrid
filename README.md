<h1 align="center">BlackSharp TreeDataGrid for Avalonia</h1>

<p align="center">
    <img src="https://raw.githubusercontent.com/Blacktempel/BlackSharp.UI.Avalonia.TreeDataGrid/master/Assets/BlackSharpFull.png" width="128" height="128"/>
</p>

[![NuGet](https://img.shields.io/nuget/v/BlackSharp.UI.Avalonia.TreeDataGrid?label=NuGet)](https://www.nuget.org/packages/BlackSharp.UI.Avalonia.TreeDataGrid/)
[![NuGet downloads](https://img.shields.io/nuget/dt/BlackSharp.UI.Avalonia.TreeDataGrid?label=Downloads)](https://www.nuget.org/packages/BlackSharp.UI.Avalonia.TreeDataGrid/)

[![.NET](https://img.shields.io/badge/.NET-8%20%7C%2010-512BD4?logo=dotnet&logoColor=white)](#requirements)
[![GitHub Sponsors](https://img.shields.io/badge/GitHub%20Sponsors-Sponsor-EA4AAA?logo=githubsponsors&logoColor=white)](https://github.com/sponsors/Blacktempel)
[![PayPal](https://img.shields.io/badge/PayPal-Donate-003087?logo=paypal&logoColor=white)](https://paypal.me/Blacktempel)
[![License](https://img.shields.io/badge/License-MPL--2.0-FF7518)](LICENSE)

**Display hierarchical and tabular data in one virtualized Avalonia control.**

BlackSharp.UI.Avalonia.TreeDataGrid is based on the original Avalonia
TreeDataGrid repository and the changes implemented by the fork of fidarit.

This is not an official Avalonia release.

## Key capabilities

- Virtualized flat and hierarchical rows with observable collection updates
- Text, checkbox, template, row-header and hierarchical expander columns
- Columns declared in XAML or configured through fluent source helpers
- Row and cell selection, sorting, filtering, editing and validation
- Frozen and reorderable columns, grid lines and drawn text cells
- Row drag and drop with a default snapshot or a custom preview template
- Expansion and selection retained when rows move within a collection
- Optional double-click expansion, left-arrow parent navigation and
  selection-preserving Ctrl+Arrow scrolling

The [demo](samples/BlackSharp.UI.Avalonia.TreeDataGrid.Demo/) shows these features
alongside Countries, Files, Wikipedia, Drag/Drop and Dynamic examples. The
Wikipedia example requires an internet connection.

## Supported .NET versions

| .NET version | Support |
| --- | --- |
| .NET 8 | ✅ |
| .NET 10 | ✅ |

## Requirements

To build a package from this checkout:

```shell
dotnet pack src/BlackSharp.UI.Avalonia.TreeDataGrid/BlackSharp.UI.Avalonia.TreeDataGrid.csproj -c Release -o artifacts/packages
```

Add the resulting `BlackSharp.UI.Avalonia.TreeDataGrid` package to a local NuGet
feed, or reference the library project directly. Once the feed is configured,
add the package to your application:

```shell
dotnet add package BlackSharp.UI.Avalonia.TreeDataGrid
```

Include the TreeDataGrid theme after Avalonia's Fluent theme:

```xml
<Application.Styles>
    <FluentTheme/>
    <StyleInclude Source="avares://BlackSharp.UI.Avalonia.TreeDataGrid/Themes/Fluent.axaml"/>
</Application.Styles>
```

Add `xmlns:tree="using:BlackSharp.UI.Avalonia.TreeDataGrid"` to the XAML root that
contains the grid. Bind the view to a view model through your application's
usual data-context setup.

## Examples

The view model owns the grid source, its columns and selection settings. The
view binds to that source; it does not need to construct columns or manipulate
rows in code-behind.

### Flat grid

`Person` is an application model with writable `Name` and `Active` properties.
It should raise `PropertyChanged` when either value changes. Adding or removing
items from the observable collection updates the grid:

```csharp
using Avalonia;
using BlackSharp.UI.Avalonia.TreeDataGrid;
using System.Collections.ObjectModel;

public sealed class PeopleViewModel : IDisposable
{
    public PeopleViewModel()
    {
        Source = new FlatTreeDataGridSource<Person>(People)
            .WithRowHeaderColumn("#")
            .WithTextColumn("Name", person => person.Name,
                options => options.Width = new GridLength(240))
            .WithCheckBoxColumn("Active", person => person.Active);

        Source.RowSelection!.SingleSelect = false;
    }

    public ObservableCollection<Person> People { get; } = [];

    public FlatTreeDataGridSource<Person> Source { get; }

    public void Add(Person person) => People.Add(person);

    public void Dispose() => Source.Dispose();
}
```

The writable properties make the text and checkbox columns editable. Set
`options.IsReadOnly = true` for a column that must only display values.

### Hierarchical grid

`Node` exposes `Name`, `Description`, observable `Children` and a writable
`IsExpanded` property. Its property setters should raise `PropertyChanged`.
Keeping expansion on the node preserves it when the row is offscreen or moves:

```csharp
using Avalonia;
using BlackSharp.UI.Avalonia.TreeDataGrid;
using System.Collections.ObjectModel;

public sealed class TreePageViewModel : IDisposable
{
    public TreePageViewModel()
    {
        Source = new HierarchicalTreeDataGridSource<Node>(Roots)
            .WithHierarchicalExpanderTextColumn(
                "Name", node => node.Name, node => node.Children,
                isExpanded: node => node.IsExpanded,
                options: options =>
                {
                    options.Width = new GridLength(260);
                    options.IsReadOnly = true;
                })
            .WithTextColumn("Description", node => node.Description,
                options => options.IsReadOnly = true);
    }

    public ObservableCollection<Node> Roots { get; } = [];

    public HierarchicalTreeDataGridSource<Node> Source { get; }

    public void AddRoot(Node node) => Roots.Add(node);

    public void ExpandFirstRoot()
    {
        if (Roots.Count > 0)
        {
            Roots[0].IsExpanded = true;
        }
    }

    public void Dispose() => Source.Dispose();
}
```

The view is the same for either view model:

```xml
<tree:TreeDataGrid Source="{Binding Source}"/>
```

A source assigned through `Source` belongs to the view model and must be
disposed when that view model ends. A hierarchical source has one expander
column; observable child collections can change after the grid is shown.

## Interaction and drag preview

These control properties opt into additional tree navigation:

```xml
<tree:TreeDataGrid Source="{Binding Source}"
                   ExpandCollapseOnDoubleTap="True"
                   SelectParentOnLeftArrow="True"
                   PreserveSelectionOnCtrlArrowScroll="True"/>
```

Double-clicking a non-editable part of a row toggles expansion. Left collapses an
expanded branch or selects its parent; embedded editors keep their own keyboard
input. Ctrl+Up/Down scrolls without changing the selection.

Row dragging shows a snapshot of the dragged row by default. Set
`DragPreviewTemplate` to replace it with a model-based template. The preview
follows the pointer inside the source window and is released when dragging ends.

## Projects and development

| Project | Purpose |
| --- | --- |
| [`src/BlackSharp.UI.Avalonia.TreeDataGrid`](src/BlackSharp.UI.Avalonia.TreeDataGrid/) | Reusable control and NuGet package |
| [`samples/BlackSharp.UI.Avalonia.TreeDataGrid.Demo`](samples/BlackSharp.UI.Avalonia.TreeDataGrid.Demo/) | Interactive feature examples |
| [`tests/BlackSharp.UI.Avalonia.TreeDataGrid.Tests`](tests/BlackSharp.UI.Avalonia.TreeDataGrid.Tests/) | API and behavior tests |
| [`tests/BlackSharp.UI.Avalonia.TreeDataGrid.RenderingTests`](tests/BlackSharp.UI.Avalonia.TreeDataGrid.RenderingTests/) | Rendering regression tests |
| [`tests/BlackSharp.UI.Avalonia.TreeDataGrid.Benchmark`](tests/BlackSharp.UI.Avalonia.TreeDataGrid.Benchmark/) | Performance profiles |

More detail is available for [column options, sorting and
filtering](docs/column-options.md), [editing and validation](docs/editing.md) and
[selection](docs/selection.md). Keep changes focused and update the relevant tests
and examples when changing behavior.

## License

This repository is licensed under the [Mozilla Public License 2.0](LICENSE).

### Origin

This repository contains code derived from the
[original Avalonia TreeDataGrid](https://github.com/AvaloniaUI/Avalonia.Controls.TreeDataGrid)
and [Fidarit fork](https://github.com/fidarit/TreeDataGrid.Avalonia),
both distributed under the MIT license. Their
[original MIT license text](LICENSE-MIT) and existing source notices are
retained. The MPL license applies to this repository's distribution; it
does not change the original copyright notices.
