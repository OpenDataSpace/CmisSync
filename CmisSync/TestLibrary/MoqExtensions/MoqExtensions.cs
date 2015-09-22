//-----------------------------------------------------------------------
// <copyright file="MoqExtensions.cs" company="GRAU DATA AG">
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General private License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General private License for more details.
//
//   You should have received a copy of the GNU General private License
//   along with this program. If not, see http://www.gnu.org/licenses/.
//
// </copyright>
//-----------------------------------------------------------------------
using System.Collections.Generic;

using Moq;
using Moq.Language.Flow;

public static class MoqExtensions {
    #region SetupToString
    /**
     * taken from http://trycatchfail.com/blog/post/Easily-override-ToString-using-Moq.aspx
     *
     * thanks to Matt Honeycutt
     */

    /// <summary>
    /// Our dummy nested interface.
    /// </summary>
    public interface IToStringable {
        /// <summary>
        /// ToString.
        /// </summary>
        /// <returns></returns>
        string ToString();
    }

    /// <summary>
    /// Setups toString() method.
    /// </summary>
    /// <returns>The result of the toString call.</returns>
    /// <param name="mock">Mock to be setup.</param>
    /// <typeparam name="TMock">The 1st type parameter.</typeparam>
    public static ISetup<IToStringable, string> SetupToString<TMock>(this Mock<TMock> mock) where TMock : class {
        return mock.As<IToStringable>().Setup(m => m.ToString());
    }
    #endregion

    #region ReturnsInOrder
    /**
     * taken from http://haacked.com/archive/2010/11/24/moq-sequences-revisited.aspx/
     *
     * thanks to Phil Haack
     */

    /// <summary>
    /// Returns the given results in order.
    /// </summary>
    /// <param name="setup">Setup.</param>
    /// <param name="results">Results.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    /// <typeparam name="TResult">The 2nd type parameter.</typeparam>
    public static void ReturnsInOrder<T, TResult>(
        this ISetup<T, TResult> setup,
        params TResult[] results) where T : class
    {
        setup.Returns(new Queue<TResult>(results).Dequeue);
    }
    #endregion
}