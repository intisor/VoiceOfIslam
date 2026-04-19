window.voiceOfIslamStorage = {
    get: function(key) {
        return window.localStorage.getItem(key);
    },
    set: function(key, value) {
        window.localStorage.setItem(key, value);
    },
    copyText: function(text) {
        if (navigator.clipboard) {
            navigator.clipboard.writeText(text);
        }
    },
    downloadFile: function(filename, mimeType, content) {
        const blob = new Blob([content], { type: mimeType });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    }
};
