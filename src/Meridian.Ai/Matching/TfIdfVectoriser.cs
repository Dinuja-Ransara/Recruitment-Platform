using System.Text.RegularExpressions;

namespace Meridian.Ai.Matching;

/// <summary>
/// Term frequency, inverse document frequency, implemented from scratch.
///
/// The idea in one line: a word matters in proportion to how often it appears in
/// one document and in inverse proportion to how many documents contain it. That
/// is what stops "experience", "team" and "responsible" from dominating every
/// comparison between a CV and a job description, because those words appear
/// everywhere and therefore carry almost no signal.
///
/// The whole class is deterministic and has no dependencies, which is what makes
/// the scoring reproducible and unit testable.
/// </summary>
public class TfIdfVectoriser
{
    private static readonly Regex TokenPattern = new(@"[a-z0-9][a-z0-9+#.\-]*", RegexOptions.Compiled);

    /// <summary>
    /// Words carrying no discriminating power in a recruitment corpus. Removing
    /// them up front keeps the vocabulary smaller and the cosine more meaningful.
    /// </summary>
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a", "an", "the", "and", "or", "but", "if", "then", "than", "as", "at", "by", "for", "from",
        "in", "into", "of", "on", "onto", "to", "with", "within", "without", "is", "are", "was",
        "were", "be", "been", "being", "have", "has", "had", "do", "does", "did", "will", "would",
        "can", "could", "should", "may", "might", "must", "this", "that", "these", "those", "it",
        "its", "we", "our", "you", "your", "they", "their", "he", "she", "his", "her",
        "work", "working", "experience", "experienced", "role", "job", "team", "teams", "company",
        "responsible", "responsibilities", "including", "ability", "strong", "good", "excellent",
        "years", "year", "skills", "skill", "knowledge", "understanding", "candidate", "candidates"
    };

    private readonly Dictionary<string, double> _inverseDocumentFrequency = new(StringComparer.OrdinalIgnoreCase);
    private int _documentCount;

    /// <summary>
    /// Learns document frequencies from the corpus. Called once with every job
    /// description and resume in play, before any comparison is made.
    /// </summary>
    public void Fit(IEnumerable<string> corpus)
    {
        var documents = corpus.Select(Tokenise).ToList();
        _documentCount = documents.Count;
        _inverseDocumentFrequency.Clear();

        if (_documentCount == 0)
        {
            return;
        }

        var documentFrequency = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var terms in documents)
        {
            foreach (var term in terms.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                documentFrequency[term] = documentFrequency.GetValueOrDefault(term) + 1;
            }
        }

        foreach (var (term, frequency) in documentFrequency)
        {
            // Smoothed IDF. The +1 terms keep the value finite when a term appears
            // in every document, and keep it positive so no term gets a negative
            // weight that would make a match count against a candidate.
            _inverseDocumentFrequency[term] = Math.Log((_documentCount + 1.0) / (frequency + 1.0)) + 1.0;
        }
    }

    /// <summary>Projects one document into a sparse term-weight vector.</summary>
    public Dictionary<string, double> Transform(string document)
    {
        var terms = Tokenise(document);
        var vector = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        if (terms.Count == 0)
        {
            return vector;
        }

        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var term in terms)
        {
            counts[term] = counts.GetValueOrDefault(term) + 1;
        }

        foreach (var (term, count) in counts)
        {
            // Term frequency is normalised by document length so a long CV does
            // not outscore a concise one purely by repeating itself.
            var termFrequency = (double)count / terms.Count;
            var idf = _inverseDocumentFrequency.GetValueOrDefault(term, DefaultIdf());
            vector[term] = termFrequency * idf;
        }

        return vector;
    }

    /// <summary>
    /// Cosine similarity, 0 to 1. Measures the angle between two term vectors,
    /// which compares what two documents are about while ignoring how long they
    /// are. A one-page CV and a three-page CV describing the same engineer score
    /// alike, which is the behaviour recruitment needs.
    /// </summary>
    public static double CosineSimilarity(
        IReadOnlyDictionary<string, double> left,
        IReadOnlyDictionary<string, double> right)
    {
        if (left.Count == 0 || right.Count == 0)
        {
            return 0;
        }

        // Iterate the smaller vector: the dot product only has terms where both sides are non-zero.
        var (smaller, larger) = left.Count <= right.Count ? (left, right) : (right, left);

        double dotProduct = 0;
        foreach (var (term, weight) in smaller)
        {
            if (larger.TryGetValue(term, out var otherWeight))
            {
                dotProduct += weight * otherWeight;
            }
        }

        if (dotProduct == 0)
        {
            return 0;
        }

        var leftMagnitude = Math.Sqrt(left.Values.Sum(v => v * v));
        var rightMagnitude = Math.Sqrt(right.Values.Sum(v => v * v));

        if (leftMagnitude == 0 || rightMagnitude == 0)
        {
            return 0;
        }

        return Math.Clamp(dotProduct / (leftMagnitude * rightMagnitude), 0, 1);
    }

    /// <summary>
    /// Weight for a term never seen during Fit. Treated as maximally rare rather
    /// than ignored, because an unseen term is usually a niche technology and
    /// those are exactly the terms worth rewarding.
    /// </summary>
    private double DefaultIdf() => Math.Log(_documentCount + 1.0) + 1.0;

    internal static List<string> Tokenise(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new List<string>();
        }

        var tokens = new List<string>();
        foreach (Match match in TokenPattern.Matches(text.ToLowerInvariant()))
        {
            var token = match.Value.Trim('.', '-');

            // Single characters carry no meaning, but "c#" and "r" style names do,
            // so length filtering happens after the stop-word check rather than before.
            if (token.Length == 0 || StopWords.Contains(token))
            {
                continue;
            }

            if (token.Length == 1 && !char.IsDigit(token[0]))
            {
                continue;
            }

            tokens.Add(token);
        }

        return tokens;
    }
}
