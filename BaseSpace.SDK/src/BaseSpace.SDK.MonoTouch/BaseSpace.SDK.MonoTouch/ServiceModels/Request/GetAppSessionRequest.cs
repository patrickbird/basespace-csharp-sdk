﻿namespace Illumina.BaseSpace.SDK.ServiceModels
{
    public class GetAppSessionRequest
    {
        /// <summary>
        /// Get specific AppSession
        /// </summary>
        /// <param name="id">AppSession Id</param>
        public GetAppSessionRequest(string id)
        {
            Id = id;
        }

        public string Id { get; set; }
    }
}
