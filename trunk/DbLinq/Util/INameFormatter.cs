
namespace DbLinq.Util
{
    public enum Case
    {
        Leave,
        camelCase,
        PascalCase
    }

    public interface INameFormatter
    {
        bool Singularize { get; set; }
        Case Case { get; set; }
        string AdjustTableName(string tableName);
        string AdjustColumnName(string columnName);
        string AdjustColumnFieldName(string columnName);
        string AdjustMethodName(string methodName);
    }
}
