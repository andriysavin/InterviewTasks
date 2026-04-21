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
using System.Text;
using System.Threading.Tasks;

namespace FindFileDuplicates
{
    internal class FileIndex
    {
        private class FileIndexEntry
        {
            public FileComparisonInfo ComparisonInfo { get; private set; }
            public List<FileInfo> Files { get; private set; }

            public FileIndexEntry(FileComparisonInfo comparisonInfo)
            {
                ComparisonInfo = comparisonInfo;
                Files = new List<FileInfo>();
            }
        }

        private readonly StreamHashCalculator hashCalculator;
        private readonly List<FileIndexEntry> entries = new List<FileIndexEntry>();
        private readonly Dictionary<long, List<FileIndexEntry>> entriesByFileSize =
            new Dictionary<long, List<FileIndexEntry>>();

        public FileIndex(StreamHashCalculator hashCalculator)
        {
            this.hashCalculator = hashCalculator;
        }

        public async Task AddFileAsync(FileInfo fileInfo)
        {
            var comparisonInfo =
                FileComparisonInfo.FromFileInfo(fileInfo, hashCalculator);

            var entry = await TryFindExistingEntryAsync(comparisonInfo);

            if (entry == null)
            {
                entry = AddEntry(comparisonInfo);
            }

            entry.Files.Add(fileInfo);
        }

        private FileIndexEntry AddEntry(FileComparisonInfo comparisonInfo)
        {
            var entry = new FileIndexEntry(comparisonInfo);

            entries.Add(entry);

            List<FileIndexEntry> sameLengthFiles;

            if (!entriesByFileSize.TryGetValue(
                entry.ComparisonInfo.FileLength, out sameLengthFiles))
            {
                sameLengthFiles = new List<FileIndexEntry>();

                entriesByFileSize.Add(
                    entry.ComparisonInfo.FileLength,
                    sameLengthFiles);
            }

            sameLengthFiles.Add(entry);

            return entry;
        }

        public void Clear()
        {
            entries.Clear();
            entriesByFileSize.Clear();
        }

        public IEnumerable<FileInfo[]> EnumerateDuplicates()
        {
            var duplicates =
                from entry in entries
                where entry.Files.Count > 1
                select entry.Files.ToArray();

            return duplicates;
        }

        private async Task<FileIndexEntry> TryFindExistingEntryAsync(FileComparisonInfo comparisonInfo)
        {
            List<FileIndexEntry> sameLengthFiles;

            if (entriesByFileSize.TryGetValue(
               comparisonInfo.FileLength, out sameLengthFiles))
            {
                foreach (var entry in sameLengthFiles)
                {
                    try
                    {
                        if (await entry.ComparisonInfo.FileSameAsAsync(comparisonInfo))
                            return entry;
                    }
                    // Skip files which can't be
                    // opened (for hash calculation).
                    catch(UnauthorizedAccessException)
                    {
                    }
                    catch (IOException)
                    {
                    }
                }
            }

            return null;
        }
    }

}
