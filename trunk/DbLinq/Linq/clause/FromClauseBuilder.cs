using System;
using System.Expressions;
using System.Collections.Generic;
using System.Text;
using System.Data.DLinq;
using DBLinq.util;

namespace DBLinq.Linq.clause
{
    class FromClauseBuilder
    {
        /// <summary>
        /// given type Employee, select all its fields: 'SELECT e.ID, e.Name,... FROM Employee'
        /// (by examining [Table] and [Column] attribs)
        /// </summary>
        /// <param name="selectParts">'output' - gets populated with query parts.</param>
        /// <param name="t">input type</param>
        /// <param name="nick">nickname such as $o for an Order</param>
        /// <returns></returns>
        public static void SelectAllFields(SessionVars vars, SqlExpressionParts selectParts, Type t1, string nick)
        {
            Type t = AttribHelper.ExtractTypeFromMSet(t1);
            TableAttribute tAttrib = AttribHelper.GetTableAttrib(t);
            if(tAttrib==null && t.Name.StartsWith("<Projection>"))
            {
                //GroupBy: given t=Proj, find our table type
                //example: GroupBy-proj: {c => new {PostalCode = c.PostalCode, ContactName = c.ContactName}}
                if(t==vars.groupByNewExpr.Body.Type){
                    Type groupByParamT = vars.groupByNewExpr.Parameters[0].Type; //this is 'c' in the example
                    tAttrib = AttribHelper.GetTableAttrib(groupByParamT);
                }
            }

            if(tAttrib==null)
                throw new ApplicationException("Class "+t+" is missing [Table] attribute");

            if(selectParts.doneClauses.Contains(nick))
            {
                Console.WriteLine("Duplicate GetClause for "+nick+", skipping");
                return;
            }
            selectParts.doneClauses.Add(nick);

            ColumnAttribute[] colAttribs = AttribHelper.GetColumnAttribs(t);

            foreach(ColumnAttribute colAtt in colAttribs)
            {
                string part = nick+"."+colAtt.Name; //eg. '$o.OrderID'
                selectParts.selectFieldList.Add( part );
            }

            string tableName2 = tAttrib.Name + " " + nick;
            selectParts.AddFrom( tableName2 );
        }

        public static void GetClause_Projected(SessionVars vars, SqlExpressionParts selectParts, Type t, string nick, LambdaExpression selectExpr)
        {
            if(selectExpr==null)
            {
                SelectAllFields(vars, selectParts, t, nick); //if no projection, select everything
                return;
            }

            TableAttribute tAttrib = AttribHelper.GetTableAttrib(t);
            if(tAttrib==null && t.ToString().Contains("IGrouping`2"))
            {
                //special handling for GroupBy expression
                Type[] genArgs = t.GetGenericArguments();
                Type genArg1 = genArgs[1];
                if(genArg1.Name.StartsWith("<Projection")){
                    //'group new {c.PostalCode, c.ContactName} by c.City into g'
                    Type t2 = vars.groupByExpr.Parameters[0].Type; //select 'c' (Customer)
                    tAttrib = AttribHelper.GetTableAttrib(t2);
                } else {
                    tAttrib = AttribHelper.GetTableAttrib(genArg1);
                }
            }

            if(tAttrib==null)
                throw new ApplicationException("Class "+t+" is missing [Table] attribute");

            string tableName2 = tAttrib.Name+" "+nick; //eg. "Employee $e"
            selectParts.AddFrom(tableName2); //2B||!2B?!
            //REMOVED***

            if(selectExpr==null)
                return;

            ExpressionType etype = selectExpr.Body.NodeType;
            //StringBuilder sbSelect = new StringBuilder("SELECT ");
            switch(etype)
            {
                case ExpressionType.MemberInit:
                    //member init specifies the projection - specifies who to select
                    MemberInitExpression exprMember = (MemberInitExpression)selectExpr.Body;
                    GetClauseProjected_MemberInit(vars, selectParts, exprMember);
                    break;
                case ExpressionType.MemberAccess:
                    //user wishes to select one field out of a class
                    string s3 = FormatMemberExpression((MemberExpression)selectExpr.Body);
                    Console.WriteLine(" -- FromClauseBuilder.GetClause_Proj:  SELECT "+s3);
                    selectParts.selectFieldList.Add(s3); //append '$e.ID'
                    break;
                case ExpressionType.Cast:
                    //this occurs for double select:
                    //  var q = from c in db.customers from o in c.Orders 
                    //  where c.City == "London" select new { c, o };

                    //We wish to build a SQL statement like this:
                    //select c.*, o.* from Customer c, Orders o
                    //where c.city='London' AND c.CustomerID=o.CustomerID
                    MethodCallExpression callEx = selectExpr.Body.XCast().XOp().XMethodCall();
                    if(callEx==null || callEx.Method.Name!="Select")
                        throw new ApplicationException("L87 Expected Cast{MethodCall 'Select'}, not "+selectExpr);
                    GetClause_Projected_Select(vars, selectParts,callEx);
                    break;

                default:
                    throw new ApplicationException("FromClauseBuilder L68: todo etype="+etype);
            }
        }

