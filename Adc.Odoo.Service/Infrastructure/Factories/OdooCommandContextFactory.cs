using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Adc.Odoo.Service.Infrastructure.Attributes;
using Adc.Odoo.Service.Infrastructure.Enums;
using Adc.Odoo.Service.Infrastructure.Extensions;
using Adc.Odoo.Service.Infrastructure.Interfaces;
using Adc.Odoo.Service.Models;

namespace Adc.Odoo.Service.Infrastructure.Factories
{
    public class OdooCommandContextFactory
    {

        public static OdooCommandContext BuildCommandContextFromExpression<T>(Expression<Func<T, bool>> conditions) where T : IOdooObject
        {
            if (!CheckCompatibility(conditions))
            {
                throw new NotImplementedException("And Or mixing is not supported");
            }

            var context = new OdooCommandContext();
            context.ParameterName = conditions.Parameters[0].Name;
            context.EntityName = GetOdooEntityName(conditions.Parameters[0]);
            var initialExpression = conditions.Body;

            if (initialExpression != null)
            {
                var counter = 9999;
                while (initialExpression != null)
                {
                    if (initialExpression.NodeType == ExpressionType.AndAlso || initialExpression.NodeType == ExpressionType.OrElse)
                    {



                        if (initialExpression.NodeType == ExpressionType.OrElse)
                        {
                            if (context.Arguments.All(x => x.CompareType != "|"))
                            {
                                context.Arguments.Add(new OdooCommandArgument { CompareType = "|", Order = counter });
                                counter--;
                            }
                        }

                        var operation = initialExpression as BinaryExpression;


                        if (operation.Right is MethodCallExpression)
                        {
                            ProcessMethodCallExpression(operation.Right as MethodCallExpression, context, counter);
                        }
                        if (operation.Right is BinaryExpression)
                        {
                            ProcessBinaryExpression(operation.Right as BinaryExpression, context, counter);
                        }
                        initialExpression = operation.Left as BinaryExpression;
                        if (initialExpression == null) initialExpression = operation.Left as MethodCallExpression;
                    }
                    else
                    {

                        if (initialExpression is MethodCallExpression)
                        {
                            ProcessMethodCallExpression(initialExpression as MethodCallExpression, context, counter);
                        }
                        if (initialExpression is BinaryExpression)
                        {
                            ProcessBinaryExpression(initialExpression as BinaryExpression, context, counter);
                        }
                        if (initialExpression.NodeType == ExpressionType.MemberAccess || initialExpression.NodeType == ExpressionType.Not)
                        {
                            ProcessMemberAccess(initialExpression, context, counter);
                        }
                        initialExpression = null;
                    }
                    counter--;
                }
            }
            return context;
        }

        /// <summary>
        /// Create a <see cref="OdooCommandContext"/> with all 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static OdooCommandContext BuildCommandContextFromEntity<T>(T entity) where T : IOdooObject
        {
            var context = new OdooCommandContext();
            context.EntityType = typeof(T);

            var attributes = (OdooMapAttribute[])context.EntityType.GetCustomAttributes(typeof(OdooMapAttribute), false);
            if (attributes.Length > 0)
            {
                context.EntityName = attributes[0].OdooName;
            }
            else
            {
                String message = string.Format("Entity {0} has no Odoo attributed defined", context.EntityType.Name);
                throw new InvalidOperationException(message);
            }
            foreach (PropertyInfo property in context.EntityType.GetProperties())
            {
                var argument = new OdooCommandArgument();
                attributes = (OdooMapAttribute[])property.GetCustomAttributes(typeof(OdooMapAttribute), false);
                if (attributes.Length > 0)
                {
                    argument.Property = attributes[0].OdooName;
                    argument.ArgumentType = attributes[0].OdooType;
                    argument.Value = property.GetValue(entity, null);
                    argument.Operation = "=";
                    argument.ReadOnly = attributes[0].ReadOnly;
                    if (argument.Value == null)
                    {
                        argument.Value = false;

                    }
                    context.Arguments.Add(argument);
                }
            }
            return context;
        }

