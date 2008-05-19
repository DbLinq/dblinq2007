namespace DbLinq.Linq.Data.Sugar
{
    public interface IQueryBuilder
    {
        Query GetQuery(ExpressionChain expressions, QueryContext queryContext);
    }
}