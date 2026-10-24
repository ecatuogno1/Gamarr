using System;
using System.Linq;
using System.Xml;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.IndexerTests.NewznabTests
{
    [TestFixture]
    public class NewznabCapabilitiesProviderFixture : CoreTest<NewznabCapabilitiesProvider>
    {
        private NewznabSettings _settings;
        private string _caps;

        [SetUp]
        public void SetUp()
        {
            _settings = new NewznabSettings()
            {
                BaseUrl = "http://indxer.local"
            };

            _caps = ReadAllText("Files/Indexers/Newznab/newznab_caps.xml");
        }

        private void GivenCapsResponse(string caps)
        {
            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Get(It.IsAny<HttpRequest>()))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), caps));
        }

        [Test]
        public void should_not_request_same_caps_twice()
        {
            GivenCapsResponse(_caps);

            Subject.GetCapabilities(_settings);
            Subject.GetCapabilities(_settings);

            Mocker.GetMock<IHttpClient>()
                .Verify(o => o.Get(It.IsAny<HttpRequest>()), Times.Once());
        }

        [Test]
        public void should_report_pagesize()
        {
            GivenCapsResponse(_caps);

            var caps = Subject.GetCapabilities(_settings);

            caps.DefaultPageSize.Should().Be(25);
            caps.MaxPageSize.Should().Be(60);
        }

        [Test]
        public void should_use_default_pagesize_if_missing()
        {
            GivenCapsResponse(_caps.Replace("<limits", "<abclimits"));

            var caps = Subject.GetCapabilities(_settings);

            caps.DefaultPageSize.Should().Be(100);
            caps.MaxPageSize.Should().Be(100);
        }

        [Test]
        public void should_throw_if_failed_to_get()
        {
            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Get(It.IsAny<HttpRequest>()))
                .Throws<Exception>();

            Assert.Throws<Exception>(() => Subject.GetCapabilities(_settings));
        }

        [Test]
        public void should_throw_if_xml_invalid()
        {
            GivenCapsResponse(_caps.Replace("<limits", "<>"));

            Assert.Throws<XmlException>(() => Subject.GetCapabilities(_settings));
        }

        [Test]
        public void should_not_throw_on_xml_data_unexpected()
        {
            GivenCapsResponse(_caps.Replace("1080", "asdf"));

            var result = Subject.GetCapabilities(_settings);

            result.Should().NotBeNull();

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_use_default_searchengine_if_missing()
        {
            GivenCapsResponse(_caps);

            var caps = Subject.GetCapabilities(_settings);

            caps.TextSearchEngine.Should().Be("sphinx");
        }

        [Test]
        public void should_use_specified_searchengine()
        {
            GivenCapsResponse(_caps.Replace("<search ", "<search searchEngine=\"raw\" "));

            var caps = Subject.GetCapabilities(_settings);

            caps.TextSearchEngine.Should().Be("raw");
        }

        [Test]
        public void should_parse_basic_search_params()
        {
            GivenCapsResponse(_caps);

            var caps = Subject.GetCapabilities(_settings);

            caps.SupportedSearchParameters.Should().BeEquivalentTo(new[] { "q" });
        }

        [Test]
        public void should_not_support_search_if_basic_search_unavailable()
        {
            GivenCapsResponse(_caps.Replace("<search available=\"yes\"", "<search available=\"no\""));

            var caps = Subject.GetCapabilities(_settings);

            caps.SupportedSearchParameters.Should().BeNull();
        }

        [Test]
        public void should_parse_game_categories()
        {
            GivenCapsResponse(_caps);

            var caps = Subject.GetCapabilities(_settings);

            var console = caps.Categories.Single(c => c.Id == 1000);
            console.Name.Should().Be("Console");
            console.Subcategories.Select(c => c.Id).Should().Contain(new[] { 1030, 1080, 1180 });

            caps.Categories.Single(c => c.Id == 4000).Subcategories.Should().Contain(c => c.Id == 4050 && c.Name == "Games");
        }
    }
}