        /// <summary>
        /// As mentioned above, this is used during double select 
        /// (from c ... from o ... select new (o,c})
        /// </summary>
        static void GetClause_Projected_Select(SessionVars vars, SqlExpressionParts selectParts, MethodCallExpression selectCallEx)
        {
            Console.WriteLine("  GetClause_Proj_Select: "+selectCallEx);
            string callName = selectCallEx.Method.Name;
            //the parameters are two queries
            foreach(Expression par in selectCallEx.Parameters)
            {
                Console.WriteLine("    GetClause..Param: "+par);
                MethodCallExpression callEx2 = par.XMethodCall();
                LambdaExpression lambda1 = par.XLambda();
                if(callEx2!=null && callEx2.Method.Name=="Where" && callEx2.Parameters.Count==2)
                {
                    //Param0: c.Orders.Where(o => op_Equality(c.City, "London"))
                    MemberExpression member2 = callEx2.Parameters[0].XCast().XOp().XMember();
                    LambdaExpression lambda2 = callEx2.Parameters[1].XLambda();
                    //ParameterExpression param10 = callEx2.Parameters[1].XLambda().Parameters[0];
                    ParameterExpression param10 = lambda2.Parameters[0];
                    AssociationAttribute assoc;

                    //WhereClauses whereClauses = new WhereClauseBuilder().Main_AnalyzeLambda(lambda2);
                    //whereClauses.CopyInto(selectParts);
                    Console.WriteLine("FromClauseBuilder: Disabled: new WhereClauseBuilder");

                    if(AttribHelper.IsAssociation(member2,out assoc))
                    {
                        //handle association {c.Orders}
                        ReverseAssociation assocParts = new ReverseAssociation();
                        assocParts.child.varName = member2.XMember().Expression.XParam().Name; //'c'
                        assocParts.parent.varName = param10.Name; //'o'
                        assocParts.parent.propInfo = member2.Member as System.Reflection.PropertyInfo;
                        GetClause_Assoc(selectParts, assocParts);
                        continue;
                    }
                    Expression par20 = callEx2.Parameters[0]; //{Cast{MemberAccess}}
                    Expression par21 = callEx2.Parameters[1]; //{Lambda{body=eq,param={o}}
                    //param1: {c.Orders}
                    //param2: {o => op_Equality(c.City, "London")}
                }
                else
                {
                    Console.WriteLine("Error L106: Unprepared for param "+par);
                }

                if(lambda1!=null && lambda1.Body.NodeType==ExpressionType.MemberInit)
                {
                    GetClauseProjected_MemberInit(vars, selectParts, (MemberInitExpression)lambda1.Body);
                    //GetClause_Projected(selectParts, t, "$$", lambda1);
                }
                StringBuilder sb2 = new StringBuilder();
                par.BuildString(sb2);
                Console.WriteLine("  Param: "+sb2);
            }
            Console.WriteLine("Done Params");
        }

