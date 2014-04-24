//-----------------------------------------------------------------------
// <copyright file="LocalFilesystemActivityGenerator.cs" company="GRAU DATA AG">
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
using System.Linq;
using System.Text;
using System.IO;

namespace TestLibrary
{
    class LocalFilesystemActivityGenerator
    {
        public static int id = 1;

        public static void CreateDirectoriesAndFiles(string path)
        {
            try
            {
                CreateRandomFile(path, 3);
                CreateRandomFile(path, 3);
                CreateRandomFile(path, 3);
                CreateRandomFile(path, 3);
                CreateRandomFile(path, 3);
                string path1 = Path.Combine(path, "dir1");
                if (!Directory.Exists(path1))
                {
                    Directory.CreateDirectory(path1);
                }
                CreateRandomFile(path1, 3);
                CreateRandomFile(path1, 3);
                CreateRandomFile(path1, 3);
                CreateRandomFile(path1, 3);
                CreateRandomFile(path1, 3);
                string path2 = Path.Combine(path1, "dir2");
                if (!Directory.Exists(path2))
                {
                    Directory.CreateDirectory(path2);
                }
                CreateRandomFile(path2, 3);
            }
            catch (IOException ex)
            {
                Console.WriteLine("Exception on testing side, ignoring " + ex);
            }
        }

        public static void CreateRandomFile(string path, int maxSizeInKb)
        {
            Random rng = new Random();
            int sizeInKb = 1 + rng.Next(maxSizeInKb);
            CreateFile(path, sizeInKb);
        }

        public static string GetNextFileName()
        {
            string filename = "file_" + id.ToString() + ".bin";
            return filename;
        }

        public static void CreateFile(string path, int sizeInKb)
        {
            Random rng = new Random();
            string filename = GetNextFileName();
            ++ id;
            byte[] data = new byte[1024];

            try
            {
                using (FileStream stream = File.OpenWrite(Path.Combine(path, filename)))
                {
                    // Write random data
                    for (int i = 0; i < sizeInKb; i++)
                    {
                        rng.NextBytes(data);
                        stream.Write(data, 0, data.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception on testing side, ignoring " + ex);
            }
        }
    }
}
