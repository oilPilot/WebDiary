using System;
using WebDiary.DTO;
using WebDiary.Entities;

namespace WebDiary.Mapping;

public static class Diaries
{
    public static Diary ToEntity(this CreateDiaryDTO diaryDTO) {
        return new Diary {
            Text = diaryDTO.Text,
            Date = DateOnly.FromDateTime(DateTime.Now),
            Time = TimeOnly.FromDateTime(DateTime.Now),
            GroupId = diaryDTO.GroupId
        };
    }
    public static DiaryDTO ToDTO(this Diary diary) {
        return new DiaryDTO {
            Id = diary.Id,
            Text = diary.Text,
            Date = diary.Date,
            Time = diary.Time,
            GroupId = diary.GroupId
        };
    }
}