        static void GetClauseProjected_MemberInit(SessionVars vars, SqlExpressionParts selectParts, MemberInitExpression exprMember)
        {
            //int numFields = 0;
            foreach(MemberAssignment member1 in exprMember.Bindings)
            {
                //example of member1: "c=o.Customer" -> 
                //-> from this, we need to retrieve both 'c' and 'o' as nickNames

                string varName = member1.Member.Name; //eg. 'c'
                //if(numFields++>0){ sbSelect.Append(", "); }
                TypeEnum typeEnum = CSharp.CategorizeType(member1.Expression.Type);
                switch(member1.Expression.NodeType)
                {
                    case ExpressionType.Parameter:
                        //handle from a ... from b ... select {a,b}
                        ParameterExpression paramExpr = (ParameterExpression)member1.Expression;
                        switch(typeEnum)
                        {
                            case TypeEnum.Column:
                                SelectAllFields(vars, selectParts, paramExpr.Type, VarName.GetSqlName(varName)); //todo - fix the $x
                                break;
                            default:
                                if(paramExpr.Type.Name.StartsWith("IGrouping`"))
                                {
                                    //replace the GroupBy sucker with the "new {c.Customer}" that it refers to
                                    Type groupByType = vars.groupByNewExpr.Body.Type;
                                    SelectAllFields(vars, selectParts, groupByType, VarName.GetSqlName(varName)); //todo - fix the $x
                                    break;
                                }
                                else
                                {
                                    throw new ArgumentOutOfRangeException("Bad MemberEnum type "+paramExpr.Type);
                                }
                        }
                        //sbSelect.Append("[TODO: append all fields of "+paramExpr.Name+"]");
                        break;
                    case ExpressionType.MemberAccess:
                        MemberExpression memberExpr = (MemberExpression)member1.Expression;
                        ParameterExpression memberExprParm = (ParameterExpression)memberExpr.Expression;
                        if(memberExprParm.Type.Name == "IGrouping`2")
                        {
                            //handled in ProjectionData. TODO: move logic here.
                            continue;
                        }
                        AssociationAttribute assoc;
                        bool isAssociation = AttribHelper.IsAssociation(memberExpr,out assoc);
                        if(isAssociation)
                        {
                            ReverseAssociation assocParts = new ReverseAssociation();
                            assocParts.child.varName = memberExprParm.Name; //'o' in "c=o.Customer"
                            assocParts.parent.varName = varName; //'c'
                            assocParts.child.propInfo = memberExpr.Member as System.Reflection.PropertyInfo;
                            GetClause_Assoc(selectParts, assocParts);
                        }

                        switch(typeEnum)
                        {
                            case TypeEnum.Column:
                                //eg. "from o in Orders select new {o.Customer}"
                                //append all fields, and add a join from Orders to Customers
                                
                                //GetClause(selectParts, memberExpr.Type, "$"+varName); //todo - fix the $x
                                SelectAllFields(vars, selectParts, memberExpr.Type, VarName.GetSqlName(varName)); //todo - fix the $x
                                //selectParts.joinList.Add("___ $o.CustomerID=$c.CustomerID");
                                //sbSelect.Append(s3); //SELECT $e.ID,
                                break;
                            case TypeEnum.Primitive:
                                //eg. "from o in Orders select o.OrderID"
                                string s2 = FormatMemberExpression(memberExpr);
                                selectParts.selectFieldList.Add(s2); //append '$e.ID'
                                break;
                            default:
                                throw new ArgumentOutOfRangeException("Bad MemberEnum type "+memberExpr.Type);
                        }
                        break;
                    case ExpressionType.MethodCall:
                        //for GroupBy, already handled in ProjectionData. should move here?
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("MemberInit contains unknown binding:"+member1.Expression.NodeType);
                }
            }
        }

