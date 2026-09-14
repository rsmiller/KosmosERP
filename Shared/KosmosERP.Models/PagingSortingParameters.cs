using System.Collections.ObjectModel;

namespace KosmosERP.Models;

public class PagingSortingParameters
{
    private const char ColumnSortSeparator = ',';
    private const char ColumnSortOrderSeparator = '-';

    public PagingSortingParameters()
    {
        this.SortDefinitions = new Collection<SortDefinition>();
    }

    public PagingSortingParameters(int start, int resultCount, string sortOrder)
    {
        this.Start = start;
        this.ResultCount = resultCount;
        this.SortDefinitions = this.ParseSortString(sortOrder);
    }

    /// <summary>
    ///   Gets or sets start the result Count
    /// </summary>
    /// <remarks>1 based (1 is the FIRST result)</remarks>
    public int Start { get; set; } = 0;

    /// <summary>
    ///   Gets or sets size of each result set
    /// </summary>
    public int ResultCount { get; set; } = 200;

    /// <summary>
    ///   Gets or sets list of Sort definitions
    /// </summary>
    public ICollection<SortDefinition> SortDefinitions { get; set; }

    private ICollection<SortDefinition> ParseSortString(string sortOrder)
    {
        if (String.IsNullOrWhiteSpace(sortOrder))
        {
            return new Collection<SortDefinition>();
        }

        var sortDefinitions = new Collection<SortDefinition>();
        var sortOrderColumns = sortOrder.Split(PagingSortingParameters.ColumnSortSeparator);

        foreach (var column in sortOrderColumns)
        {
            if (column.Contains(PagingSortingParameters.ColumnSortOrderSeparator, StringComparison.InvariantCulture) == false)
            {
                sortDefinitions.Add(new SortDefinition
                {
                    ColumnName = column,
                    SortOrder = SortOrder.Ascending
                });
                continue;
            }

            var sortFeature = column.Split(PagingSortingParameters.ColumnSortOrderSeparator);

            var sortDefinition = new SortDefinition
            {
                ColumnName = sortFeature.First(),
                SortOrder = this.DetermineSortOrder(sortFeature.Last())
            };
            sortDefinitions.Add(sortDefinition);
        }

        return sortDefinitions;
    }

    private SortOrder DetermineSortOrder(string sortOrder)
    {
        if (string.Compare(sortOrder, "desc", StringComparison.InvariantCultureIgnoreCase) == 0)
        {
            return SortOrder.Descending;
        }
        else if (string.Compare(sortOrder, "asc", StringComparison.InvariantCultureIgnoreCase) == 0)
        {
            return SortOrder.Ascending;
        }

        return SortOrder.None;
    }
}

public class SortDefinition
{
    public string ColumnName { get; set; } = "";

    public SortOrder SortOrder { get; set; }
}
