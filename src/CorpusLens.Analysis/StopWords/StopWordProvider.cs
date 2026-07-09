namespace CorpusLens.Analysis.StopWords;

public static class StopWordProvider
{
    private static readonly IReadOnlyDictionary<string, HashSet<string>> StopWordsByLanguage =
        new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = new HashSet<string>(StringComparer.Ordinal)
            {
                "a", "about", "above", "after", "again", "against", "all", "am", "an", "and", "any", "are", "aren't",
                "as", "at", "be", "because", "been", "before", "being", "below", "between", "both", "but", "by", "can't",
                "cannot", "could", "couldn't", "did", "didn't", "do", "does", "doesn't", "doing", "don't", "down", "during",
                "each", "few", "for", "from", "further", "had", "hadn't", "has", "hasn't", "have", "haven't", "having",
                "he", "he'd", "he'll", "he's", "her", "here", "here's", "hers", "herself", "him", "himself", "his", "how",
                "how's", "i", "i'd", "i'll", "i'm", "i've", "if", "in", "into", "is", "isn't", "it", "it's", "its", "itself",
                "let's", "me", "more", "most", "mustn't", "my", "myself", "no", "nor", "not", "of", "off", "on", "once", "only",
                "or", "other", "ought", "our", "ours", "ourselves", "out", "over", "own", "same", "shan't", "she", "she'd",
                "she'll", "she's", "should", "shouldn't", "so", "some", "such", "than", "that", "that's", "the", "their", "theirs",
                "them", "themselves", "then", "there", "there's", "these", "they", "they'd", "they'll", "they're", "they've",
                "this", "those", "through", "to", "too", "under", "until", "up", "very", "was", "wasn't", "we", "we'd",
                "we'll", "we're", "we've", "were", "weren't", "what", "what's", "when", "when's", "where", "where's", "which",
                "while", "who", "who's", "whom", "why", "why's", "with", "won't", "would", "wouldn't", "you", "you'd",
                "you'll", "you're", "you've", "your", "yours", "yourself", "yourselves"
            },
            ["it"] = new HashSet<string>(StringComparer.Ordinal)
            {
                "a", "ad", "al", "allo", "ai", "agli", "all", "agl", "alla", "alle", "anche", "avere", "aveva", "avevano",
                "ben", "che", "chi", "ci", "cio", "cioè", "come", "con", "contro", "cui", "da", "dal", "dallo", "dai", "dagli",
                "dall", "dagl", "dalla", "dalle", "de", "del", "dello", "dei", "degli", "dell", "degl", "della", "delle", "di",
                "dove", "e", "ed", "era", "erano", "essere", "fa", "fra", "gli", "ha", "hai", "hanno", "ho", "il", "in",
                "io", "la", "le", "lei", "li", "lo", "loro", "lui", "ma", "mi", "mia", "mie", "miei", "mio", "ne", "negli",
                "nel", "nella", "nelle", "nello", "no", "noi", "non", "o", "per", "perché", "più", "quale", "quali", "quando",
                "quanta", "quante", "quanti", "quanto", "quella", "quelle", "quelli", "quello", "questa", "queste", "questi", "questo",
                "se", "sei", "si", "sia", "siamo", "siete", "sono", "sta", "su", "sua", "sue", "sugli", "sul", "sulla",
                "sulle", "sullo", "suo", "suoi", "ti", "tra", "tu", "tua", "tue", "tuo", "tuoi", "tutti", "un", "una",
                "uno", "vi", "voi"
            },
            ["fr"] = new HashSet<string>(StringComparer.Ordinal)
            {
                "a", "ai", "aie", "aient", "aies", "ait", "as", "au", "aura", "aurai", "auraient", "aurais", "aurait", "auras",
                "aurez", "auriez", "aurions", "aurons", "auront", "aux", "avaient", "avais", "avait", "avec", "avez", "aviez",
                "avions", "avoir", "avons", "ayant", "ayez", "ayons", "c", "ce", "ces", "cet", "cette", "d", "dans", "de", "des",
                "du", "elle", "elles", "en", "es", "est", "et", "été", "étée", "étées", "étés", "étant", "étante", "étants",
                "étantes", "êtes", "être", "eu", "eue", "eues", "eus", "eusse", "eussent", "eusses", "eussiez", "eussions",
                "eut", "eux", "fûmes", "fût", "fûtes", "furent", "fus", "fusse", "fussent", "fusses", "fussiez", "fussions",
                "fut", "ici", "il", "ils", "j", "je", "l", "la", "le", "les", "leur", "leurs", "lui", "m", "ma", "mais",
                "me", "même", "mes", "moi", "mon", "ne", "nos", "notre", "nous", "on", "ont", "ou", "par", "pas", "pour",
                "qu", "que", "quel", "quelle", "quelles", "quels", "qui", "s", "sa", "sans", "se", "sera", "serai", "seraient",
                "serais", "serait", "seras", "serez", "seriez", "serions", "serons", "seront", "ses", "soi", "soient", "sois",
                "soit", "sommes", "son", "sont", "soyez", "soyons", "suis", "sur", "t", "ta", "te", "tes", "toi", "ton", "tu",
                "un", "une", "vos", "votre", "vous", "y"
            },
            ["de"] = new HashSet<string>(StringComparer.Ordinal)
            {
                "aber", "alle", "allem", "allen", "aller", "alles", "als", "also", "am", "an", "ander", "andere", "anderem",
                "anderen", "anderer", "anderes", "anderm", "andern", "anderr", "anders", "auch", "auf", "aus", "bei", "bin", "bis",
                "bist", "da", "damit", "dann", "das", "dass", "daß", "dasselbe", "dazu", "dein", "deine", "deinem", "deinen",
                "deiner", "deines", "dem", "demselben", "den", "denn", "denselben", "der", "derer", "derselbe", "derselben", "des",
                "desselben", "dessen", "dich", "die", "dies", "diese", "dieselbe", "dieselben", "diesem", "diesen", "dieser", "dieses",
                "dir", "doch", "dort", "du", "durch", "ein", "eine", "einem", "einen", "einer", "eines", "einig", "einige",
                "einigem", "einigen", "einiger", "einiges", "einmal", "er", "es", "etwas", "euch", "euer", "eure", "eurem",
                "euren", "eurer", "eures", "für", "gegen", "gewesen", "hab", "habe", "haben", "hat", "hatte", "hatten", "hier",
                "hin", "hinter", "ich", "ihm", "ihn", "ihnen", "ihr", "ihre", "ihrem", "ihren", "ihrer", "ihres", "im", "in",
                "indem", "ins", "ist", "jede", "jedem", "jeden", "jeder", "jedes", "jene", "jenem", "jenen", "jener", "jenes",
                "jetzt", "kann", "kein", "keine", "keinem", "keinen", "keiner", "keines", "können", "könnte", "machen", "man",
                "manche", "manchem", "manchen", "mancher", "manches", "mein", "meine", "meinem", "meinen", "meiner", "meines",
                "mich", "mir", "mit", "muss", "musste", "nach", "nicht", "nichts", "noch", "nun", "nur", "ob", "oder", "ohne",
                "sehr", "sein", "seine", "seinem", "seinen", "seiner", "seines", "selbst", "sich", "sie", "sind", "so", "solche",
                "solchem", "solchen", "solcher", "solches", "soll", "sollte", "sondern", "sonst", "über", "um", "und", "uns",
                "unse", "unsem", "unsen", "unser", "unses", "unter", "viel", "vom", "von", "vor", "während", "war", "waren",
                "warst", "was", "weg", "weil", "weiter", "welche", "welchem", "welchen", "welcher", "welches", "wenn", "werde",
                "werden", "wie", "wieder", "will", "wir", "wird", "wirst", "wo", "wollen", "wollte", "würde", "würden",
                "zu", "zum", "zur", "zwar", "zwischen"
            }
        };

    public static bool IsStopWord(string word, string languageCode)
    {
        if (string.IsNullOrWhiteSpace(word) || string.IsNullOrWhiteSpace(languageCode))
        {
            return false;
        }

        string normalizedLanguageCode = NormalizeLanguageCode(languageCode);
        return StopWordsByLanguage.TryGetValue(normalizedLanguageCode, out HashSet<string>? stopWords)
            && stopWords.Contains(word);
    }

    public static bool IsStopWord(string word, IEnumerable<string> languageCodes)
    {
        ArgumentNullException.ThrowIfNull(languageCodes);

        foreach (string languageCode in languageCodes)
        {
            if (IsStopWord(word, languageCode))
            {
                return true;
            }
        }

        return false;
    }

    private static string NormalizeLanguageCode(string languageCode)
    {
        string trimmed = languageCode.Trim().ToLowerInvariant();
        int separatorIndex = trimmed.IndexOfAny(new[] { '-', '_' });
        return separatorIndex < 0 ? trimmed : trimmed[..separatorIndex];
    }
}
