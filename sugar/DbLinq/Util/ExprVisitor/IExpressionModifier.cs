using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace DbLinq.Util.ExprVisitor
{
    /// <summary>
    /// allows modification of expression trees.
    /// </summary>
    public interface IExpressionModifier
    {
        Expression Modify(Expression expression);
    }
}
