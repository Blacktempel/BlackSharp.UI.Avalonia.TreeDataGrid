# Editing and validation

Text and checkbox columns expose `IsReadOnly`. Fluent helpers infer a read-only column for non-writable expressions; a writable text property is editable by default. Standard XAML bindings should use `Mode=TwoWay` for editors.

`BeginEditGestures` supports `F2`, `Tap`, `DoubleTap`, `TextInput` and the `WhenSelected` modifier. The default is F2 or double-tap. Typing starts an enabled text editor with the typed text as the initial value.

```csharp
source.WithTextColumn("Name", item => item.Name, options =>
{
    options.BeginEditGestures = BeginEditGestures.Default | BeginEditGestures.TextInput;
});
```

Enter commits, Escape cancels, and Tab/Shift+Tab commits and moves to the next/previous editable visible cell. Popup content logically belonging to a template editor can retain editing focus. `BeginEdit(columnIndex, rowIndex)`, `CommitEdit()` and `CancelEdit()` provide control-level access.

Numeric text editing converts using the column’s configured culture. Invalid conversion retains the editor and sets Avalonia validation errors instead of throwing or overwriting the model. Cancel restores the previous display value. `StringFormat` affects display while the editor uses the underlying value.

```csharp
source.WithTextColumn("Temperature", item => item.Temperature, options =>
{
    options.Culture = CultureInfo.GetCultureInfo("de-DE");
    options.StringFormat = "{0:N1} °C";
});
```

A template editing control is responsible for its model bindings and validation rules. Use an editing template for ComboBox, date/time or custom editors. The grid does not hide programming errors in application callbacks, broken templates or model setters behind blanket exception handlers.
