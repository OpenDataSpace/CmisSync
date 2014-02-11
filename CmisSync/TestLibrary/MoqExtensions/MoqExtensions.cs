/**
 * taken from http://haacked.com/archive/2010/11/24/moq-sequences-revisited.aspx/
 *
 * thanks to Phil Haack
 */
using Moq.Language.Flow;
using System.Collections.Generic;
public static class MoqExtensions
{
  public static void ReturnsInOrder<T, TResult>(this ISetup<T, TResult> setup, 
    params TResult[] results) where T : class  {
    setup.Returns(new Queue<TResult>(results).Dequeue);
  }
}
