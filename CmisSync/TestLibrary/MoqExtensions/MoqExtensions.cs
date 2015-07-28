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
/**
 * taken from http://haacked.com/archive/2010/11/24/moq-sequences-revisited.aspx/
 *
 * thanks to Phil Haack
 */
using System.Collections.Generic;
using Moq.Language.Flow;

public static class MoqExtensions {
    public static void ReturnsInOrder<T, TResult>(
        this ISetup<T, TResult> setup, 
        params TResult[] results) where T : class
    {
        setup.Returns(new Queue<TResult>(results).Dequeue);
    }
}