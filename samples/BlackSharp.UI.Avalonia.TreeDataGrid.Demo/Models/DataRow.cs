using Bogus.DataSets;
using System.Collections.ObjectModel;

namespace BlackSharp.UI.Avalonia.TreeDataGrid.Demo.Models;

internal class DataRow
{
    #region Fields

    private static Database s_database = new() { Random = new(0) };

    private ObservableCollection<DataRow>? _children;

    #endregion

    #region Properties

    public string Name { get; } = s_database.Random.Word();

    public List<DataCell> Cells { get; } = [];

    public ObservableCollection<DataRow> Children
    {
        get
        {
            if (_children == null)
            {
                SetColumnCount(_children = CreateRandomItems(), Cells.Count);
            }

            return _children;
        }
    }

    #endregion

    #region Public

    public static ObservableCollection<DataRow> CreateRandomItems()
    {
        return new ObservableCollection<DataRow>(Enumerable.Range(0, s_database.Random.Int(0, 15))
            .Select(x => new DataRow()));
    }

    public static string NewColumnName() => s_database.Column();

    public static void SetColumnCount(IEnumerable<DataRow> rows, int count)
    {
        foreach (var row in rows)
        {
            row.SetColumnCount(count);
        }
    }

    public void SetColumnCount(int count)
    {
        for (int i = Cells.Count - 1; i >= count; i--)
        {
            Cells.RemoveAt(i);
        }

        for (int i = Cells.Count; i < count; i++)
        {
            Cells.Add(new()
            {
                Value    = s_database.Random.Word(),
                ReadOnly = s_database.Random.Bool(0.15f)
            });
        }

        if (_children != null)
        {
            SetColumnCount(_children, count);
        }
    }

    #endregion
}
