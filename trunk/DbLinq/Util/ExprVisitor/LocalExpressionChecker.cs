using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace DbLinq.Util.ExprVisitor
{
    public delegate object FunctionReturningObject();

    public class LocalExpressionChecker : ExpressionVisitor
    {
        bool _foundParameter = false;
        bool _foundConstant = false;

        public LocalExpressionChecker()
        {
        }

        /// <summary>
        /// given 'localObject.FieldName', check if it's local, 
        /// and try to return compiled lambda expression {()->localObject.FieldName}.
        /// </summary>
        public static bool TryMatchLocalExpression(Expression expr, out FunctionReturningObject funcReturningObj)
        {
            try
            {
                LocalExpressionChecker obj = new LocalExpressionChecker();
                obj.Visit(expr);

                bool isLocal = obj._foundConstant && !obj._foundParameter;
                if (isLocal)
                {
                    //compile an access function
                    Expression expr2 = expr;
                    if (CSharp.IsPrimitiveType(expr.Type))
                        expr2 = Expression.Convert(expr, typeof(object));

                    var empty = new ParameterExpression[] { };
                    LambdaExpression lambda = Expression.Lambda<FunctionReturningObject>(expr2, empty);
                    Delegate delg = lambda.Compile();
                    funcReturningObj = delg as FunctionReturningObject;
                }
                else
                {
                    funcReturningObj = null;
                }
                return isLocal;

            }
            catch (Exception ex)
            {
                Console.WriteLine("TryMatchLocalExpression failed: " + ex);
                throw;
            }
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            _foundConstant = true;
            return base.VisitConstant(c);
        }
        protected override Expression VisitParameter(ParameterExpression p)
        {
            _foundParameter = true;
            return base.VisitParameter(p);
        }
    }
}
