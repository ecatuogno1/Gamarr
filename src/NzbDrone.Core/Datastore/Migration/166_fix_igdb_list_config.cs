using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using FluentMigrator;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(166)]
    public class fix_igdb_list_config : NzbDroneMigrationBase
    {
        private readonly JsonSerializerOptions _serializerSettings;

        public fix_igdb_list_config()
        {
            _serializerSettings = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                PropertyNameCaseInsensitive = true,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            _serializerSettings.Converters.Add(new StringConverter());
        }

        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(RenameIGDBListType);
            Execute.WithConnection(RenameTraktListType);
            Execute.WithConnection(FixConfig);
        }

        private void RenameIGDBListType(IDbConnection conn, IDbTransaction tran)
        {
            var rows = conn.Query<ProviderDefinition166>($"SELECT \"Id\", \"Implementation\", \"ConfigContract\", \"Settings\" FROM \"NetImport\" WHERE \"Implementation\" = 'IGDBPopularImport'");

            var corrected = new List<ProviderDefinition166>();

            foreach (var row in rows)
            {
                var settings = JsonSerializer.Deserialize<IGDBPopularSettings165>(row.Settings, _serializerSettings);

                var newSettings = new IGDBPopularSettings166
                {
                    IGDBListType = settings.ListType,
                    FilterCriteria = new IGDBFilterSettings166
                    {
                        MinVoteAverage = settings.FilterCriteria.MinVoteAverage,
                        MinVotes = settings.FilterCriteria.MinVotes,
                        Certification = settings.FilterCriteria.Ceritification,
                        IncludeGenreIds = settings.FilterCriteria.IncludeGenreIds,
                        ExcludeGenreIds = settings.FilterCriteria.ExcludeGenreIds,
                        LanguageCode = settings.FilterCriteria.LanguageCode
                    }
                };

                corrected.Add(new ProviderDefinition166
                {
                    Id = row.Id,
                    Implementation = "IGDBPopularImport",
                    ConfigContract = "IGDBPopularSettings",
                    Settings = JsonSerializer.Serialize(newSettings, _serializerSettings)
                });
            }

            var updateSql = "UPDATE \"NetImport\" SET \"Settings\" = @Settings WHERE \"Id\" = @Id";
            conn.Execute(updateSql, corrected, transaction: tran);
        }

        private void RenameTraktListType(IDbConnection conn, IDbTransaction tran)
        {
            var rows = conn.Query<ProviderDefinition166>($"SELECT \"Id\", \"Implementation\", \"ConfigContract\", \"Settings\" FROM \"NetImport\" WHERE \"Implementation\" = 'TraktImport'");

            var corrected = new List<ProviderDefinition166>();

            foreach (var row in rows)
            {
                var settings = JsonSerializer.Deserialize<TraktSettings165>(row.Settings, _serializerSettings);

                var newSettings = new TraktSettings166
                {
                    AccessToken = settings.AccessToken,
                    RefreshToken = settings.RefreshToken,
                    Expires = settings.Expires,
                    Link = settings.Link,
                    TraktListType = settings.ListType,
                    Username = settings.Username,
                    Listname = settings.Listname,
                    Rating = settings.Rating,
                    Certification = settings.Ceritification,
                    Genres = settings.Genres,
                    Years = settings.Years,
                    Limit = settings.Limit,
                    TraktAdditionalParameters = settings.TraktAdditionalParameters,
                    SignIn = settings.SignIn
                };

                corrected.Add(new ProviderDefinition166
                {
                    Id = row.Id,
                    Implementation = "TraktImport",
                    ConfigContract = "TraktSettings",
                    Settings = JsonSerializer.Serialize(newSettings, _serializerSettings)
                });
            }

            var updateSql = "UPDATE \"NetImport\" SET \"Settings\" = @Settings WHERE \"Id\" = @Id";
            conn.Execute(updateSql, corrected, transaction: tran);
        }

        private void FixConfig(IDbConnection conn, IDbTransaction tran)
        {
            var rows = conn.Query<ProviderDefinition166>($"SELECT \"Id\", \"Implementation\", \"ConfigContract\", \"Settings\" FROM \"NetImport\" WHERE \"Implementation\" = 'IGDBImport'");

            var corrected = new List<ProviderDefinition166>();

            foreach (var row in rows)
            {
                var settings = JsonSerializer.Deserialize<IGDBSettings165>(row.Settings, _serializerSettings);

                if (settings.ListId.IsNotNullOrWhiteSpace())
                {
                    var newSettings = new IGDBListSettings166
                    {
                        ListId = settings.ListId
                    };

                    corrected.Add(new ProviderDefinition166
                    {
                        Id = row.Id,
                        Implementation = "IGDBListImport",
                        ConfigContract = "IGDBListSettings",
                        Settings = JsonSerializer.Serialize(newSettings, _serializerSettings)
                    });
                }
                else
                {
                    var newSettings = new IGDBPopularSettings166
                    {
                        IGDBListType = settings.ListType,
                        FilterCriteria = new IGDBFilterSettings166
                        {
                            MinVoteAverage = settings.MinVoteAverage,
                            MinVotes = settings.MinVotes,
                            Certification = settings.Ceritification,
                            IncludeGenreIds = settings.IncludeGenreIds,
                            ExcludeGenreIds = settings.ExcludeGenreIds,
                            LanguageCode = settings.LanguageCode
                        }
                    };

                    corrected.Add(new ProviderDefinition166
                    {
                        Id = row.Id,
                        Implementation = "IGDBPopularImport",
                        ConfigContract = "IGDBPopularSettings",
                        Settings = JsonSerializer.Serialize(newSettings, _serializerSettings)
                    });
                }
            }

            var updateSql = "UPDATE \"NetImport\" SET \"Implementation\" = @Implementation, \"ConfigContract\" = @ConfigContract, \"Settings\" = @Settings WHERE \"Id\" = @Id";
            conn.Execute(updateSql, corrected, transaction: tran);
        }

        private class ProviderDefinition166 : ModelBase
        {
            public string Implementation { get; set; }
            public string ConfigContract { get; set; }
            public string Settings { get; set; }
        }

        private class IGDBSettings165
        {
            public string Link { get; set; }
            public int ListType { get; set; }
            public string ListId { get; set; }
            public string MinVoteAverage { get; set; }
            public string MinVotes { get; set; }
            public string Ceritification { get; set; }
            public string IncludeGenreIds { get; set; }
            public string ExcludeGenreIds { get; set; }
            public int LanguageCode { get; set; }
        }

        private class IGDBListSettings166
        {
            public string ListId { get; set; }
        }

        private class IGDBPopularSettings165
        {
            public int ListType { get; set; }
            public IGDBFilterSettings165 FilterCriteria { get; set; }
        }

        private class IGDBFilterSettings165
        {
            public string MinVoteAverage { get; set; }
            public string MinVotes { get; set; }
            public string Ceritification { get; set; }
            public string IncludeGenreIds { get; set; }
            public string ExcludeGenreIds { get; set; }
            public int LanguageCode { get; set; }
        }

        private class IGDBPopularSettings166
        {
            public int IGDBListType { get; set; }
            public IGDBFilterSettings166 FilterCriteria { get; set; }
        }

        private class IGDBFilterSettings166
        {
            public string MinVoteAverage { get; set; }
            public string MinVotes { get; set; }
            public string Certification { get; set; }
            public string IncludeGenreIds { get; set; }
            public string ExcludeGenreIds { get; set; }
            public int LanguageCode { get; set; }
        }

        private class TraktSettings165
        {
            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
            public DateTime Expires { get; set; }
            public string Link { get; set; }
            public int ListType { get; set; }
            public string Username { get; set; }
            public string Listname { get; set; }
            public string Rating { get; set; }
            public string Ceritification { get; set; }
            public string Genres { get; set; }
            public string Years { get; set; }
            public int Limit { get; set; }
            public string TraktAdditionalParameters { get; set; }
            public string SignIn { get; set; }
        }

        private class TraktSettings166
        {
            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
            public DateTime Expires { get; set; }
            public string Link { get; set; }
            public int TraktListType { get; set; }
            public string Username { get; set; }
            public string Listname { get; set; }
            public string Rating { get; set; }
            public string Certification { get; set; }
            public string Genres { get; set; }
            public string Years { get; set; }
            public int Limit { get; set; }
            public string TraktAdditionalParameters { get; set; }
            public string SignIn { get; set; }
        }
    }
}
