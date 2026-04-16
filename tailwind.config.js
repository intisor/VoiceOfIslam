/** @type {import('tailwindcss').Config} */
module.exports = {
    darkMode: "class",
    content: [
        "./VoiceOfIslam.Client/**/*.{razor,html,cshtml}",
        "./VoiceOfIslam/**/*.{razor,html,cshtml}"
    ],
    theme: {
        extend: {
            "colors": {
                "secondary-fixed": "#f0e0d3",
                "error-container": "#ffdad6",
                "surface-dim": "#dddad3",
                "surface-tint": "#436558",
                "tertiary-fixed": "#ffdad7",
                "primary-container": "#597c6d",
                "on-background": "#1c1c18",
                "secondary": "#685c53",
                "on-tertiary-fixed-variant": "#633d3a",
                "inverse-on-surface": "#f4f0e9",
                "inverse-primary": "#aacfbe",
                "on-primary-container": "#f5fff8",
                "primary-fixed": "#c5ebd9",
                "inverse-surface": "#31302c",
                "outline-variant": "#c1c8c3",
                "on-surface": "#1c1c18",
                "on-error-container": "#93000a",
                "error": "#ba1a1a",
                "tertiary": "#7b514e",
                "tertiary-fixed-dim": "#efbab5",
                "secondary-container": "#f0e0d3",
                "on-primary-fixed": "#002117",
                "on-secondary-fixed-variant": "#4f453c",
                "on-error": "#ffffff",
                "primary": "#416355",
                "on-secondary-container": "#6e6258",
                "on-secondary": "#ffffff",
                "surface-container-lowest": "#ffffff",
                "on-secondary-fixed": "#221a13",
                "on-tertiary-fixed": "#301311",
                "surface-variant": "#e6e2db",
                "outline": "#717974",
                "surface-container-high": "#ece8e1",
                "surface-container-highest": "#e6e2db",
                "tertiary-container": "#966966",
                "on-primary": "#ffffff",
                "surface-bright": "#fdf9f2",
                "surface-container": "#f1ede6",
                "on-surface-variant": "#414845",
                "on-tertiary": "#ffffff",
                "secondary-fixed-dim": "#d3c4b8",
                "surface-container-low": "#f7f3ec",
                "surface": "#fdf9f2",
                "on-tertiary-container": "#fffbff",
                "on-primary-fixed-variant": "#2c4d40",
                "primary-fixed-dim": "#aacfbe",
                "background": "#fdf9f2"
            },
            "borderRadius": {
                "DEFAULT": "0.25rem",
                "lg": "0.5rem",
                "xl": "0.75rem",
                "2xl": "1rem",
                "3xl": "1.5rem",
                "full": "9999px"
            },
            "fontFamily": {
                "headline": ["Manrope", "sans-serif"],
                "body": ["Plus Jakarta Sans", "sans-serif"],
                "label": ["Plus Jakarta Sans", "sans-serif"]
            }
        }
    },
    plugins: [
        require('@tailwindcss/container-queries'),
        require('@tailwindcss/forms')
    ]
}
