using System.Text;
using System.Xml.Linq;

namespace SmallRecordBook.Web.Services;

public static class FeedGeneratorExtensions
{
    public static string ToXmlString(this XDocument xml)
    {
        using var writer = new Utf8StringWriter();
        xml.Save(writer);
        return writer.ToString();
    }

    private class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
