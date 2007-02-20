using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Query;
using System.Data.DLinq;
using System.Expressions;
using DBLinq.util;

namespace DBLinq.Linq.clause
{
    /// <summary>
    /// given an expression such as 'o.Customer', 
    /// call to see if there is a nickname for it, such as '$c'
    /// </summary>
    public delegate string AskNicknameHandler(Expression ex, AssociationAttribute assoc);

    public class WhereClauseBuilder
    {
        WhereClauses _clauses;
        readonly SqlExpressionParts _parts;

        public WhereClauseBuilder(SqlExpressionParts parts)
        {
            _parts = parts;
        }

        public event AskNicknameHandler NicknameRequest;

        /// <summary>
        /// given entire where clause (starts with MethodCall 'where'),
        /// find the lambda
        /// </summary>
        public static LambdaExpression FindLambda(Expression expr,out string methodName)
        {
            if(expr==null || expr.NodeType!=ExpressionType.MethodCall)
            {
                //expr.NoteType==Cast when we enter via EntitySet or EntityMSet
                throw new ApplicationException("FindLambda: L25 failure");
            }

            MethodCallExpression methodCall = (MethodCallExpression)expr;
            if(methodCall.Parameters.Count!=2)
                throw new ApplicationException("FindLambda: L28 failure");
                //return null; //"MethodCallExpr: expected 2 params";

            methodName = methodCall.Method.Name;
            //param0 is const-type
            Expression param1 = methodCall.Parameters[1];
            if(methodName=="Including")
            {
                //if(param1.NodeType==ExpressionType.NewArrayInit)...
                throw new ApplicationException("FindLambda: L38 'Including' clause not yet supported");
            }

            if(param1.NodeType!=ExpressionType.Lambda)
                throw new ApplicationException("FindLambda: L41 failure");
                //return null;
            return (LambdaExpression)param1;
        }

        public static LambdaExpression FindSelectManyLambda(LambdaExpression selectManyExpr,out string methodName)
        {
            Expression e1 = selectManyExpr.Body;
            if( !(e1 is UnaryExpression) )
                throw new ApplicationException("L44: Expected Unary inside SelectMany");
            UnaryExpression un = (UnaryExpression)e1;
            Expression e2 = un.Operand;
            if(e2.NodeType!=ExpressionType.MethodCall)
                throw new ApplicationException("L48: Expected Unary{MethodCall} inside SelectMany");
            //inside Unary is MethodCall "Select"
            MethodCallExpression methodCall = (MethodCallExpression)e2;
            //the Select contains MethodCall "Where" and Lambda specifying projection
            if(methodCall.Method.Name!="Select")
                throw new ApplicationException("L48: Expected Unary{MethodCall'Select'} inside SelectMany");
            Expression e3 = methodCall.Parameters[0];
            LambdaExpression lambda2 = FindLambda(e3,out methodName);
            return lambda2;
        }

        public WhereClauses Main_AnalyzeLambda(LambdaExpression expr)
        {
            this._clauses = new WhereClauses();
            RecurData recurData = new RecurData();
            AnalyzeExpression(recurData, expr.Body);
            //_clauses.nickName = "$"+expr.Parameters[0].Name; //eg "$e"
            _clauses.nickName = VarName.GetSqlName(expr.Parameters[0].Name); //eg "e$"
            return _clauses;
        }

