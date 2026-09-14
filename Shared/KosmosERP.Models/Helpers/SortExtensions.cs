using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;

namespace KosmosERP.Models.Helpers;

public static class SortExtensions
{

    public static IList<T> JustSort<T>(this IList<T> query, PagingSortingParameters args)
         where T : class
    {
        if (args == null)
        {
            throw new ArgumentNullException(nameof(args));
        }

        Contract.EndContractBlock();

        //  handle multi column sort
        var firstsortdef = args.SortDefinitions.First();
        var propertyInfo = typeof(T).GetProperty(firstsortdef.ColumnName);

        if (propertyInfo != null)
        {
            switch (firstsortdef.SortOrder)
            {
                case SortOrder.Ascending:
                    query = query.OrderBy(p => propertyInfo.GetValue(p)).ToList();
                    break;
                case SortOrder.Descending:
                    query = query.OrderByDescending(p => propertyInfo.GetValue(p)).ToList();
                    break;
            }
        }

        return query;
    }
    public static IList<T> SortAndPageBy<T>(this IList<T> query, PagingSortingParameters args)
        where T : class
    {
        if (args == null)
        {
            throw new ArgumentNullException(nameof(args));
        }

        Contract.EndContractBlock();

        //  handle multi column sort
        var firstsortdef = args.SortDefinitions.First();
        var propertyInfo = typeof(T).GetProperty(firstsortdef.ColumnName);

        if (propertyInfo != null)
        {
            switch (firstsortdef.SortOrder)
            {
                case SortOrder.Ascending:
                    query = query.OrderBy(p => propertyInfo.GetValue(p)).ToList();
                    break;
                case SortOrder.Descending:
                    query = query.OrderByDescending(p => propertyInfo.GetValue(p)).ToList();
                    break;
            }
        }
        query = query.Skip(args.Start - 1).Take(args.ResultCount).ToList();

        return query;
    }

    public static IQueryable<T> SortAndPageBy<T>(this IQueryable<T> query, PagingSortingParameters args)
        where T : class
    {
        if (args == null)
        {
            throw new ArgumentNullException(nameof(args));
        }

        Contract.EndContractBlock();

        //  handle first entry
        if (args.SortDefinitions.Count() > 0)
        {
            var orderedquery = (IOrderedQueryable<T>)query;
            var firstsortdef = args.SortDefinitions.First();
            switch (firstsortdef.SortOrder)
            {
                case SortOrder.Ascending:
                    orderedquery = query.OrderBy(firstsortdef.ColumnName);
                    break;
                case SortOrder.Descending:
                    orderedquery = query.OrderByDescending(firstsortdef.ColumnName);
                    break;
            }

            // handle rest of the sort columns
            foreach (var sortdef in args.SortDefinitions.Skip(1))
            {
                switch (sortdef.SortOrder)
                {
                    case SortOrder.Ascending:
                        orderedquery = orderedquery.ThenBy(sortdef.ColumnName);
                        break;
                    case SortOrder.Descending:
                        orderedquery = orderedquery.ThenByDescending(sortdef.ColumnName);
                        break;
                }
            }

            query = orderedquery;
        }

        query = query.Skip(args.Start - 1).Take(args.ResultCount);

        return query;
    }

    //  http://stackoverflow.com/questions/41244/dynamic-linq-orderby
    public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string property)
    {
        return SortExtensions.ApplyOrder(source, property, "OrderBy");
    }

    public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string property)
    {
        return SortExtensions.ApplyOrder(source, property, "OrderByDescending");
    }

    public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string property)
    {
        return SortExtensions.ApplyOrder(source, property, "ThenBy");
    }

    public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, string property)
    {
        return SortExtensions.ApplyOrder(source, property, "ThenByDescending");
    }

    private static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> source, string property, string methodName)
    {
        string[] props = property.Split('.');
        Type type = typeof(T);
        ParameterExpression arg = Expression.Parameter(type, "x");
        Expression expr = arg;
        try
        {
            foreach (string prop in props)
            {
                // use reflection (not ComponentModel) to mirror LINQ
                PropertyInfo pi = type.GetProperty(prop, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }
        }
        catch (Exception ex)
        {
            throw new MissingFieldException("Sort Column Name does not match a Column Name on the Resource.", ex);
        }

        Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
        LambdaExpression lambda = Expression.Lambda(delegateType, expr, arg);

        object result = typeof(Queryable).GetMethods()
                                         .Single(
                                             method => method.Name == methodName
                                                       && method.IsGenericMethodDefinition
                                                       && method.GetGenericArguments().Length == 2
                                                       && method.GetParameters().Length == 2)
                                         .MakeGenericMethod(typeof(T), type)
                                         .Invoke(null, new object[] { source, lambda });
        return (IOrderedQueryable<T>)result;
    }
}
