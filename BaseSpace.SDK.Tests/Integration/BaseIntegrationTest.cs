﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Illumina.BaseSpace.SDK.Types;

namespace Illumina.BaseSpace.SDK.Tests.Integration
{
    public class BaseIntegrationTest
    {
        public IBaseSpaceClient CreateRealClient()
        {
            string apiKey = ConfigurationManager.AppSettings.Get("basespace:api-key");
            string apiSecret = ConfigurationManager.AppSettings.Get("basespace:api-secret");
            string apiUrl = ConfigurationManager.AppSettings.Get("basespace:api-url");
            string webUrl = ConfigurationManager.AppSettings.Get("basespace:web-url");
            string version = ConfigurationManager.AppSettings.Get("basespace:api-version");
            string authCode = ConfigurationManager.AppSettings.Get("basespace:api-authcode");
            var settings = new BaseSpaceClientSettings(){AppClientId = apiKey, AppClientSecret = apiSecret, BaseSpaceApiUrl = apiUrl, BaseSpaceWebsiteUrl = webUrl, Version =version};
            var client = new BaseSpaceClient(settings, new RequestOptions(apiUrl, authCode));
            return client;
        }
    }
}