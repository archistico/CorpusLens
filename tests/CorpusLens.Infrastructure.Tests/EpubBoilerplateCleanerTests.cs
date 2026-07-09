using CorpusLens.Infrastructure.Epub;
using Xunit;

namespace CorpusLens.Infrastructure.Tests;

public sealed class EpubBoilerplateCleanerTests
{
    [Fact]
    public void Clean_ShouldRemoveProjectGutenbergHeaderAndFooter()
    {
        const string text = """
            The Project Gutenberg eBook of Sample
            *** START OF THE PROJECT GUTENBERG EBOOK SAMPLE ***
            Real book text.
            *** END OF THE PROJECT GUTENBERG EBOOK SAMPLE ***
            License text.
            """;

        EpubBoilerplateCleaner cleaner = new();

        string cleaned = cleaner.Clean(text);

        Assert.Contains("Real book text.", cleaned, StringComparison.Ordinal);
        Assert.DoesNotContain("Project Gutenberg", cleaned, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("License text", cleaned, StringComparison.Ordinal);
    }

    [Fact]
    public void Clean_ShouldRemoveLeadingDuplicatedChapterList()
    {
        const string text = """
            Alice's Adventures in Wonderland
            by Lewis Carroll
            Contents
            CHAPTER I.
            Down the Rabbit-Hole
            CHAPTER II.
            The Pool of Tears

            CHAPTER I.
            Down the Rabbit-Hole
            Alice was beginning to get very tired.
            """;

        EpubBoilerplateCleaner cleaner = new();

        string cleaned = cleaner.Clean(text);

        Assert.StartsWith("CHAPTER I.", cleaned, StringComparison.Ordinal);
        Assert.Contains("Alice was beginning", cleaned, StringComparison.Ordinal);
        Assert.DoesNotContain("Contents", cleaned, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("by Lewis Carroll", cleaned, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Clean_ShouldNotRemoveSingleChapterAtStart()
    {
        const string text = """
            CHAPTER I.
            Down the Rabbit-Hole
            Alice was beginning to get very tired.
            """;

        EpubBoilerplateCleaner cleaner = new();

        string cleaned = cleaner.Clean(text);

        Assert.StartsWith("CHAPTER I.", cleaned, StringComparison.Ordinal);
        Assert.Contains("Alice was beginning", cleaned, StringComparison.Ordinal);
    }

    [Fact]
    public void IsLikelyFrontMatterOnly_ShouldDetectStandaloneTableOfContents()
    {
        const string text = """
            Contents
            CHAPTER I.
            Down the Rabbit-Hole
            CHAPTER II.
            The Pool of Tears
            CHAPTER III.
            A Caucus-Race and a Long Tale
            """;

        EpubBoilerplateCleaner cleaner = new();

        Assert.True(cleaner.IsLikelyFrontMatterOnly(text));
    }


    [Fact]
    public void IsLikelyFrontMatterOnly_ShouldDetectTableOfContentsAfterTitleLines()
    {
        const string text = """
            Alice's Adventures in Wonderland
            by Lewis Carroll
            THE MILLENNIUM FULCRUM EDITION 3.0
            Contents
            CHAPTER I.
            Down the Rabbit-Hole
            CHAPTER II.
            The Pool of Tears
            CHAPTER III.
            A Caucus-Race and a Long Tale
            CHAPTER IV.
            The Rabbit Sends in a Little Bill
            """;

        EpubBoilerplateCleaner cleaner = new();

        Assert.True(cleaner.IsLikelyFrontMatterOnly(text));
    }

    [Fact]
    public void IsLikelyFrontMatterOnly_ShouldNotDetectChapterWithProse()
    {
        const string text = """
            Contents
            CHAPTER I.
            Alice was beginning to get very tired of sitting by her sister on the bank.
            """;

        EpubBoilerplateCleaner cleaner = new();

        Assert.False(cleaner.IsLikelyFrontMatterOnly(text));
    }

    [Fact]
    public void Clean_ShouldRemoveLiberLiberLeadingMatterAndKeepFirstChapter()
    {
        const string text = """
            Alice nel paese delle meraviglie, di Lewis Carroll
            Copertina

            Informazioni
            QUESTO E-BOOK:
            TITOLO: Alice nel paese delle meraviglie
            AUTORE: Carroll, Lewis
            LICENZA: questo testo è distribuito con licenza Liber Liber.

            Liber Liber
            Se questo libro ti è piaciuto, aiutaci a realizzarne altri.

            Indice
            Copertina
            Colophon
            Liber Liber
            Indice (questa pagina)
            I – NELLA CONIGLIERA
            II – LO STAGNO DI LAGRIME

            ALICE NEL PAESE DELLE MERAVIGLIE
            di Lewis Carroll

            I
            NELLA CONIGLIERA
            Alice cominciava a sentirsi assai stanca di starsene seduta.
            """;

        EpubBoilerplateCleaner cleaner = new();

        string cleaned = cleaner.Clean(text);

        Assert.StartsWith("I", cleaned, StringComparison.Ordinal);
        Assert.Contains("NELLA CONIGLIERA", cleaned, StringComparison.Ordinal);
        Assert.Contains("Alice cominciava", cleaned, StringComparison.Ordinal);
        Assert.DoesNotContain("QUESTO E-BOOK", cleaned, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Liber Liber", cleaned, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("LICENZA", cleaned, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void IsLikelyFrontMatterOnly_ShouldDetectItalianLiberLiberTocOnly()
    {
        const string text = """
            Alice nel paese delle meraviglie
            Informazioni
            QUESTO E-BOOK:
            TITOLO: Alice nel paese delle meraviglie
            LICENZA: Liber Liber
            Indice
            I – NELLA CONIGLIERA
            II – LO STAGNO DI LAGRIME
            III – CORSA SCOMPIGLIATA
            """;

        EpubBoilerplateCleaner cleaner = new();

        Assert.True(cleaner.IsLikelyFrontMatterOnly(text));
    }

    [Fact]
    public void IsLikelyFrontMatterOnly_ShouldDetectItalianLiberLiberMetadataChapter()
    {
        const string text = """
            Alice nel paese delle meraviglie
            Informazioni
            QUESTO E-BOOK:
            TITOLO: Alice nel paese delle meraviglie
            AUTORE: Carroll, Lewis
            LICENZA: questo testo è distribuito con licenza Liber Liber.
            http://www.liberliber.it
            Se questo libro ti è piaciuto, aiutaci a realizzarne altri.
            Fai una donazione.
            """;

        EpubBoilerplateCleaner cleaner = new();

        Assert.True(cleaner.IsLikelyFrontMatterOnly(text));
    }

    [Fact]
    public void IsLikelyFrontMatterOnly_ShouldNotDetectNarrativeChapterMentioningIndice()
    {
        const string text = """
            Il giudice trovò l'indice della mano sulla pagina.
            Era una prova strana, ma nessuno nella stanza osava parlare.
            Dopo qualche minuto, il vecchio si alzò e disse che avrebbe raccontato tutto.
            """;

        EpubBoilerplateCleaner cleaner = new();

        Assert.False(cleaner.IsLikelyFrontMatterOnly(text));
    }

    [Fact]
    public void IsLikelyFrontMatterOnly_ShouldDetectLongerItalianEbookMetadataChapter()
    {
        const string text = """
            Informazioni
            QUESTO E-BOOK:
            TITOLO: Alla conquista della Luna
            AUTORE: Emilio Salgari
            TRADUTTORE:
            CURATORE:
            LICENZA: questo testo è distribuito con licenza Liber Liber.
            http://www.liberliber.it
            Se questo libro ti è piaciuto, aiutaci a realizzarne altri.
            Fai una donazione.
            Puoi sostenere il progetto e contribuire alla diffusione gratuita della cultura.
            Questa pagina contiene informazioni editoriali e tecniche sul file digitale.
            """;

        EpubBoilerplateCleaner cleaner = new();

        Assert.True(cleaner.IsLikelyFrontMatterOnly(text));
    }

}
