////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.Expressions;
using System.Collections.Generic;
using System.Text;

namespace DBLinq.util
{
    public static class ExprEnum
    {
        public static IEnumerable<ExprPair> EnumExpressions(Expression e1)
        {
            if(e1==null)
                yield break;

            //yield return e1;
            switch(e1.NodeType)
            {
                case ExpressionType.Parameter:
                    {
                        break;
                    }
                case ExpressionType.MemberAccess:
                    {
                        MemberExpression memberEx = (MemberExpression)e1;
                        yield return new ExprPair(memberEx,memberEx.Expression);
                        foreach(ExprPair pair in EnumExpressions(memberEx.Expression))
                        { yield return pair; }
                    }
                    break;
                case ExpressionType.MemberInit:
                    {
                        MemberInitExpression mInit = (MemberInitExpression)e1;
                        foreach(Binding bind in mInit.Bindings)
                        {
                            switch(bind.BindingType)
                            {
                                case BindingType.MemberAssignment:
                                    MemberAssignment memberAssign = (MemberAssignment)bind;
                                    yield return new ExprPair(e1,memberAssign.Expression,memberAssign.Member.Name);
                                    foreach(ExprPair pair in EnumExpressions(memberAssign.Expression))
                                    { yield return pair; }
                                    break;
                                default:
                                    Console.WriteLine("TODO: bind XXX: "+bind);
                                    break;

                            }
                            //Expression bindExpr = bind as Expression;
                            //foreach(Expression e2 in EnumExpressions(bindExpr)){ yield return e2; }
                        }

                        yield return new ExprPair(e1,mInit.NewExpression);
                        foreach(ExprPair pair in EnumExpressions(mInit.NewExpression))
                        { yield return pair; }
                    }
                    break;
                case ExpressionType.Lambda:
                    {
                        LambdaExpression lambda = (LambdaExpression)e1;
                        foreach(Expression paramExpr in lambda.Parameters)
                        {
                            yield return new ExprPair(e1,paramExpr);
                            foreach(ExprPair pair in EnumExpressions(paramExpr))
                            { yield return pair; }
                        }
                        yield return new ExprPair(e1,lambda.Body);
                        foreach(ExprPair pair in EnumExpressions(lambda.Body))
                        { yield return pair; }
                    }
                    break;
                default:
                    Console.WriteLine("TODO: EnumExpr handle "+e1);
                    break;
            }
        }
    }
    public class ExprPair
    {
        public readonly Expression parent; 
        public readonly Expression child;
        public readonly string name;
        public ExprPair(Expression p,Expression c){ parent=p; child=c; }
        public ExprPair(Expression p,Expression c,string n){ parent=p; child=c; name=n; }
    }
}
