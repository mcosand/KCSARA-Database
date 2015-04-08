using System;
using System.Collections.Generic;
using System.Text;
using Kcsar.Database.Data;
using NUnit.Framework;
using System.Linq;
using System.Reflection;

namespace Internal.Database.Data
{
  [TestFixture]
  public class ModelTroubleshooter
  {
    /// <summary>Generate a GVEdit compatible graph of the model, coloring lines based on CASCADE ON DELETE or Multiplicity.</summary>
    [Test]
    [Explicit]
    public void CascadeDeleteTree()
    {
      var connString = DatabaseTestHelpers.CreateTestDatabase();

      using (var db = new KcsarContext(connString, Console.WriteLine))
      {
        var onj = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)db);

        List<string> typeList = new List<string>();
        List<string> edges = new List<string>();

        var typeNameList = new[] { "SarEventRow", "MemberRow", "UnitRow", "TrainingCourseRow" };

        foreach (var typeName in typeNameList)
        {
          if (typeList.Contains(typeName)) continue;

          ProcessType((System.Data.Entity.Core.Metadata.Edm.EntityType)onj.ObjectContext.MetadataWorkspace.GetType(typeName, "Kcsar.Database.Data", System.Data.Entity.Core.Metadata.Edm.DataSpace.CSpace),
            typeList, edges);
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("digraph CascadeDeleteTree {");
        sb.AppendLine("node [shape=box];  " + string.Join("; ", typeList) + ";");
        foreach (var edge in edges)
        {
          sb.AppendLine(edge);
        }
        sb.AppendLine();
        sb.AppendLine("overlap=false");
        sb.AppendLine("}");

        Console.WriteLine(sb.ToString());
      }
    }

    private void ProcessType(System.Data.Entity.Core.Metadata.Edm.EntityType entityType, List<string> typeList, List<string> edges)
    {
      var navProperties = entityType.NavigationProperties;
      //var getter = typeof(System.Data.Entity.Core.Metadata.Edm.EdmMember).GetProperty("IsPrimaryKeyColumn", BindingFlags.NonPublic | BindingFlags.Instance);
      foreach (var nav in navProperties)
      {
        var toType = nav.ToEndMember.GetEntityType();
        if (!typeList.Contains(toType.Name))
        {
          typeList.Add(toType.Name);
          ProcessType(toType, typeList, edges);
        }
        //AddForeignKey("dbo.AnimalEvents", "AnimalId", "dbo.Animals", "Id", cascadeDelete: true);
        /*
        string edge = string.Format("AddForeignKey(\"dbo.{0}\", \"{1}\", \"dbo.{2}\", \"{3}\"{4});",
          entityType.Name,
          nav.FromEndMember.Name,
          toType.Name,
          nav.ToEndMember.Name,
          nav.ToEndMember.DeleteBehavior == System.Data.Entity.Core.Metadata.Edm.OperationAction.Cascade ? ", cascadeDelete: true" : string.Empty
          );
        */
        
        string edge = string.Format("{0} -> {1} [color=\"{2}\" penwidth=\"{3}\"]",
          entityType.Name,
          toType.Name,
          nav.ToEndMember.DeleteBehavior == System.Data.Entity.Core.Metadata.Edm.OperationAction.Cascade ? "red" : "black",
          nav.ToEndMember.RelationshipMultiplicity == System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity.Many ? "2.0" : "1.0"
          );
        
        if (!edges.Contains(edge)) edges.Add(edge);
      }
    }

  }
}
