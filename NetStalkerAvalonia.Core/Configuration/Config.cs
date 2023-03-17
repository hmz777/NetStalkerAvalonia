﻿using NetStalkerAvalonia.Services;
using System.Text.Json;

namespace NetStalkerAvalonia.Configuration
{
    public class Config
    {
        public static AppSettings AppSettings { get; set; } = new AppSettings();
        public static JsonSerializerOptions JsonSerializerOptions = new();
    }
}