        private static void ProcessMethodCallExpression(MethodCallExpression expression, OdooCommandContext context, int order)
        {
            var argument = new OdooCommandArgument();
            argument.Order = order;
            if (expression != null)
            {
                switch (expression.Method.Name)
                {
                    case "Contains":
                        argument.Value = GetValue(expression, context);
                        SetProperty(expression, context, argument);
                        if (argument.Value is string)
                        {
                            argument.Operation = "ilike";
                        }
                        if (argument.Value is Array)
                        {
                            argument.Operation = "in";
                        }
                        context.Arguments.Add(argument);
                        break;
                    case "StartsWith":
                        argument.Value = GetValue(expression, context) + "%";
                        SetProperty(expression, context, argument);
                        argument.Operation = "like";
                        context.Arguments.Add(argument);
                        break;
                    case "EndsWith":
                        argument.Value = "%" + GetValue(expression, context);
                        SetProperty(expression, context, argument);
                        argument.Operation = "like";
                        context.Arguments.Add(argument);
                        break;
                    case "Equals":
                        argument.Value = GetValue(expression, context);
                        SetProperty(expression, context, argument);
                        argument.Operation = "=";
                        context.Arguments.Add(argument);
                        break;
                }
            }
        }

        private static void ProcessBinaryExpression(BinaryExpression operation, OdooCommandContext context, int order)
        {
            var argument = new OdooCommandArgument();
            argument.Order = order;
            argument.Value = GetValue(operation, context);
            SetProperty(operation, context, argument);
            switch (operation.NodeType)
            {
                case ExpressionType.GreaterThan:
                    argument.Operation = ">";
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    argument.Operation = ">=";
                    break;
                case ExpressionType.LessThan:
                    argument.Operation = "<";
                    break;
                case ExpressionType.LessThanOrEqual:
                    argument.Operation = "<=";
                    break;
                case ExpressionType.Equal:
                    argument.Operation = "=";
                    break;
                case ExpressionType.NotEqual:
                    argument.Operation = "!=";

                    break;
            }
            context.Arguments.Add(argument);
        }

        private static void ProcessMemberAccess(Expression expression, OdooCommandContext context, int counter)
        {
            if (expression.Type == typeof(bool))
            {
                var argument = new OdooCommandArgument();
                argument.Order = counter;
                argument.Value = expression.NodeType != ExpressionType.Not;
                argument.Operation = "=";
                SetProperty(expression, context, argument);

                context.Arguments.Add(argument);
            }
        }

        private static void SetProperty(Expression expression, OdooCommandContext context, OdooCommandArgument argument)
        {

            if (expression == null)
            {
                return;
            }

            argument.Property = string.Empty;

            switch (expression.NodeType)
            {
                case ExpressionType.Call:
                    MethodCallExpression methodCall = expression as MethodCallExpression;
                    SetProperty(methodCall.Object, context, argument);
                    if (string.IsNullOrEmpty(argument.Property))
                    {
                        foreach (Expression item in methodCall.Arguments)
                        {
                            SetProperty(item, context, argument);
                        }
                    }
                    break;
                case ExpressionType.MemberAccess:
                    MemberExpression memberAccess = expression as MemberExpression;
                    if (memberAccess.Expression is ParameterExpression)
                    {
                        if ((memberAccess.Expression as ParameterExpression).Name.Equals(context.ParameterName))
                        {
                            argument.Property = GetOdooPropertyName(memberAccess as MemberExpression);
                            argument.ArgumentType = GetOdooPropertyType(memberAccess as MemberExpression);
                        }
                    }
                    else
                    {

                    }
                    break;
                case ExpressionType.Constant:
                    argument.Property = (expression as ConstantExpression).Value.ToString();
                    break;
                default:
                    if (expression is BinaryExpression)
                    {
                        BinaryExpression binary = expression as BinaryExpression;
                        SetProperty(binary.Left, context, argument);
                        if (string.IsNullOrEmpty(argument.Property))
                        {
                            SetProperty(binary.Right, context, argument);
                        }
                    }
                    break;
            }
        }

        private static object GetValue(Expression expression, OdooCommandContext context)
        {

            if (expression == null)
                return null;

