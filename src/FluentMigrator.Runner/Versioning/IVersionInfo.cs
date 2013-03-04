using FluentMigrator.Infrastructure;

namespace FluentMigrator.Runner.Versioning
{
    public interface IVersionInfo
    {
        void AddAppliedMigration(IMigrationInfo migrationInfo);
        System.Collections.Generic.IEnumerable<string> AppliedMigrations();
        bool HasAppliedMigration(IMigrationInfo migrationInfo);
        string Latest();
    }
}
