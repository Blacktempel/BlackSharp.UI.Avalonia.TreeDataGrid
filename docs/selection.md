# Selection

Set `SelectionMode` on the control to `None`, `Row`, `Cell`, or combine Row/Cell with `Multiple`.

```xml
<TreeDataGrid Source="{Binding Source}" SelectionMode="Cell,Multiple"/>
```

Sources start with row selection. A view model can replace it explicitly:

```csharp
source.Selection = new TreeDataGridCellSelectionModel<Item>(source)
{
    SingleSelect = false,
};
source.CellSelection.SetSelectedRange(new CellIndex(0, new IndexPath(0)), 3, 4);
source.CellSelection.SelectAll();
```

Row selection supports `Select`, `Deselect`, `SelectRange`, `SelectAll`, `Clear` and batch updates. Cell selection exposes a selected cell/range and batch updates. Index paths address the underlying model; flat path `2` identifies the third item, while hierarchical path `2,1` identifies its second child. Sorting and filtering preserve these model paths.

`SelectedIndex` and `SelectedItem` on `TreeDataGrid` support two-way binding. Both source and control report selection changes; typed event args include added/removed model items and model/cell index paths. The former split public row/cell selection event types and interfaces have been replaced by the v12 selection model.

Select-all and row range operations use currently visible rows. Ctrl+A selects all when multiple selection is enabled. A row-header click selects the row; in multiple cell mode it selects its cells across columns.

A selection belongs to its source. Replacing it clears and detaches its old item subscriptions; the same selection can subsequently be attached to that source again. A selection from a different source is rejected.
