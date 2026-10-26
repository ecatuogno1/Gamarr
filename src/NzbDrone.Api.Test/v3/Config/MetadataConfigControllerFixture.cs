using System.Collections.Generic;
using FluentAssertions;
using Gamarr.Api.V3.Config;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Test.Common;

namespace NzbDrone.Api.Test.v3.Config
{
    [TestFixture]
    public class MetadataConfigControllerFixture : TestBase<MetadataConfigController>
    {
        private const string Masked = "********";

        private Dictionary<string, object> _saved;

        [SetUp]
        public void Setup()
        {
            _saved = null;

            Mocker.GetMock<IConfigService>().SetupGet(s => s.IgdbClientId).Returns("client-id");
            Mocker.GetMock<IConfigService>().SetupGet(s => s.IgdbClientSecret).Returns("stored-secret");

            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.SaveConfigDictionary(It.IsAny<Dictionary<string, object>>()))
                  .Callback<Dictionary<string, object>>(d => _saved = d);

            Subject.Url = new Mock<IUrlHelper>().Object;
        }

        [Test]
        public void should_mask_client_secret()
        {
            var resource = Subject.GetConfig().Value;

            resource.IgdbClientId.Should().Be("client-id");
            resource.IgdbClientSecret.Should().Be(Masked);
        }

        [Test]
        public void should_not_mask_missing_client_secret()
        {
            Mocker.GetMock<IConfigService>().SetupGet(s => s.IgdbClientSecret).Returns(string.Empty);

            Subject.GetConfig().Value.IgdbClientSecret.Should().BeEmpty();
        }

        [Test]
        public void should_keep_stored_secret_when_masked_value_is_sent_back()
        {
            var resource = Subject.GetConfig().Value;

            Subject.SaveConfig(resource);

            _saved["IgdbClientSecret"].Should().Be("stored-secret");
        }

        [Test]
        public void should_save_new_secret()
        {
            Subject.SaveConfig(new MetadataConfigResource { Id = 1, IgdbClientId = "client-id", IgdbClientSecret = "new-secret" });

            _saved["IgdbClientSecret"].Should().Be("new-secret");
        }
    }
}
