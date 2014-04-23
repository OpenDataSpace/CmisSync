//-----------------------------------------------------------------------
// <copyright file="NothingToDoSolver.cs" company="GRAU DATA AG">
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
namespace CmisSync.Lib.Sync.Solver
{
    using CmisSync.Lib.Storage;
    
    using DotCMIS.Client;
    
    /// <summary>
    /// Nothing to do solver, does nothing.
    /// </summary>
    public class NothingToDoSolver : ISolver
    {
        /// <summary>
        /// Does nothing
        /// </summary>
        /// <param name='session'>
        /// Cmis Session.
        /// </param>
        /// <param name='storage'>
        /// The Storage.
        /// </param>
        /// <param name='localFile'>
        /// Local file.
        /// </param>
        /// <param name='remoteId'>
        /// Remote identifier.
        /// </param>
        public virtual void Solve(ISession session, IMetaDataStorage storage, IFileSystemInfo localFile, IObjectId remoteId)
        {
            // No Operation Needed
        }
    }
}