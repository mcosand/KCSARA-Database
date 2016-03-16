/*
 * Copyright 2016 Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Kcsar.Database.Model;
using Kcsara.Database.Model;
using System.Data.Entity;

namespace Kcsara.Database.Services.Training
{
  public class TrainingRecordsService
  {
    private readonly Func<IKcsarContext> _dbFactory;

    public TrainingRecordsService(Func<IKcsarContext> dbFactory)
    {
      _dbFactory = dbFactory;
    }

    public async Task<List<ParsedKcsaraCsv>> ParseKcsaraCsv(Stream dataStream)
    {
      var result = new List<ParsedKcsaraCsv>();

      using (var db = _dbFactory())
      {
        var courses = await db.TrainingCourses.ToDictionaryAsync(f => f.FullName, f => new NameIdPair { Id = f.Id, Name = f.DisplayName });
        using (var reader = new StreamReader(dataStream))
        {
          using (var csv = new CsvParser(reader))
          {
            string[] row;
            row = csv.Read();
            if (row[1] != "Email") throw new InvalidOperationException("Unexpected file format");
            row = csv.Read();
            if (row[0] != "Link to Certificate") throw new InvalidOperationException("Unexpected file format");

            while ((row = csv.Read()) != null)
            {
              string link = csv.Read().FirstOrDefault() ?? string.Empty;

              var entry = new ParsedKcsaraCsv
              {
                Email = row[1],
                Name = row[0],
                Link = link,
                Completed = DateTime.Parse(row[6])
              };

              NameIdPair course;
              if (!courses.TryGetValue("Core/" + row[3] + " - Written", out course))
              {
                if (!courses.TryGetValue("Core/" + row[3], out course))
                {
                  course = null;
                  entry.Error = "Can't find course " + row[3];
                }
              }
              entry.Course = course;

              var multiple = false;
              string dem = row[2];
              var matches = await db.Members.Where(f => f.ContactNumbers.Any(g => g.Value == entry.Email)).ToListAsync();
              if (matches.Count > 1 && !string.IsNullOrWhiteSpace(dem))
              {
                multiple = true;
                matches = matches.Where(f => f.DEM == dem).ToList();
              }
              else if (matches.Count == 0)
              {
                matches = await db.Members.Where(f => f.DEM == dem && f.Memberships.Any(g => g.Status.IsActive && (g.EndTime == null))).ToListAsync();
              }

              if (matches.Count == 1)
              {
                entry.Member = new NameIdPair(matches[0].Id, matches[0].FullName);
              }
              else if (matches.Count > 1 || multiple)
              {
                entry.Error = "Multiple members with email " + entry.Email;
              }
              else
              {
                entry.Error = "No match found for " + entry.Email;
              }
              result.Add(entry);
            }
          }
        }

        if (result.Count > 0)
        {
          DateTime min = result.Min(f => f.Completed);
          DateTime max = result.Max(f => f.Completed);
          var awards = db.TrainingAward.Where(f => f.Completed >= min && f.Completed <= max).ToList();

          foreach (var row in result)
          {
            if (!string.IsNullOrWhiteSpace(row.Error)) continue;

            var existing = awards.FirstOrDefault(f => f.Completed == row.Completed && f.Member.Id == row.Member.Id && f.Course.Id == row.Course.Id);
            if (existing != null)
            {
              row.Existing = existing.Id;
            }
          }
        }
      }
      return result;
    }
  }
}
