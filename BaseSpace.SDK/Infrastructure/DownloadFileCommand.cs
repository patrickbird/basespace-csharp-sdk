﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Illumina.BaseSpace.SDK.ServiceModels;
using Illumina.BaseSpace.SDK.Types;

namespace Illumina.BaseSpace.SDK
{
	public class DownloadFileCommand
	{
		private readonly BaseSpaceClient _client;
	    private readonly IClientSettings _settings;
		private readonly Stream _stream;
        private readonly FileCompact _file;
	    private static CancellationToken Token { get; set; }
        private int ChunkSize { get; set; }
        private int MaxRetries { get; set; }

        public DownloadFileCommand(BaseSpaceClient client, FileCompact file, Stream stream, IClientSettings settings, CancellationToken token = new CancellationToken())
        {
            _client = client;
            _settings = settings;
            _stream = stream;
            _file = file;
            Token = token;
            ChunkSize = Convert.ToInt32(_settings.FileMultipartSizeThreshold);
            MaxRetries = Convert.ToInt32(_settings.RetryAttempts);
        }

        public DownloadFileCommand(BaseSpaceClient client, string fileId, Stream stream, IClientSettings settings, CancellationToken token = new CancellationToken())
        {
            _client = client;
            _settings = settings;
            _stream = stream;
            _file = _client.GetFilesInformation(new GetFileInformationRequest(fileId), null).Response;
            Token = token;
            ChunkSize = Convert.ToInt32(_settings.FileMultipartSizeThreshold);
            MaxRetries = Convert.ToInt32(_settings.RetryAttempts);
        }

		public event FileDownloadProgressChangedEventHandler FileDownloadProgressChanged;

		public void Execute()
		{

            string contentUrl = null;
            var urlExpiration = DateTime.Now;
            var sync = new object();

            var getUrl = new Func<string>(() =>
            {
                lock (sync)
                {
                    if (DateTime.Now > urlExpiration - TimeSpan.FromMinutes(10))
                    {
                        contentUrl = GetFileContentUrl(out urlExpiration);
                    }
                    return contentUrl;
                }
            });

            DownloadFile(getUrl, _stream, Convert.ToInt64(_file.Size), ChunkSize, UpdateStatusForFile, MaxRetries, LogManager.GetCurrentClassLogger());
		}

	    private void UpdateStatusForFile(int downloadedChunkCount, int totalChunkCount, int chunkSizeAdj, double span)
	    {
            OnFileDownloadProgressChanged(new FileDownloadProgressChangedEventArgs(
                                _file.Id,
                                100 * downloadedChunkCount / totalChunkCount,
                                8000.0 * chunkSizeAdj /
                                span));
	    }

	    public static void DownloadFile(Func<string> getUrl, Stream stream, long fileSize, int chunkSize,Action<int,int,int,double> updateStatus =null, int maxRetries = 3 , ILog Logger = null)
        {
            var parallelOptions = new ParallelOptions()
            {
                MaxDegreeOfParallelism = 16,
                CancellationToken = Token
            };
            var totalChunkCount = GetChunkCount(fileSize, chunkSize);
            var sync = new object();
            try
	        {
	            var downloadedChunkCount = 0;

	            Parallel.For(0, totalChunkCount, parallelOptions, partNumber =>
	                {
	                    var startDateTime = DateTime.Now;
                        var startPosition = (long)partNumber * chunkSize;
                        var chunkSizeAdj = GetChunkSize(fileSize, chunkSize, partNumber);
                        var endPosition = startPosition + chunkSizeAdj - 1;

	                    parallelOptions.CancellationToken.ThrowIfCancellationRequested();

	                    JsonWebClient.GetByteRange(getUrl, startPosition, endPosition, (b, pos, len) =>
	                                                    {
                                                            lock (sync)
                                                            {
                                                                stream.Position = pos;
                                                                stream.Write(b, 0, (int)len);
                                                            }
	                                                    }, chunkSize, maxRetries, Logger);

	                    lock (sync)
	                    {
	                        downloadedChunkCount++;
	                    }

                        if (updateStatus != null)
    	                    updateStatus(downloadedChunkCount,totalChunkCount,chunkSizeAdj,DateTime.Now.Subtract(startDateTime).TotalMilliseconds);
	                });
	        }
	        catch (OperationCanceledException)
	        {
	        }
	    }

	    protected virtual void OnFileDownloadProgressChanged(FileDownloadProgressChangedEventArgs e)
		{
			if (FileDownloadProgressChanged != null)
			{
				FileDownloadProgressChanged(this, e);
			}
		}


		private string GetFileContentUrl(out DateTime expiration)
		{
			// get the download URL
		    var response = _client.GetFileContentUrl(new FileContentRedirectMetaRequest(_file.Id));

			if (response.Response == null || response.Response.HrefContent == null)
			{
				throw new ApplicationException("Unable to get HrefContent");
			}

			if (!response.Response.SupportsRange)
			{
				throw new ApplicationException("This file does not support range queries");
			}

            expiration = response.Response.Expires;
			return response.Response.HrefContent;
		}

		private static int GetChunkCount(long fileSize, int chunkSize)
		{
            return (int)(fileSize / chunkSize + (fileSize % chunkSize > 0 ? 1 : 0));
		}

		private static int GetChunkSize(long fileSize, int chunkSize, int zeroBasedChunkNumber)
		{
			var chunkCount = GetChunkCount(fileSize, chunkSize);

			if (zeroBasedChunkNumber + 1 < chunkCount)
			{
				return chunkSize;
			}

			if (zeroBasedChunkNumber >= chunkCount)
			{
				return 0;
			}

            var remainder = (int)(fileSize % chunkSize);
			return remainder > 0 ? remainder : chunkSize;
		}
	}
}

