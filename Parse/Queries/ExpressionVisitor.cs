using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Parse.Queries
{
    public abstract class ExpressionVisitor
    {
        protected virtual Expression Visit(Expression exp)
        {
            if (exp == null)
                return null;

            switch (exp.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return VisitUnary((UnaryExpression)exp);
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    return VisitBinary((BinaryExpression)exp);
                case ExpressionType.TypeIs:
                    return VisitTypeIs((TypeBinaryExpression)exp);
                case ExpressionType.Conditional:
                    return VisitConditional((ConditionalExpression)exp);
                case ExpressionType.Constant:
                    return VisitConstant((ConstantExpression)exp);
                case ExpressionType.Parameter:
                    return VisitParameter((ParameterExpression)exp);
                case ExpressionType.MemberAccess:
                    return VisitMemberAccess((MemberExpression)exp);
                case ExpressionType.Call:
                    return VisitMethodCall((MethodCallExpression)exp);
                case ExpressionType.Lambda:
                    return VisitLambda((LambdaExpression)exp);
                case ExpressionType.New:
                    return VisitNew((NewExpression)exp);
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    return VisitNewArray((NewArrayExpression)exp);
                case ExpressionType.Invoke:
                    return VisitInvocation((InvocationExpression)exp);
                case ExpressionType.MemberInit:
                    return VisitMemberInit((MemberInitExpression)exp);
                case ExpressionType.ListInit:
                    return VisitListInit((ListInitExpression)exp);
                default:
                    throw new Exception(string.Format("Unhandled expression type: '{0}'", exp.NodeType));
            }
        }

        protected virtual MemberBinding VisitBinding(MemberBinding binding)
        {
            switch (binding.BindingType)
            {
                case MemberBindingType.Assignment:
                    return VisitMemberAssignment((MemberAssignment)binding);
                case MemberBindingType.MemberBinding:
                    return VisitMemberMemberBinding((MemberMemberBinding)binding);
                case MemberBindingType.ListBinding:
                    return VisitMemberListBinding((MemberListBinding)binding);
                default:
                    throw new Exception(string.Format("Unhandled binding type '{0}'", binding.BindingType));
            }
        }

        protected virtual ElementInit VisitElementInitializer(ElementInit initializer)
        {
            var arguments = VisitExpressionList(initializer.Arguments);
            return arguments != initializer.Arguments ? Expression.ElementInit(initializer.AddMethod, arguments) : initializer;
        }

        protected virtual Expression VisitUnary(UnaryExpression u)
        {
            var operand = Visit(u.Operand);
            return operand != u.Operand ? Expression.MakeUnary(u.NodeType, operand, u.Type, u.Method) : u;
        }

        protected virtual Expression VisitBinary(BinaryExpression b)
        {
            var left = Visit(b.Left);
            var right = Visit(b.Right);
            var conversion = Visit(b.Conversion);
            if (left != b.Left || right != b.Right || conversion != b.Conversion)
            {
                return b.NodeType == ExpressionType.Coalesce ? Expression.Coalesce(left, right, conversion as LambdaExpression) : Expression.MakeBinary(b.NodeType, left, right, b.IsLiftedToNull, b.Method);
            }
            return b;
        }

        protected virtual Expression VisitTypeIs(TypeBinaryExpression b)
        {
            var expr = Visit(b.Expression);
            return expr != b.Expression ? Expression.TypeIs(expr, b.TypeOperand) : b;
        }

        protected virtual Expression VisitConstant(ConstantExpression c)
        {
            return c;
        }

        protected virtual Expression VisitConditional(ConditionalExpression c)
        {
            var test = Visit(c.Test);
            var ifTrue = Visit(c.IfTrue);
            var ifFalse = Visit(c.IfFalse);
            return test != c.Test || ifTrue != c.IfTrue || ifFalse != c.IfFalse ? Expression.Condition(test, ifTrue, ifFalse) : c;
        }

        protected virtual Expression VisitParameter(ParameterExpression p)
        {
            return p;
        }

        protected virtual Expression VisitMemberAccess(MemberExpression m)
        {
            var exp = Visit(m.Expression);
            return exp != m.Expression ? Expression.MakeMemberAccess(exp, m.Member) : m;
        }

        protected virtual Expression VisitMethodCall(MethodCallExpression m)
        {
            var obj = Visit(m.Object);
            var args = VisitExpressionList(m.Arguments);
            return obj != m.Object || args != m.Arguments ? Expression.Call(obj, m.Method, args) : m;
        }

        protected virtual ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            List<Expression> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                var p = Visit(original[i]);
                if (list != null)
                {
                    list.Add(p);
                }
                else if (p != original[i])
                {
                    list = new List<Expression>(n);
                    for (var j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(p);
                }
            }
            return list != null ? list.AsReadOnly() : original;
        }

        protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
        {
            var e = Visit(assignment.Expression);
            return e != assignment.Expression ? Expression.Bind(assignment.Member, e) : assignment;
        }

        protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
        {
            var bindings = VisitBindingList(binding.Bindings);
            return !Equals(bindings, binding.Bindings) ? Expression.MemberBind(binding.Member, bindings) : binding;
        }

        protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding)
        {
            var initializers = VisitElementInitializerList(binding.Initializers);
            return !Equals(initializers, binding.Initializers) ? Expression.ListBind(binding.Member, initializers) : binding;
        }

        protected virtual IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
        {
            List<MemberBinding> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                var b = VisitBinding(original[i]);
                if (list != null)
                {
                    list.Add(b);
                }
                else if (b != original[i])
                {
                    list = new List<MemberBinding>(n);
                    for (var j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(b);
                }
            }
            return list != null ? (IEnumerable<MemberBinding>)list : original;
        }

        protected virtual IEnumerable<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
        {
            List<ElementInit> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                var init = VisitElementInitializer(original[i]);
                if (list != null)
                {
                    list.Add(init);
                }
                else if (init != original[i])
                {
                    list = new List<ElementInit>(n);
                    for (var j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(init);
                }
            }
            return list != null ? (IEnumerable<ElementInit>)list : original;
        }

        protected virtual Expression VisitLambda(LambdaExpression lambda)
        {
            var body = Visit(lambda.Body);
            return body != lambda.Body ? Expression.Lambda(lambda.Type, body, lambda.Parameters) : lambda;
        }

        protected virtual NewExpression VisitNew(NewExpression nex)
        {
            var args = VisitExpressionList(nex.Arguments);
            return args != nex.Arguments ? (Expression.New(nex.Constructor, args, nex.Members)) : nex;
        }

        protected virtual Expression VisitMemberInit(MemberInitExpression init)
        {
            var n = VisitNew(init.NewExpression);
            var bindings = VisitBindingList(init.Bindings);
            return n != init.NewExpression || !Equals(bindings, init.Bindings) ? Expression.MemberInit(n, bindings) : init;
        }

        protected virtual Expression VisitListInit(ListInitExpression init)
        {
            var n = VisitNew(init.NewExpression);
            var initializers = VisitElementInitializerList(init.Initializers);
            return n != init.NewExpression || !Equals(initializers, init.Initializers) ? Expression.ListInit(n, initializers) : init;
        }

        protected virtual Expression VisitNewArray(NewArrayExpression na)
        {
            var exprs = VisitExpressionList(na.Expressions);
            if (exprs != na.Expressions)
            {
                return na.NodeType == ExpressionType.NewArrayInit ? Expression.NewArrayInit(na.Type.GetElementType(), exprs) : Expression.NewArrayBounds(na.Type.GetElementType(), exprs);
            }
            return na;
        }

        protected virtual Expression VisitInvocation(InvocationExpression iv)
        {
            var args = VisitExpressionList(iv.Arguments);
            var expr = Visit(iv.Expression);
            return args != iv.Arguments || expr != iv.Expression ? Expression.Invoke(expr, args) : iv;
        }
    }
}