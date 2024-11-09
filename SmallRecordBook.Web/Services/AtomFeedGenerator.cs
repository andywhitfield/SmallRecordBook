using System.Net;
using System.Text;
using System.Xml.Linq;
using SmallRecordBook.Web.Models;

namespace SmallRecordBook.Web.Services;

public class AtomFeedGenerator : IFeedGenerator
{
    private readonly XNamespace ns = "http://www.w3.org/2005/Atom";

    public (string Content, string ContentType, Encoding Encoding) GenerateFeed(string baseUri,
        IEnumerable<RecordEntry> recordEntries, UserFeed userFeed)
    {
        var atom = new XDocument(new XDeclaration("1.0", "utf-8", null));
        var docRoot = new XElement(ns + "feed");
        var todayDt = DateTime.Today;
        var today = DateOnly.FromDateTime(todayDt);

        docRoot.Add(new XElement(ns + "title", "SmallRecordBook"));
        docRoot.Add(new XElement(ns + "link", new XAttribute("type", "text/html"), new XAttribute("href", baseUri), new XAttribute("rel", "alternate")));
        docRoot.Add(new XElement(ns + "updated", (userFeed.LastUpdateDateTime ?? userFeed.CreatedDateTime).ToString("O")));
        docRoot.Add(new XElement(ns + "id", $"{baseUri}/api/feed/{userFeed.UserFeedIdentifier}"));

        foreach (var recordEntry in recordEntries)
        {
            var entry = new XElement(ns + "entry");

            var recordUpdateDate = recordEntry.LastUpdateDateTime ?? recordEntry.CreatedDateTime;
            if (recordUpdateDate < todayDt)
                recordUpdateDate = todayDt;

            var dueDate = recordEntry.ReminderDate ?? today;
            var isUpcoming = dueDate > today;
            var isOverdue = dueDate < today;
            var itemUri = $"{baseUri}/record/{recordEntry.RecordEntryId}";
            entry.Add(new XElement(ns + "title", $"Reminder {(isUpcoming ? "upcoming" : isOverdue ? "overdue" : "due")}: {recordEntry.Title}"));
            entry.Add(new XElement(ns + "link", new XAttribute("href", itemUri)));
            entry.Add(new XElement(ns + "updated", recordUpdateDate.ToString("O")));
            entry.Add(new XElement(ns + "id", $"{recordEntry.RecordEntryId}/{isOverdue.ToString().ToLowerInvariant()}/{dueDate:yyyyMMdd}"));

            var content = new XElement(ns + "content", new XAttribute("type", "html"));
            var itemDescription = $"<p>Reminder due {GetFriendlyDueDate(isUpcoming, isOverdue, dueDate)}:</p><p><b>{WebUtility.HtmlEncode(recordEntry.Title)}</b></p>";
            content.Add(new XCData($"{itemDescription}<p><a href=\"{itemUri}\">Open item</a></p>"));
            entry.Add(content);

            docRoot.Add(entry);
        }

        atom.Add(docRoot);
        return (atom.ToXmlString(), "application/xml", Encoding.UTF8);
    }

    private static string GetFriendlyDueDate(bool isUpcoming, bool isOverdue, DateOnly dueDate)
    {
        if (isUpcoming)
        {
            var dayDiff = (dueDate.ToDateTime(TimeOnly.MinValue) - DateTime.Today).TotalDays;
            return dayDiff == 1 ? "tomorrow" : dayDiff < 7 ? $"next {dueDate:dddd}" : $"on {dueDate:d MMM yyyy}";
        }
        else if (isOverdue)
        {
            var dayDiff = (DateTime.Today - dueDate.ToDateTime(TimeOnly.MinValue)).TotalDays;
            return dayDiff == 1 ? "yesterday" : dayDiff < 7 ? $"last {dueDate:dddd}" : $"on {dueDate:d MMM yyyy}";
        }
        else
        {
            return "today";
        }
    }
}