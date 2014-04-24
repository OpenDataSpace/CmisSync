//-----------------------------------------------------------------------
// <copyright file="LocalFolderLoader.cs" company="GRAU DATA AG">
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CmisSync.Lib;

namespace CmisSync.CmisTree
{
    /// <summary>
    /// Loads a local folder hierarchie as Nodes
    /// </summary>
    public static class LocalFolderLoader
    {
        /// <summary>
        /// Loads all sub folder from the given path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parent">Parent Node for the new list of Nodes</param>
        /// <returns></returns>
        public static List<Node> CreateNodesFromLocalFolder(string path, Node parent)
        {
            string[] subdirs = Directory.GetDirectories(path);
            List<Node> results = new List<Node>();
            foreach (string subdir in subdirs)
            {
                Folder f = new Folder()
                {
                    Name = new DirectoryInfo(subdir).Name,
                    Parent = parent,
                    LocationType = Node.NodeLocationType.LOCAL
                };
                if(f.Parent.Path.EndsWith("/"))
                    f.Path = f.Parent.Path + f.Name ;
                else
                    f.Path = f.Parent.Path + "/" + f.Name ;
                f.IsIllegalFileNameInPath = CmisSync.Lib.Utils.IsInvalidFolderName(f.Name, ConfigManager.CurrentConfig.IgnoreFolderNames);
                List<Node> children = CreateNodesFromLocalFolder(subdir, f);
                foreach (Node child in children)
                    f.Children.Add(child);
                results.Add(f);
            }
            return results;
        }

        /// <summary>
        /// Merges the sub folder of the given path to the given Repo Node
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="localPath"></param>
        public static void AddLocalFolderToRootNode(RootFolder repo, string localPath)
        {
            List<Node> children = CreateNodesFromLocalFolder(localPath, repo);
            AsyncNodeLoader.MergeFolderTrees(repo, children);
        }
    }
}
