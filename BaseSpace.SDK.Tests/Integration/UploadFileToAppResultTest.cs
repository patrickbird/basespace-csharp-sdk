﻿using Illumina.BaseSpace.SDK.ServiceModels;
using Illumina.BaseSpace.SDK.Tests.Helpers;
using Illumina.BaseSpace.SDK.Types;
using Xunit;
using File = System.IO.File;

namespace Illumina.BaseSpace.SDK.Tests.Integration
{
    public class UploadFileToAppResultTest : BaseIntegrationTest
    {
        [Fact]
        public void CanUploadSmallFileToAppResult_SinglePart()
        {
            var project = TestHelpers.CreateRandomTestProject(Client);
            var appResult = TestHelpers.CreateRandomTestAppResult(Client, project);
            var file = File.Create(string.Format("UnitTestFile_{0}", appResult.Id));
            file.Write(new byte[10],0,10);
            file.Close();
            var response = Client.UploadFileToAppResult(new UploadFileToAppResultRequest(appResult.Id, file.Name), null);
            Assert.NotNull(response);
            Assert.True(response.UploadStatus == FileUploadStatus.complete);
        }

        [Fact]
        public void CanUploadBigFileToAppResult_MultiPart()
        {
            var project = TestHelpers.CreateRandomTestProject(Client);
            var appResult = TestHelpers.CreateRandomTestAppResult(Client, project);
            var file = File.Create(string.Format("UnitTestFile_{0}", appResult.Id));
            file.Write(new byte[31000000], 0, 31000000);
            file.Close();
            var response = Client.UploadFileToAppResult(new UploadFileToAppResultRequest(appResult.Id, file.Name), null);
            Assert.NotNull(response);
            Assert.True(response.UploadStatus == FileUploadStatus.complete);
        }
    }
}

