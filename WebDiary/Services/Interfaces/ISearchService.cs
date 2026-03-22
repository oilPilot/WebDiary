using WebDiary.DTO;

public interface ISearchService
{
    Task<List<DiaryDTO>> SearchEntriesAsync(int userId, SearchFilterDTO filters);
}