        private void AnalyzeExpression(RecurData recurData, Expression expr)
        {
            recurData.depth++;
            switch(expr.NodeType)
            {
                case ExpressionType.GT:
                case ExpressionType.LT:
                case ExpressionType.EQ:
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    AnalyzeBinary(recurData, (BinaryExpression)expr);
                    return;
                case ExpressionType.MethodCall:
                case ExpressionType.MethodCallVirtual:
                    AnalyzeMethodCall(recurData, (MethodCallExpression)expr);
                    return;
                case ExpressionType.MemberAccess:
                    AnalyzeMember(recurData, (MemberExpression)expr);
                    return;
                case ExpressionType.Constant:
                    AnalyzeConstant(recurData, (ConstantExpression)expr);
                    return;
                case ExpressionType.Parameter:
                    AnalyzeParameter(recurData, (ParameterExpression)expr);
                    return;
                case ExpressionType.MemberInit:
                    AnalyzeMemberInit(recurData, (MemberInitExpression)expr);
                    return;
                case ExpressionType.Cast:
                    AnalyzeUnary(recurData, (UnaryExpression)expr);
                    return;
                default:
                    throw new ApplicationException("Analyze: L45 TODO: "+expr.NodeType);
            }
        }

        private void AnalyzeConstant(RecurData recurData, ConstantExpression expr)
        {
            object val = expr.Value;
            if(expr.Type==typeof(string))
            {
                //pass as named parameter:
                string paramName = _clauses.storeParam((string)val);
                _clauses.sb.Append(paramName);
                return;
            }
            if(expr.Type==typeof(DateTime) || expr.Type==typeof(DateTime?))
            {
                //this is where DateTime.Now gets given to us as a const DateTime
                if(val==null)
                {
                    _clauses.sb.Append("NULL"); //for Nullable DateTime only
                    return;
                }

                DateTime dt = (DateTime)val;
                _clauses.sb.Append("'");
                //TODO: how to format the datetime string?
                //on a UK machine, this format seems to work: '2007-12-03 08:25:00'
                _clauses.sb.Append(dt.ToString("yyyy-MM-dd hh:mm:ss"));
                _clauses.sb.Append("'");
                return;
            }
            _clauses.sb.Append(val.ToString());
        }

        private void AnalyzeMemberInit(RecurData recurData, MemberInitExpression expr)
        {
            _clauses.sb.Append("Init");
        }

        private void AnalyzeParameter(RecurData recurData, ParameterExpression expr)
        {
            //_clauses.sb.Append("$");
            //_clauses.sb.Append(expr.Name); //"$e"
            _clauses.sb.Append(VarName.GetSqlName(expr.Name)); //"e$"
        }

        private void AnalyzeMember(RecurData recurData, MemberExpression expr)
        {
            //TODO: check for 'o.Customer.City'
            MemberExpression memberInner = expr.Expression.XMember();
            if(memberInner!=null)
            {
                AssociationAttribute assoc1, assoc2;
                bool isAssoc = AttribHelper.IsAssociation(memberInner,out assoc1);
                if(isAssoc)
                {
                    //given 'o.Customer.City', 
                    //a) insert join $o.CustomerId=$c.CustomerID
                    //b) insert into our StringBuilder '$c.City'
                    string nick = this.NicknameRequest(expr, assoc1);
                    string nick2 = memberInner.Expression.XParam().Name;
                    assoc2 = AttribHelper.FindReverseAssociation(assoc1);

                    //string joinString = "$"+nick+"."+assoc1.ThisKey+"=$"+nick2+"."+assoc2.OtherKey;
                    string joinString = VarName.GetSqlName(nick)+"."+assoc1.ThisKey+"="+VarName.GetSqlName(nick2)+"."+assoc2.OtherKey;

                    _parts.joinList.Add(joinString);
                    //_clauses.sb.Append("$"+nick+"."+expr.Member.Name); 
                    _clauses.sb.Append(VarName.GetSqlName(nick)+"."+expr.Member.Name); 
                    //TODO - replace expr.Member.Name with SQL column name (use attribs)
                    Console.WriteLine("TODO: handle o.Customer.City !!!");
                    return;
                }
                else
                {
                    throw new Exception("L175 AnalyzeMember: member1.member2.member3 only allowed for associations, not for: "+expr);
                }
            }
            AnalyzeExpression(recurData, expr.Expression);
            _clauses.sb.Append(".");
            _clauses.sb.Append(expr.Member.Name);
        }