            object res = null;
            switch (expression.NodeType)
            {
                case ExpressionType.Call:
                    var methodCall = expression as MethodCallExpression;
                    res = GetValue(methodCall.Object, context);
                    if (res == null)
                    {
                        foreach (Expression item in methodCall.Arguments)
                        {
                            var tmp = GetValue(item, context);
                            if (tmp != null) res = tmp;
                        }
                    }
                    break;
                case ExpressionType.MemberAccess:
                    var memberAccess = expression as MemberExpression;
                    if (memberAccess.Expression is ParameterExpression)
                    {
                        if (!(memberAccess.Expression as ParameterExpression).Name.Equals(context.ParameterName))
                        {
                            if (memberAccess.Type.IsGenericCollection())
                            {
                                var methodToArray = memberAccess.Type.GetMethods().FirstOrDefault(m => m.Name == "ToArray");
                                var expresion = Expression.Call(memberAccess, methodToArray);
                                var getterLambda = Expression.Lambda<Func<object>>(expresion);
                                var getter = getterLambda.Compile();
                                res = getter();
                            }
                            else if (memberAccess.Type.IsNullable())
                            {
                                string test = "hola";
                            }
                            else
                            {
                                var objectMember = Expression.Convert(memberAccess, typeof(object));
                                var getterLambda = Expression.Lambda<Func<object>>(objectMember);
                                var getter = getterLambda.Compile();
                                res = getter();
                            }
                        }
                    }
                    else
                    {
                        if (memberAccess.Type.IsGenericCollection())
                        {
                            MethodInfo methodToArray = memberAccess.Type.GetMethods().FirstOrDefault(m => m.Name == "ToArray");
                            var expresion = Expression.Call(memberAccess, methodToArray);
                            var getterLambda = Expression.Lambda<Func<object>>(expresion);
                            var getter = getterLambda.Compile();
                            res = getter();
                        }
                        else
                        {
                            if (memberAccess.Member.GetCustomAttributes(typeof(OdooMapAttribute))
                                .Any())
                            {

                            }
                            else
                            {
                                var objectMember = Expression.Convert(memberAccess, typeof(object));

                                var getterLambda = Expression.Lambda<Func<object>>(objectMember);
                                var getter = getterLambda.Compile();
                                res = getter();
                            }


                        }
                    }
                    break;
                case ExpressionType.Constant:
                    res = (expression as ConstantExpression).Value;
                    break;
                default:
                    if (expression is BinaryExpression)
                    {
                        var binary = expression as BinaryExpression;
                        res = GetValue(binary.Right, context) ?? GetValue(binary.Left, context);
                    }
                    break;
            }
            return res;
        }

        private static string GetOdooPropertyName(MemberExpression member)
        {
            var attributes = (OdooMapAttribute[])member.Member.GetCustomAttributes(typeof(OdooMapAttribute), false);
            var name = attributes.Length > 0 ? attributes[0].OdooName : member.Member.Name;

            return name;
        }

        private static bool CheckCompatibility<T>(Expression<Func<T, bool>> conditions)
        {
            var expression = conditions.Body;
            var andCounter = 0;
            var orCounter = 0;

            while (expression != null)
            {
                if (expression.NodeType == ExpressionType.OrElse || expression.NodeType == ExpressionType.AndAlso)
                {
                    if (expression.NodeType == ExpressionType.AndAlso) andCounter++;
                    if (expression.NodeType == ExpressionType.OrElse) orCounter++;

                    var operation = expression as BinaryExpression;
                    if (operation.Left.NodeType == ExpressionType.AndAlso) andCounter++;
                    if (operation.Left.NodeType == ExpressionType.OrElse) orCounter++;

                    expression = operation.Right;
                }
                else
                {
                    expression = null;
                }
            }

            if (andCounter > 0 && orCounter > 0)
                return false;

            return true;
        }

        private static OdooType GetOdooPropertyType(MemberExpression member)
        {
            var type = OdooType.Undefined;
            var attributes = (OdooMapAttribute[])member.Member.GetCustomAttributes(typeof(OdooMapAttribute), false);
            if (attributes.Length > 0)
            {
                type = attributes[0].OdooType;
            }
            return type;
        }

        private static string GetOdooEntityName(ParameterExpression member)
        {
            string name = string.Empty;
            if (member != null)
            {
                name = GetOdooEntityName(member.Type);
            }
            return name;
        }

        public static string GetOdooEntityName(Type type)
        {
            var attributes = (OdooMapAttribute[])type.GetCustomAttributes(typeof(OdooMapAttribute), false);

            var name = attributes.Length > 0 ? attributes[0].OdooName : type.Name.Replace("_", ".");
            return name;
        }

    }
}
