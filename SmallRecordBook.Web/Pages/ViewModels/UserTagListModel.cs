namespace SmallRecordBook.Web.Pages.ViewModels;

public record UserTagListModel(IEnumerable<(string Tag, int TagCount)> Tags) {}