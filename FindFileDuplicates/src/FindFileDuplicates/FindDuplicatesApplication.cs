/*
 * Copyright (c) 2026 Andriy Savin
 *
 * This code is licensed under the MIT License.
 * See the LICENSE file in the repository root for full license text.
 * 
 * Attribution is appreciated when reusing this code.
 * 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FindFileDuplicates
{
    internal class FindDuplicatesApplication
    {
        private readonly FileIndex fileIndex;
        private readonly StreamHashCalculator streamHashCalculator;

        public FindDuplicatesApplication(
            long fileOffsetForComparing,
            long fileLengthForComparing)
        {
            streamHashCalculator = new StreamHashCalculator(
                fileOffsetForComparing,
                fileLengthForComparing);

            fileIndex = new FileIndex(streamHashCalculator);
        }

        public Task<int> BuildFilesIndexAsync(string pathToSearch, IProgress<int> progress)
        {
            // Initiate file system enumeration on a thread pool,
            // since such operations, while being lazy, execute
            // syncronously, and may block the UI thread.
            // Once started on the thread pool, await constructs
            // will resume on the pool all subsequent operations.
            return Task.Run(async () =>
            {
                var files = EnumerateFilesRecursively(pathToSearch);

                return await AddFilesToIndexAsync(files, progress);
            });
        }

        public IEnumerable<FileInfo[]> EnumerateDuplicates()
        {
            return fileIndex.EnumerateDuplicates();
        }

        public int GetHashesComputedCount()
        {
            return streamHashCalculator.GetHashesComputedCount();
        }

        private async Task<int> AddFilesToIndexAsync(IEnumerable<string> files, IProgress<int> progress)
        {
            int filesAdded = 0;

            foreach (var file in files)
            {
                var fileInfo = TryGetFileInfo(file);

                if (fileInfo == null)
                    continue;

                await fileIndex.AddFileAsync(fileInfo);

                if (filesAdded % 100 == 0)
                    progress.Report(filesAdded);

                filesAdded++;
            }

            progress.Report(filesAdded);

            return filesAdded;
        }

        private FileInfo TryGetFileInfo(string path)
        {
            try
            {
                return new FileInfo(path);
            }
            catch (PathTooLongException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }

            return null;
        }

        private IEnumerable<string> EnumerateFilesRecursively(string pathToSearch)
        {
            try
            {
                var localFiles = Directory.EnumerateFiles(pathToSearch);

                var localDirs = Directory.EnumerateDirectories(pathToSearch);

                var nestedFiles = localDirs.SelectMany(dir => EnumerateFilesRecursively(dir));

                return localFiles.Concat(nestedFiles);
            }
            catch (PathTooLongException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }

            return Enumerable.Empty<string>();
        }
    }
}
