using System.Text;
using SmallRecordBook.Web.Models;

namespace SmallRecordBook.Web.Services;

public interface IFeedGenerator
{
    (string Content, string ContentType, Encoding Encoding) GenerateFeed(string baseUri, IEnumerable<RecordEntry> recordEntries, UserFeed userFeed);    
}