//-----------------------------------------------------------------------
// <copyright file="AggregatingIconLoader.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.UiUtils {
    using System;

    /// <summary>
    /// Icon loader which takes two or more IIconLoader which will be called from first one until a non null result is returned.
    /// </summary>
    public class AggregatingIconLoader : IIconLoader {
        private IIconLoader primaryLoader;
        private IIconLoader fallbackLoader;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.UiUtils.AggregatingIconLoader"/> class.
        /// </summary>
        /// <param name="primaryLoader">Primary loader.</param>
        /// <param name="fallbackLoader">Fallback loader.</param>
        /// <param name="moreFallBackLoader">More fall back loader.</param>
        public AggregatingIconLoader(IIconLoader primaryLoader, IIconLoader fallbackLoader, params IIconLoader[] moreFallBackLoader) {
            if (primaryLoader == null) {
                throw new ArgumentNullException("Given primary loader is null");
            }

            if (fallbackLoader == null) {
                throw new ArgumentNullException("Given fallback loader is null");
            }

            this.primaryLoader = primaryLoader;

            if (moreFallBackLoader.Length > 0) {
                IIconLoader[] moreLoader = new IIconLoader[moreFallBackLoader.Length - 1];
                Array.Copy(moreFallBackLoader, moreLoader, moreLoader.Length);
                this.fallbackLoader = new AggregatingIconLoader(fallbackLoader, moreFallBackLoader[0], moreLoader);
            } else {
                this.fallbackLoader = fallbackLoader;
            }
        }

        /// <summary>
        /// Gets the path of the given icon, or null if this loader is not able to provide a path to the requested icon.
        /// It will call the first loader and if null is returned, the next are called.
        /// </summary>
        /// <returns>The path to this given icon or null if no path can be returned.</returns>
        /// <param name="icon">Requested Icon.</param>
        public string GetPathOf(Icons icon) {
            return this.primaryLoader.GetPathOf(icon) ?? this.fallbackLoader.GetPathOf(icon);
        }
    }
}