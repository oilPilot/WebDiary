using System;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using WebDiary.DTO;
using WebDiary.Entities;
using WebDiary.Helpers;

namespace WebDiary.Mapping;

public static class Diaries
{
    public static Diary ToEntity(this CreateDiaryDTO diaryDTO) {
        DateTime localTime = diaryDTO.CreatedUtc.AddMinutes(diaryDTO.UtcOffsetMinutes);
        var sanitizedText = DiaryHtmlSanitizer.Sanitize(diaryDTO.Text);
        var baseText = new HtmlDocument();
        baseText.LoadHtml(sanitizedText);
        return new Diary {
            Text = sanitizedText,
            BaseText = HtmlEntity.DeEntitize(baseText.DocumentNode.InnerText),
            Date = DateOnly.FromDateTime(localTime),
            Time = TimeOnly.FromDateTime(localTime),
            CreatedUtc = diaryDTO.CreatedUtc,
            UtcOffsetMinutes = diaryDTO.UtcOffsetMinutes,
            GroupId = diaryDTO.GroupId,
            mood = diaryDTO.mood ?? ""
        };
    }
    public static DiaryDTO ToDTO(this Diary diary) {
        return new DiaryDTO {
            Id = diary.Id,
            Text = DiaryHtmlSanitizer.Sanitize(diary.Text),
            Date = diary.Date,
            Time = diary.Time,
            GroupId = diary.GroupId,
            mood = diary.mood
        };
    }
}
