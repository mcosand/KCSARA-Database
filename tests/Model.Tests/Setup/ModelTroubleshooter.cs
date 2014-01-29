/*
 * Copyright 2014 Matthew Cosand
 */
namespace Internal.Database.Model.Setup
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using NUnit.Framework;

  [TestFixture]
  public class ModelTroubleshooter
  {
    [Test]
    [Explicit]
    public void CascadeDeleteTree()
    {
      var db = ContextGenerator.CreateContext();

      var onj = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)db);

      List<string> typeList = new List<string>();
      List<string> edges = new List<string>();

      var typeNameList = new[] { "Mission" };
      foreach (var typeName in typeNameList)
      {
        if (typeList.Contains(typeName)) continue;

        ProcessType((System.Data.Entity.Core.Metadata.Edm.EntityType)onj.ObjectContext.MetadataWorkspace.GetType("Mission", "Kcsar.Database.Model", System.Data.Entity.Core.Metadata.Edm.DataSpace.CSpace),
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

    private void ProcessType(System.Data.Entity.Core.Metadata.Edm.EntityType entityType, List<string> typeList, List<string> edges)
    {
      var navProperties = entityType.NavigationProperties;
      foreach (var nav in navProperties)
      {
        var toType = nav.ToEndMember.GetEntityType();
        if (!typeList.Contains(toType.Name))
        {
          typeList.Add(toType.Name);
          ProcessType(toType, typeList, edges);
        }

        string edge = string.Format("{0} -> {1} {2}",
          entityType.Name,
          toType.Name,
          nav.ToEndMember.DeleteBehavior == System.Data.Entity.Core.Metadata.Edm.OperationAction.Cascade ? " [color=\"red\"]" : ""
          );

        if (!edges.Contains(edge)) edges.Add(edge);
      }
    }

  }
}
