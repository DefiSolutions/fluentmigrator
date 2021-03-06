﻿using System;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using NUnit.Should;
using System.Data;
using FluentMigrator.Model;
using FluentMigrator.Expressions;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer
{
    public class SqlServer2005AlterTableTests : GeneratorTestBase
    {
        protected SqlServer2005Generator generator;

        [SetUp]
        public void Setup()
        {
            generator = new SqlServer2005Generator();
        }

        [Test]
        public void CanAddColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD [TestColumn1] NVARCHAR(5) NOT NULL");
        }

        [Test]
        public void CanAddDecimalColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD [TestColumn1] DECIMAL(19,2) NOT NULL");
        }

        [Test]
        public void CanRenameTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("sp_rename '[dbo].[TestTable1]', 'TestTable2'");
        }

        [Test]
        public void CanRenameColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("sp_rename '[dbo].[TestTable1].[TestColumn1]', 'TestColumn2'");
        }

        [Test]
        public void CanAlterColumnWithDefaultSchema()
        {
            //TODO: This will fail if there are any keys attached 
            var expression = GeneratorTestHelper.GetAlterColumnExpression();

            var sql = generator.Generate(expression);

            sql.ShouldBe("ALTER TABLE [dbo].[TestTable1] ALTER COLUMN [TestColumn1] NVARCHAR(20) NOT NULL");
        }

        [Test]
        public void CanAlterDefaultConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetAlterDefaultConstraintExpression();
            var sql = generator.Generate(expression);

            string expected = "DECLARE @default sysname, @sql nvarchar(max);" + Environment.NewLine + Environment.NewLine +
            "-- get name of default constraint" + Environment.NewLine +
            "SELECT @default = name" + Environment.NewLine +
            "FROM sys.default_constraints" + Environment.NewLine +
            "WHERE parent_object_id = object_id('[dbo].[TestTable1]')" + Environment.NewLine +
            "AND type = 'D'" + Environment.NewLine +
            "AND parent_column_id = (" + Environment.NewLine +
            "SELECT column_id" + Environment.NewLine +
            "FROM sys.columns" + Environment.NewLine +
            "WHERE object_id = object_id('[dbo].[TestTable1]')" + Environment.NewLine +
            "AND name = 'TestColumn1'" + Environment.NewLine +
            ");" + Environment.NewLine + Environment.NewLine +
            "-- create alter table command to drop constraint as string and run it" + Environment.NewLine +
            "SET @sql = N'ALTER TABLE [dbo].[TestTable1] DROP CONSTRAINT ' + @default;" + Environment.NewLine +
            "EXEC sp_executesql @sql;" + Environment.NewLine + Environment.NewLine +
            "-- create alter table command to create new default constraint as string and run it" + Environment.NewLine +
            "ALTER TABLE [dbo].[TestTable1] WITH NOCHECK ADD CONSTRAINT [DF_TestTable1_TestColumn1] DEFAULT(1) FOR [TestColumn1];";

            sql.ShouldBe(expected);
        }

        [Test]
        public void CanAlterDefaultConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetAlterDefaultConstraintExpression();
            expression.SchemaName = "TestSchema";
            var sql = generator.Generate(expression);

            string expected = "DECLARE @default sysname, @sql nvarchar(max);" + Environment.NewLine + Environment.NewLine +
            "-- get name of default constraint" + Environment.NewLine +
            "SELECT @default = name" + Environment.NewLine +
            "FROM sys.default_constraints" + Environment.NewLine +
            "WHERE parent_object_id = object_id('[TestSchema].[TestTable1]')" + Environment.NewLine +
            "AND type = 'D'" + Environment.NewLine +
            "AND parent_column_id = (" + Environment.NewLine +
            "SELECT column_id" + Environment.NewLine +
            "FROM sys.columns" + Environment.NewLine +
            "WHERE object_id = object_id('[TestSchema].[TestTable1]')" + Environment.NewLine +
            "AND name = 'TestColumn1'" + Environment.NewLine +
            ");" + Environment.NewLine + Environment.NewLine +
            "-- create alter table command to drop constraint as string and run it" + Environment.NewLine +
            "SET @sql = N'ALTER TABLE [TestSchema].[TestTable1] DROP CONSTRAINT ' + @default;" + Environment.NewLine +
            "EXEC sp_executesql @sql;" + Environment.NewLine + Environment.NewLine +
            "-- create alter table command to create new default constraint as string and run it" + Environment.NewLine +
            "ALTER TABLE [TestSchema].[TestTable1] WITH NOCHECK ADD CONSTRAINT [DF_TestTable1_TestColumn1] DEFAULT(1) FOR [TestColumn1];";

            sql.ShouldBe(expected);
        }

        [Test]
        public void CanCreateForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [FK_Test] FOREIGN KEY ([TestColumn1]) REFERENCES [dbo].[TestTable2] ([TestColumn2])");

        }

        [Test]
        public void CanCreateMulitColumnForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnForeignKeyExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [FK_Test] FOREIGN KEY ([TestColumn1], [TestColumn3]) REFERENCES [dbo].[TestTable2] ([TestColumn2], [TestColumn4])");

        }

        [TestCase(Rule.SetDefault, "SET DEFAULT"), TestCase(Rule.SetNull, "SET NULL"), TestCase(Rule.Cascade, "CASCADE")]
        public void CanCreateForeignKeyWithOnUpdateOptions(Rule rule, string output) 
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.OnUpdate = rule;
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                string.Format("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [FK_Test] FOREIGN KEY ([TestColumn1]) REFERENCES [dbo].[TestTable2] ([TestColumn2]) ON UPDATE {0}", output));
        }

        [TestCase(Rule.SetDefault, "SET DEFAULT"), TestCase(Rule.SetNull, "SET NULL"), TestCase(Rule.Cascade, "CASCADE")]
        public void CanCreateForeignKeyWithOnDeleteOptions(Rule rule, string output) 
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.OnDelete = rule;
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                string.Format("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [FK_Test] FOREIGN KEY ([TestColumn1]) REFERENCES [dbo].[TestTable2] ([TestColumn2]) ON DELETE {0}", output));
        }

        [Test]
        public void CanCreateForeignKeyWithOnDeleteAndOnUpdateOptions() 
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.OnDelete = Rule.Cascade;
            expression.ForeignKey.OnUpdate = Rule.SetDefault;
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [FK_Test] FOREIGN KEY ([TestColumn1]) REFERENCES [dbo].[TestTable2] ([TestColumn2]) ON DELETE CASCADE ON UPDATE SET DEFAULT");
        }

        [Test]
        public void CanCreateXmlColumnWithDefaultSchema()
        {
            var expression = new CreateColumnExpression();
            expression.TableName = "TestTable1";

            expression.Column = new ColumnDefinition();
            expression.Column.Name = "TestColumn1";
            expression.Column.Type = DbType.Xml;

            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD [TestColumn1] XML NOT NULL");
        }

        public void CanCreateAutoIncrementColumnWithDefaultSchema()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void CanAddColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            expression.SchemaName = "TestSchema";
            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD [TestColumn1] NVARCHAR(5) NOT NULL");
        }

        [Test]
        public void CanAddDecimalColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();
            expression.SchemaName = "TestSchema";
            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD [TestColumn1] DECIMAL(19,2) NOT NULL");
        }

        [Test]
        public void CanRenameTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();
            expression.SchemaName = "TestSchema";
            var sql = generator.Generate(expression);
            sql.ShouldBe("sp_rename '[TestSchema].[TestTable1]', 'TestTable2'");
        }

        [Test]
        public void CanRenameColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            expression.SchemaName = "TestSchema";
            var sql = generator.Generate(expression);
            sql.ShouldBe("sp_rename '[TestSchema].[TestTable1].[TestColumn1]', 'TestColumn2'");
        }

        [Test]
        public void CanAlterColumnWithCustomSchema()
        {
            //TODO: This will fail if there are any keys attached 
            var expression = GeneratorTestHelper.GetAlterColumnExpression();
            expression.SchemaName = "TestSchema";

            var sql = generator.Generate(expression);

            sql.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ALTER COLUMN [TestColumn1] NVARCHAR(20) NOT NULL");
        }

        [Test]
        public void CanCreateForeignKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";
            expression.ForeignKey.PrimaryTableSchema = "TestSchema";
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [FK_Test] FOREIGN KEY ([TestColumn1]) REFERENCES [TestSchema].[TestTable2] ([TestColumn2])");

        }

        [Test]
        public void CanCreateMulitColumnForeignKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";
            expression.ForeignKey.PrimaryTableSchema = "TestSchema";
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [FK_Test] FOREIGN KEY ([TestColumn1], [TestColumn3]) REFERENCES [TestSchema].[TestTable2] ([TestColumn2], [TestColumn4])");

        }

        [Test]
        public void CanCreateXmlColumnWithCustomSchema()
        {
            var expression = new CreateColumnExpression();
            expression.TableName = "TestTable1";

            expression.Column = new ColumnDefinition();
            expression.Column.Name = "TestColumn1";
            expression.Column.Type = DbType.Xml;
            expression.SchemaName = "TestSchema";

            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD [TestColumn1] XML NOT NULL");
        }

        public void CanCreateAutoIncrementColumnWithCustomSchema()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void CanAlterSchema()
        {
            var expression = new AlterSchemaExpression
            {
                DestinationSchemaName = "DEST",
                SourceSchemaName = "SOURCE",
                TableName = "TABLE"
            };

            var sql = generator.Generate(expression);
            sql.ShouldBe(
              "ALTER SCHEMA [DEST] TRANSFER [SOURCE].[TABLE]");
        }

        [Test]
        public void CanDropPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeletePrimaryKeyExpression();
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] DROP CONSTRAINT [TESTPRIMARYKEY]");
        }

        [Test]
        public void CanDropPrimaryKeyConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeletePrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] DROP CONSTRAINT [TESTPRIMARYKEY]");
        }

        [Test]
        public void CanDropUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteUniqueConstraintExpression();
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] DROP CONSTRAINT [TESTUNIQUECONSTRAINT]");
        }

        [Test]
        public void CanDropUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] DROP CONSTRAINT [TESTUNIQUECONSTRAINT]");
        }

        [Test]
        public void CanCreatePrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreatePrimaryKeyExpression();
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [PK_TestTable1_TestColumn1] PRIMARY KEY ([TestColumn1])");
        }

        [Test]
        public void CanCreatePrimaryKeyConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreatePrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [PK_TestTable1_TestColumn1] PRIMARY KEY ([TestColumn1])");
        }

        [Test]
        public void CanCreateNamedPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedPrimaryKeyExpression();
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [TESTPRIMARYKEY] PRIMARY KEY ([TestColumn1])");
        }

        [Test]
        public void CanCreateNamedPrimaryKeyConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedPrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [TESTPRIMARYKEY] PRIMARY KEY ([TestColumn1])");
        }

        [Test]
        public void CanCreateMultiColumnPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnPrimaryKeyExpression();
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [PK_TestTable1_TestColumn1_TestColumn2] PRIMARY KEY ([TestColumn1], [TestColumn2])");
        }

        [Test]
        public void CanCreateMultiColumnPrimaryKeyConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnPrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [PK_TestTable1_TestColumn1_TestColumn2] PRIMARY KEY ([TestColumn1], [TestColumn2])");
        }

        [Test]
        public void CanCreateMultiColumnNamedPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnNamedPrimaryKeyExpression();
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [TESTPRIMARYKEY] PRIMARY KEY ([TestColumn1], [TestColumn2])");
        }

        [Test]
        public void CanCreateMultiColumnNamedPrimaryKeyConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnNamedPrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [TESTPRIMARYKEY] PRIMARY KEY ([TestColumn1], [TestColumn2])");
        }

        [Test]
        public void CanCreateUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueConstraintExpression();
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [UC_TestTable1_TestColumn1] UNIQUE ([TestColumn1])");
        }

        [Test]
        public void CanCreateUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [UC_TestTable1_TestColumn1] UNIQUE ([TestColumn1])");
        }

        [Test]
        public void CanCreateNamedUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedUniqueConstraintExpression();
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE ([TestColumn1])");
        }

        [Test]
        public void CanCreateNamedUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE ([TestColumn1])");
        }

        [Test]
        public void CanCreateMultiColumnUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnUniqueConstraintExpression();
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [UC_TestTable1_TestColumn1_TestColumn2] UNIQUE ([TestColumn1], [TestColumn2])");
        }

        [Test]
        public void CanCreateMultiColumnUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [UC_TestTable1_TestColumn1_TestColumn2] UNIQUE ([TestColumn1], [TestColumn2])");
        }

        [Test]
        public void CanCreateMultiColumnNamedUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnNamedUniqueConstraintExpression();
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE ([TestColumn1], [TestColumn2])");
        }

        [Test]
        public void CanCreateMultiColumnNamedUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnNamedUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";
            var result = generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE ([TestColumn1], [TestColumn2])");
        }
    }
}
