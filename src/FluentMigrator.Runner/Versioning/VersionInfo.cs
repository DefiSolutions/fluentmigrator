#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System.Collections.Generic;
using System.Linq;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Runner.Versioning
{
    public class VersionInfo : IVersionInfo
    {
        private IList<IMigrationInfo> _versionsApplied = new List<IMigrationInfo>();
        private IList<string> complexVersionsApplied = new List<string>();

        public long Latest()
        {
            var max = _versionsApplied.Any() ? _versionsApplied.Max(x => x.Version) : 0;
            return max;
        }

        public void AddAppliedMigration(IMigrationInfo migration)
        {
            _versionsApplied.Add(migration);
            complexVersionsApplied.Add(migration.ComplexVersion);
        }

        public bool HasAppliedMigration(IMigrationInfo migration)
        {
            return complexVersionsApplied.Contains(migration.ComplexVersion);
        }

        public IEnumerable<long> AppliedMigrations()
        {
            return _versionsApplied.OrderByDescending(x => x.Version).Select(x => x.Version).AsEnumerable();
        }
    }
}
