﻿using System.Linq;
using System.Text.Json;

namespace Blazicons.Demo.Models;

public class KeywordsManager
{
    private Dictionary<string, string>? keywords;

    public Dictionary<string, string> Keywords
    {
        get
        {
            if (keywords is null)
            {
                Reload();
            }

            return keywords!;
        }
    }

    public void Reload()
    {
        keywords = JsonSerializer.Deserialize<Dictionary<string, string>>(Properties.Resources.SearchMeta);
    }

    public void AddKeyword(string key, string value)
    {
        var currentValue = string.Empty;
        if (Keywords.ContainsKey(key))
        {
            currentValue = Keywords[key];
        }

        var words = currentValue.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        if (words.Any(x => x.Equals(value, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        words.Add(value);
        Keywords[key] = string.Join(' ', words);
    }
}
