# Column options, sorting and filtering

Columns expose `Width`, `MinWidth`, `MaxWidth`, `IsVisible`, `Header`, `Tag`, alignment, per-column resize/reorder/sort permissions and sort direction. Fluent helpers accept an options callback:

```csharp
source.WithTextColumn("Name", item => item.Name, options =>
{
    options.Width = GridLength.Star;
    options.MinWidth = new GridLength(160);
    options.MaxWidth = new GridLength(600);
    options.IsReadOnly = true;
});
```

`TreeDataGridSource` provides defaults for `CanUserResizeColumns`, `CanUserSortColumns`, `CanUserReorderColumns`, `AllowTriStateSorting`, `AutoDragDropRows`, `FrozenColumnCount` and `ShowColumnHeaders`. Explicit control values take precedence. Per-column nullable permissions override the corresponding grid permission.

Header clicks cycle ascending/descending and, with tri-state sorting enabled, unsorted. Programmatic sorting is available independently of user permissions:

```csharp
source.SortBy(source.Columns[0], ListSortDirection.Ascending);
source.ClearSort(source.Columns[0]);
source.Filter(item => item.Visible);
source.RefreshFilter();
source.Filter(null);
```

Filtering does not modify the item collection or its model index paths. Sorting changes row order rather than model paths. Call `RefreshFilter()` after changes to properties used by the predicate.

Set `FrozenColumnCount` to pin leading columns during horizontal scrolling. Hidden columns remain members of the column collection. A header drag reorders within the frozen or scrollable region; it cannot cross that boundary. `ColumnReordering` is cancellable; `ColumnReordered` reports completion. `Columns.Move(oldIndex, newIndex)` moves a column programmatically.

`GridLinesVisibility` accepts `None`, `Horizontal`, `Vertical` or `All`. `RowTheme` and `ColumnHeaderTheme` customize containers. The demo shows these features together and exposes the frozen count at runtime.
