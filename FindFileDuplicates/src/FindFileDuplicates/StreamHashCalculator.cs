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
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace FindFileDuplicates
{
    public class StreamHashCalculator
    {
        private const int DefaultLength = 1024;

        private readonly long startPosition; 
        private readonly long length;

        /// <summary>
        /// For statistics.
        /// </summary>
        private int hashesComputedCount;

        public StreamHashCalculator(long startPosition, long length)
        {
            this.startPosition = startPosition;
            this.length = length;
        }

        public async Task<byte[]> CalculateHashAsync(Stream stream)
        {
            using (var hasher = SHA256.Create())
            {
                var subStream = new SubReadStream(
                    stream, 
                    startPosition,
                    GetLengthOrDefault(stream));

                // ComputeHash doesn't have an async counterpart,
                // so work around this with pseudo-async code. 
                // Should be refactored into truly asyncronous code.
                var hash = await Task.Run(() => hasher.ComputeHash(subStream));

                Interlocked.Increment(ref hashesComputedCount);

                return hash;
            }
        }
      
        public int GetHashesComputedCount()
        {
            return hashesComputedCount;
        }

        private long GetLengthOrDefault(Stream stream)
        {
            if (length != 0)
                return length;

            return stream.CanSeek ? stream.Length : DefaultLength;
        }

    }
}
