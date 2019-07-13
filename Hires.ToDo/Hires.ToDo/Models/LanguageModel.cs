using System.Collections.Generic;

namespace Hires.ToDo.Models
{
    public static class LanguageModel
    {
        public static readonly List<KeyValuePair<string, string>> Languages = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("en-GB", "English (United Kingdom)"),
            new KeyValuePair<string, string>("en-US", "English (United States)"),
            new KeyValuePair<string, string>("es-ES", "Spanish (Spain)"),
            new KeyValuePair<string, string>("fr-FR", "French (France)"),
            new KeyValuePair<string, string>("it-IT", "Italian (Italy)"),
            new KeyValuePair<string, string>("pl-PL", "Polish (Poland)"),
            new KeyValuePair<string, string>("ru-RU", "Russian (Russia)")
        };
    }
}
