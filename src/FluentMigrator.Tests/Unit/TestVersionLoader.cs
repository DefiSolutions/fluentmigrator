using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Versioning;

namespace FluentMigrator.Tests.Unit
{
    public class TestVersionLoader
        : IVersionLoader
    {
        private VersionTableInfo.IVersionTableMetaData versionTableMetaData;

        public TestVersionLoader(IMigrationRunner runner, VersionTableInfo.IVersionTableMetaData versionTableMetaData)
        {
            this.versionTableMetaData = versionTableMetaData;
            this.Runner = runner;
            this.VersionInfo = new VersionInfo();
            this.Versions = new SortedList<long, IMigrationInfo>();
        }

        public bool AlreadyCreatedVersionSchema { get; set; }

        public bool AlreadyCreatedVersionTable { get; set; }

        public void DeleteVersion(long version, string featureName)
        {
            this.Versions.Remove(version);
        }

        public VersionTableInfo.IVersionTableMetaData GetVersionTableMetaData()
        {
            return versionTableMetaData;
        }

        public void LoadVersionInfo()
        {
            this.VersionInfo = new VersionInfo();

            foreach (var version in Versions)
            {
                this.VersionInfo.AddAppliedMigration(version.Value);
            }

            this.DidLoadVersionInfoGetCalled = true;
        }

        public bool DidLoadVersionInfoGetCalled { get; private set; }

        public void RemoveVersionTable()
        {
            this.DidRemoveVersionTableGetCalled = true;
        }

        public bool DidRemoveVersionTableGetCalled { get; private set; }

        public IMigrationRunner Runner { get; set; }

        public void UpdateVersionInfo(long version, string featureName)
        {
            this.Versions.Add(version, new MigrationInfo(version, TransactionBehavior.Default, new VersionMigration(null)));

            this.DidUpdateVersionInfoGetCalled = true;
        }

        public bool DidUpdateVersionInfoGetCalled { get; private set; }

        public Runner.Versioning.IVersionInfo VersionInfo { get; set; }

        public VersionTableInfo.IVersionTableMetaData VersionTableMetaData
        {
            get { return versionTableMetaData; }
        }

        public SortedList<long, IMigrationInfo> Versions { get; private set; }
    }
}
