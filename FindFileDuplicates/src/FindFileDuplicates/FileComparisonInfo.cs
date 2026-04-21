/*
 * Copyright (c) 2026 Andriy Savin
 *
 * This code is licensed under the MIT License.
 * See the LICENSE file in the repository root for full license text.
 * 
 * Attribution is appreciated when reusing this code.
 * 
 */

using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FindFileDuplicates
{
    internal class FileComparisonInfo
    {
        private readonly long fileLength;
        private readonly AsyncLazy<byte[]> fileHash;

        public long FileLength 
        {
            get { return fileLength; }
        }

        public static FileComparisonInfo FromFileInfo(
            FileInfo fileInfo, 
            StreamHashCalculator hashCalculator)
        {
            return new FileComparisonInfo(
                fileInfo.Length,
                new AsyncLazy<byte[]>(() => GetFileHash(fileInfo, hashCalculator)));
        }

        private FileComparisonInfo(long fileLength, AsyncLazy<byte[]> fileHash)
        {
            this.fileLength = fileLength;
            this.fileHash = fileHash;
        }

        public async Task<bool> FileSameAsAsync(FileComparisonInfo other)
        {
            return
                HasSameLengthAs(other) &&
                await HasSameHashAsAsync(other);
        }

        private async Task<bool> HasSameHashAsAsync(FileComparisonInfo other)
        {
            // A bit of parallelizm.
            var values = await Task.WhenAll(fileHash.Value, other.fileHash.Value);

            var thisHash = values[0];
            var otherHash = values[1];

            return thisHash.SequenceEqual(otherHash);
        }

        private bool HasSameLengthAs(FileComparisonInfo other)
        {
            return fileLength == other.fileLength;
        }

        private static async Task<byte[]> GetFileHash(
            FileInfo fileInfo, 
            StreamHashCalculator hashCalculator)
        {

            using (var fileStream = new FileStream(
                fileInfo.FullName,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                4 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan)) 
            {
                return await hashCalculator.CalculateHashAsync(fileStream);
            }
        }
    }
}
