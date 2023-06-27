using System.Linq.Expressions;
using System.Reflection;

namespace Hajusly.extensions;

// public static class LeftJoinExtens {
//     public static IQueryable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(
//         this IQueryable<TOuter> outer,
//         IQueryable<TInner> inner,
//         Expression<Func<TOuter, TKey>> outerKeySelector,
//         Expression<Func<TInner, TKey>> innerKeySelector,
//         Expression<Func<TOuter, TInner, TResult>> resultSelect    {
//         MethodInfo groupJoin = typeof(Queryable).GetMethods()
//                                                  .Single(m => m.ToString() == "System.Linq.IQueryable`1[TResult] GroupJoin[TOuter,TInner,TKey,TResult](System.Linq.IQueryable`1[TOuter], System.Collections.Generic.IEnumerable`1[TInner], System.Linq.Expressions.Expression`1[System.Func`2[TOuter,TKey]], System.Linq.Expressions.Expression`1[System.Func`2[TInner,TKey]], System.Linq.Expressions.Expression`1[System.Func`3[TOuter,System.Collections.Generic.IEnumerable`1[TInner],TResult]])")
//                                                  .MakeGenericMethod(typeof(TOuter), typeof(TInner), typeof(TKey), typeof(LeftJoinIntermediate<TOuter, TInner>));
//         MethodInfo selectMany = typeof(Queryable).GetMethods()
//                                                   .Single(m => m.ToString() == "System.Linq.IQueryable`1[TResult] SelectMany[TSource,TCollection,TResult](System.Linq.IQueryable`1[TSource], System.Linq.Expressions.Expression`1[System.Func`2[TSource,System.Collections.Generic.IEnumerable`1[TCollection]]], System.Linq.Expressions.Expression`1[System.Func`3[TSource,TCollection,TResult]])")
//                                                   .MakeGenericMethod(typeof(LeftJoinIntermediate<TOuter, TInner>), typeof(TInner), typeof(TResult));

//         var groupJoinResultSelector = (Expression<Func<TOuter, IEnumerable<TInner>, LeftJoinIntermediate<TOuter, TInner>>>)
//                                       ((oneOuter, manyInners) => new LeftJoinIntermediate<TOuter, TInner> { OneOuter = oneOuter, ManyInners = manyInn ers });

//         MethodCallExpression exprGroupJoin = Expression.Call(groupJoin, outer.Expression, inner.Expression, outerKeySelector, innerKeySelector, groupJoinResultSelector);

//         var selectManyCollectionSelector = (Expression<Func<LeftJoinIntermediate<TOuter, TInner>, IEnumerable<TInner>>>)
//                                            (t => t.ManyInners.DefaultIfEmpty());

//         ParameterExpression paramUser = resultSelector.Parameters.First();

//         ParameterExpression paramNew = Expression.Parameter(typof(LeftJoinIntermediate<TOuter, TInner>), "t");
//         MemberExpression propExpr = Expression.Property(paramNew, "OneOuter");

//         LambdaExpression selectManyResultSelector = Expression.Lambda(new Replacer(paramUser, propExpr).Visit(resultSelector.Body), paramNew, resultSelector.Parameters.Skip(1).First());

//         MethodCallExpression exprSelectMany = Expression.Call(selectMany, exprGroupJoin, selectManyCollectionSelector, selectManyResultSelector);

//         return outer.Provider.CreateQuery<TResult>(exprSelectMany);
//     }

//     private class LeftJoinIntermediate<TOuter, TInn    {
//         public TOuter OneOuter { get; set; }
//         public IEnumerable<TInner> ManyInners { get; set; }
//     }

//     private class Replacer : ExpressionVisi {
//         private readonly ParameterExpression _oldParam;
//         private readonly Expression _replacement;

//         public Replacer(ParameterExpression oldParam, Expression replaceme    {
//             _oldParam = oldParam;
//             _replacement = replacement;
//         }

//         public override Expression Visit(Expression e    {
//             if (exp == _oldPar    {
//                 return _replacement;
//             }

//             return base.Visit(exp);
//         }
//     }
// }