        public static void GetClause_Assoc(SqlExpressionParts selectParts, ReverseAssociation assocParts)
        {
            //string varName = "X_";//member1.Member.Name; //eg. 'c'
            ReverseAssociation.Part givenPart = assocParts.child.propInfo!=null
                ? assocParts.child
                : assocParts.parent;

            AssociationAttribute assoc;
            //bool isAssociation = AttribHelper.IsAssociation(assocParts.child.propInfo,out assoc);
            bool isAssociation = AttribHelper.IsAssociation(givenPart.propInfo,out assoc);
            
            givenPart.assocAttrib = assoc;
            Type ourTableType = givenPart.propInfo.DeclaringType;
            givenPart.tableAttrib = AttribHelper.GetTableAttrib(ourTableType);
            bool ok;

            if(assoc.ThisKey!=null)
            {
                //we started on child side
                //find the other side (if we are a child record, find the parent record)
                ok = AttribHelper.FindReverseAssociation(assocParts.child,assocParts.parent);
            }
            else
            {
                //we started on parent side
                //find the other side (if we are a child record, find the parent record)
                ok = AttribHelper.FindReverseAssociation(assocParts.parent,assocParts.child);
            }
            if(!ok)
                throw new ApplicationException("L232 missing [Table] for type "+assocParts.child.propInfo);

            //string nickName = "$"+assocParts.parent.varName;
            string nickName = VarName.GetSqlName(assocParts.parent.varName);
            
            //GetClause(selectParts, assocParts.child.propInfo.PropertyType, nickName); 
            //GetClause(selectParts, givenPart.propInfo.PropertyType, nickName); 
            //the GetClause statement 'selects' all parts of table - 
            //- but that should be done in the projection handler, not here!

            string childTable = assocParts.child.tableAttrib.Name;
            string childCol   = assocParts.child.assocAttrib.ThisKey;
            string parentTable = assocParts.parent.tableAttrib.Name;
            string parentCol = assocParts.parent.assocAttrib.OtherKey;
            //string joinString = childTable+"."+childCol+"="+parentTable+"."+parentCol;

            // "$o.CustomerID=$c.CustomerID"
            
            //string joinString = "$"+assocParts.child.varName+"."+childCol+"=$"+assocParts.parent.varName+"."+parentCol;
            string joinString = VarName.GetSqlName(assocParts.child.varName)+"."+childCol+"="+VarName.GetSqlName(assocParts.parent.varName)+"."+parentCol;
            selectParts.joinList.Add(joinString);
        }




        /// <summary>
        /// given MemberExpression, return string '$e.Name'
        /// </summary>
        public static string FormatMemberExpression(MemberExpression memberExpr)
        {
            ParameterExpression paramExpr = (ParameterExpression)memberExpr.Expression;
            string nick = paramExpr.Name;
            //return "$" + nick+ "." + memberExpr.Member.Name;
            return VarName.GetSqlName(nick)+ "." + memberExpr.Member.Name;
        }

        /// <summary>
        /// handle the 'Select' part of query - choose fields
        /// </summary>
        public static void Main_AnalyzeLambda(SessionVars vars)
        {
            //handle '{new {c = o.Customer, o = o}}'
            SqlExpressionParts sqlParts = vars._sqlParts;
            LambdaExpression selectLambda = vars.selectExpr;
            
            //string varName = "$"+selectLambda.Parameters[0].XParam().Name; //"$e"
            string varName = VarName.GetSqlName(selectLambda.Parameters[0].XParam().Name); //"$e"
            Type sourceTableType = selectLambda.Parameters[0].XParam().Type;

            GetClause_Projected(vars, sqlParts, sourceTableType, varName, selectLambda);

            //MemberInitExpression memberInit = selectLambda.Body.XMemberInit();
            //if(memberInit!=null)
            //{
            //    Console.WriteLine("ERROR TODO handle lambda MemberInit");
            //    return;
            //}
            //MemberExpression member = selectLambda.Body.XMember();
            //if(member!=null)
            //{
            //    sqlParts.AddFrom("ZZZ");
            //    //add '$p.ProductID'
            //    string varName = member.XParam().Name;
            //    string fieldToSelect = "$"+varName+"."+member.Member.Name;
            //    sqlParts.selectFieldList.Add(member.Member.Name);
            //}
        }
    }
}
