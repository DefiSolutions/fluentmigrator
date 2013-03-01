using FluentMigrator.Infrastructure;

namespace FluentMigrator.Runner.Versioning
{
    public interface IVersionInfo
    {
        void AddAppliedMigration(IMigrationInfo migrationInfo);
        System.Collections.Generic.IEnumerable<long> AppliedMigrations();
        bool HasAppliedMigration(IMigrationInfo migrationInfo);
        long Latest();
    }
}
