using System.Collections.Generic;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using NzbDrone.Core.MetadataSource.Igdb.Resource;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MetadataSource.Igdb
{
    [TestFixture]
    public class IgdbReferenceConverterFixture : TestBase
    {
        [Test]
        public void should_read_bare_id()
        {
            var result = JsonConvert.DeserializeObject<List<IgdbReference>>("[19, 58]");

            result.Should().HaveCount(2);
            result[0].Id.Should().Be(19);
            result[0].Name.Should().BeNull();
        }

        [Test]
        public void should_read_expanded_object()
        {
            var result = JsonConvert.DeserializeObject<IgdbReference>("{\"id\": 2, \"status\": \"Beta\", \"name\": \"x\"}");

            result.Id.Should().Be(2);
            result.Status.Should().Be("Beta");
            result.Name.Should().Be("x");
        }

        [Test]
        public void should_read_null()
        {
            JsonConvert.DeserializeObject<IgdbReleaseDate>("{\"id\": 1, \"platform\": null}").Platform.Should().BeNull();
        }
    }
}