        private void AnalyzeMethodCall(RecurData recurData, MethodCallExpression expr)
        {
            //special handling
            if(expr.Method.Name=="op_Equality"){
                AnalyzeExpression(recurData, expr.Parameters[0]);
                _clauses.sb.Append(" = ");
                AnalyzeExpression(recurData, expr.Parameters[1]);
                return;
            }
            if(expr.Method.Name=="op_Inequality"){
                AnalyzeExpression(recurData, expr.Parameters[0]);
                _clauses.sb.Append(" != ");
                AnalyzeExpression(recurData, expr.Parameters[1]);
                return;
            }
            if(expr.Method.Name=="StartsWith"){
                //turn "e.Name.StartsWith("X")" -> "e.Name LIKE 'X%'
                AnalyzeExpression(recurData, expr.Object);
                _clauses.sb.Append(" LIKE ");
                AnalyzeExpression(recurData, expr.Parameters[0]);
                string paramName = _clauses.lastParamName;
                string lastParam = _clauses.paramMap[paramName] as string;
                if(lastParam !=null){
                    //modify parameter from X to X%
                    _clauses.paramMap[paramName] = lastParam+"%";
                }
                return;
            }
            //TODO: throw for any other method - database probably cannot handle such call
            StringBuilder sb = new StringBuilder();
            expr.BuildString(sb);
            string msg2 ="L160: Unprepared to map method "+expr.Method.Name+" ("+sb+") to SQL";
            Console.WriteLine(msg2);
            throw new ApplicationException(msg2);
            //_clauses.sb.Append(expr.Method.Name);
        }

        private void AnalyzeUnary(RecurData recurData, UnaryExpression expr)
        {
            AnalyzeExpression(recurData, expr.Operand);

            string operatorStr = "UNOP:"+expr.NodeType.ToString(); //formatBinaryOperator(expr.NodeType);
            _clauses.sb.Append(" "+ operatorStr + " ");
        }


        private void AnalyzeBinary(RecurData recurData, BinaryExpression expr)
        {
            int precedence = Operators.GetPrecedence(expr.NodeType);
            bool needsBrackets = (recurData.operatorPrecedence > precedence);
            recurData.operatorPrecedence = precedence; //nested methods will see different precedence
            
            if(needsBrackets)
            {
                _clauses.sb.Append("(");
            }

            AnalyzeExpression(recurData, expr.Left);

            string operatorStr = Operators.FormatBinaryOperator(expr.NodeType);
            _clauses.sb.Append(" "+ operatorStr + " ");

            AnalyzeExpression(recurData, expr.Right);
            if(needsBrackets)
            {
                _clauses.sb.Append(")");
            }
        }

        /// <summary>
        /// clause can be represented as string or (string+parameter)
        /// </summary>
        class Clause
        {
            readonly string clauseStr;
            public Clause(string str)
            {
                clauseStr = str;
            }
        }
        //List<Clause> clauses;

        internal struct RecurData
        {
            public int depth;
            public int operatorPrecedence;
        }

    }

    public class WhereClauses
    {
        public readonly Dictionary<string,object> paramMap = new Dictionary<string,object>();
        public readonly StringBuilder sb = new StringBuilder(200);
        public string lastParamName;

        /// <summary>
        /// nickname of our parameter, eg. '$e'
        /// </summary>
        public string nickName;

        /// <summary>
        /// store param in hashtable for later addition to MySqlCommand, 
        /// return param name
        /// </summary>
        public string storeParam(string value)
        {
            int count = paramMap.Count;
#if ORACLE
            string paramName = ":P"+count;
#else
            string paramName = "?P"+count;
#endif
            paramMap[paramName] = value;
            lastParamName = paramName;
            return paramName;
        }

        public void CopyInto(SqlExpressionParts sqlParts)
        {
            sqlParts.whereList.Add( this.sb.ToString());
            foreach(string key in this.paramMap.Keys)
            {
                sqlParts.paramMap.Add(key, this.paramMap[key]);
            }
        }

    }
}
