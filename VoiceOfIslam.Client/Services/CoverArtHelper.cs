using System.Text;
using System.Text.Encodings.Web;

namespace VoiceOfIslam.Client.Services
{
    public static class CoverArtHelper
    {
        public static string CreateCoverUrl(string? title, string? speaker)
        {
            var primaryLine = HtmlEncoder.Default.Encode(TrimOrFallback(title, "Ohun Islam"));
            var secondaryLine = HtmlEncoder.Default.Encode(TrimOrFallback(speaker, "Audio Lecture"));
            var initials = HtmlEncoder.Default.Encode(BuildInitials(title, speaker));
            var gradient = CreateGradientSeed(title, speaker);

            var svg = $"""
                <svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 512 512' role='img' aria-label='{primaryLine}'>
                    <defs>
                        <linearGradient id='bg' x1='0%' y1='0%' x2='100%' y2='100%'>
                            <stop offset='0%' stop-color='{gradient[0]}' />
                            <stop offset='100%' stop-color='{gradient[1]}' />
                        </linearGradient>
                    </defs>
                    <rect width='512' height='512' rx='48' fill='url(#bg)' />
                    <circle cx='392' cy='112' r='96' fill='rgba(255,255,255,0.10)' />
                    <circle cx='120' cy='392' r='120' fill='rgba(255,255,255,0.08)' />
                    <text x='64' y='210' fill='rgba(255,255,255,0.82)' font-family='Manrope, Arial, sans-serif' font-size='34' font-weight='700'>{primaryLine}</text>
                    <text x='64' y='266' fill='rgba(255,255,255,0.72)' font-family='Plus Jakarta Sans, Arial, sans-serif' font-size='22' font-weight='500'>{secondaryLine}</text>
                    <rect x='64' y='304' width='128' height='10' rx='5' fill='rgba(255,255,255,0.22)' />
                    <text x='64' y='404' fill='white' font-family='Manrope, Arial, sans-serif' font-size='86' font-weight='800'>{initials}</text>
                </svg>
                """;

            var svgBytes = Encoding.UTF8.GetBytes(svg);
            return $"data:image/svg+xml;base64,{Convert.ToBase64String(svgBytes)}";
        }

        private static string TrimOrFallback(string? value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        private static string BuildInitials(string? title, string? speaker)
        {
            static string FirstLetters(string? text)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    return string.Empty;
                }

                var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var builder = new StringBuilder();
                foreach (var part in parts.Take(2))
                {
                    builder.Append(char.ToUpperInvariant(part[0]));
                }

                return builder.Length > 0 ? builder.ToString() : string.Empty;
            }

            var initials = FirstLetters(title);
            if (!string.IsNullOrWhiteSpace(initials))
            {
                return initials;
            }

            initials = FirstLetters(speaker);
            return string.IsNullOrWhiteSpace(initials) ? "OI" : initials;
        }

        private static string[] CreateGradientSeed(string? title, string? speaker)
        {
            var seed = HashCode.Combine(title?.GetHashCode(StringComparison.OrdinalIgnoreCase) ?? 0, speaker?.GetHashCode(StringComparison.OrdinalIgnoreCase) ?? 0);
            var palette = new[]
            {
                ("#416355", "#6B8E7F"),
                ("#5B4B43", "#8A6E5B"),
                ("#334155", "#64748B"),
                ("#4C1D95", "#7C3AED")
            };

            var selected = palette[Math.Abs(seed) % palette.Length];
            return [selected.Item1, selected.Item2];
        }
    }
}