using NUnit.Framework;
using Quipu.ParameterizationExtractor.DSL;
using Quipu.ParameterizationExtractor.DSL.Connector;
using Quipu.ParameterizationExtractor.Logic.Interfaces;
using Quipu.ParameterizationExtractor.Logic.Model;
using System.Linq;
using static Quipu.ParameterizationExtractor.DSL.ParserResult;

namespace Tests
{
    public class Tests
    {
        private const string testString2Froms = @"
for script ""Departments_Structure_Positions"" take
  from ""Departments"" where """" order ""1""
  from ""DepartmentStructure"" where """" order ""0""
consider
  OneTable for ""Cls_Positions"" and UniqueColumns ""Code"" build sql with throw with NoInserts
  FK for ""DepartmentStructure"" and UniqueColumns ""Code"" exclude ""DepartmentStructure,Departments""
  FK for ""Departments"" and UniqueColumns ""Code""   
";

        private const string testString = @"for script ""bla bla"" take
 from ""Cls_CutOffTimeTypes"" where """"
 from ""ExternalSystemsExportsTimeLimits"" where ""BPTypeCode in ('_IMT_SWIFT_103_INC2')""
consider
 OnlyOneTableExtract for ""Cls_CutOffTimeTypes"" and UniqueColumns ""Code""
 FKDependencyExtract for ""ExternalSystemsExportsTimeLimits"" and UniqueColumns ""Code,Code2""
  build sql with ThrowExecptionIfNotExists
 OnlyOneTableExtract for ""Cls_CutOffTimeTypes1"" and UniqueColumns ""Code""

for script ""super bla"" take
 from ""table1"" where """"
 from ""tabl2"" where ""field in ('_IMT_SWIFT_103_INC2')""
consider
 OnlyOneTableExtract for ""table1"" and UniqueColumns ""Code""
 FKDependencyExtract for ""tabl2"" and UniqueColumns ""Code""
  build sql with ThrowExecptionIfNotExists";

        private const string testString2 = @"for script ""script1"" take from ""table1"" consider FK for ""table1"" build sql with asIs with throw";


        [SetUp]
        public void DSLTest()
        {
         
        }

        [Test]
        public void DSLTestWithoutUniqueColumns()
        {
            var parser = new FparsecConnector();

            var t = parser.Parse(testString2);

            Assert.IsTrue(t.Scripts.First().TablesToProcess.Any());
            var table = t.Scripts.First().TablesToProcess.First();
            Assert.IsTrue(string.IsNullOrEmpty(t.Scripts.First().RootRecords.First().Where));
            Assert.IsInstanceOf(typeof(FKDependencyExtractStrategy), table.ExtractStrategy);
            Assert.IsTrue(table.SqlBuildStrategy.AsIsInserts && table.SqlBuildStrategy.ThrowExecptionIfNotExists);
        }

        [Test]
        public void DSLTestWith2Froms()
        {
            var parser = new FparsecConnector();

            var t = parser.Parse(testString2Froms);
            Assert.IsTrue(t.Scripts.First().TablesToProcess.Count == 3);
            Assert.IsTrue(t.Scripts.First().RootRecords.First(_ => _.TableName == "Departments").ProcessingOrder == 1);

            Assert.IsTrue(t.Scripts.First().TablesToProcess.First(_=>_.TableName == "DepartmentStructure").ExtractStrategy.DependencyToExclude.Count == 2);
            Assert.IsTrue(t.Scripts.First().TablesToProcess.Last().ExtractStrategy.DependencyToExclude.Count == 0);

        }

        private const string testString4Command1 = @"# Help";
        private const string testString4Command2 = @"# CheckTable(table1)";
        private const string testString4Command3 = @"# ToBC(table1,table2)";

        [Test]
        public void TestInternalCommand()
        {
            var s = InternalCommandParser.parse(testString4Command1);

            var t = s as CommandOK;

            Assert.IsNotNull(t, (s as Fail)?.ErrorMessage);

            Assert.AreEqual(t.GetResult, AST.Command.Help);
        }


        [Test]
        public void Test1()
        {
            var parser = new FparsecConnector();

            var t = parser.Parse(testString);

            Assert.IsTrue(t.Scripts.Count == 2);
            Assert.IsTrue(t.Scripts.Any(_ => _.ScriptName == "super bla"));
            var table2 = t.Scripts.First().TablesToProcess.First(_ => _.TableName == "ExternalSystemsExportsTimeLimits");
            Assert.IsInstanceOf(typeof(FKDependencyExtractStrategy), table2.ExtractStrategy);
            Assert.IsTrue(table2.SqlBuildStrategy.ThrowExecptionIfNotExists && !table2.SqlBuildStrategy.NoInserts && !table2.SqlBuildStrategy.AsIsInserts);
            Assert.IsTrue(table2.UniqueColumns.Count == 2);

            if (t is IAmDSLFriendly dsl)
            {
                var s = dsl.AsString();
            }
        }
    }
}