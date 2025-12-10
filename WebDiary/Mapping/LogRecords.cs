using System;
using Microsoft.AspNetCore.Identity;
using Serilog.Events;
using WebDiary.DTO;
using WebDiary.Entities;

namespace WebDiary.Mapping;

public static class LogRecords
{
    public static LogRecordsDTO toDTO(this LogRecord logs) {
        string? LevelAsString = null;
        switch (logs.level) {
            case 0:
                LevelAsString = "Verbose";
                break;
            case 1:
                LevelAsString = "Debug";
                break;
            case 2:
                LevelAsString = "Information";
                break;
            case 3:
                LevelAsString = "Warning";
                break;
            case 4:
                LevelAsString = "Error";
                break;
            case 5:
                LevelAsString = "Fatal";
                break;
        }
        return new LogRecordsDTO()
        {
            Message = logs.message,
            level = LevelAsString != null ? LevelAsString : "Fatal",
            Timestamp = logs.timestamp,
            Exception = logs.exception != null ? logs.exception : "",
            Properties = logs.log_event != null ? logs.log_event : "",
            levelInt = logs.level
        };
    }
}

