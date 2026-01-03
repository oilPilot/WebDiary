
// Code for themes

function toggleTheme(theme) {
        document.documentElement.setAttribute('data-bs-theme', theme);
        localStorage.setItem('theme', theme);
    }
function toggleLastTheme() {
        if (!localStorage.getItem('theme'))
            localStorage.setItem('theme', window.matchMedia('(prefers-color-scheme: dark)').matches);
        document.documentElement.setAttribute('data-bs-theme', localStorage.getItem('theme'));
    }
function getTheme() {
        if (!localStorage.getItem('theme'))
            localStorage.setItem('theme', window.matchMedia('(prefers-color-scheme: dark)').matches);
        return localStorage.getItem('theme')
    }
function getAndSetLastTheme() {
        var lastTheme = localStorage.getItem('theme');
        if (!lastTheme)
            lastTheme = window.matchMedia('(prefers-color-scheme: dark)').matches;
        document.documentElement.setAttribute('data-bs-theme', lastTheme);
        return lastTheme;
    }

// Code for Time Managing (Retrieve Local)
function getClientTime() {
    const now = new Date();
    return {
        isoUtc: now.toISOString(),
        offsetMinutes: -now.getTimezoneOffset()
    };
}

// Code for Quill Editor

function initializeQuillEditor() {
    var quill = new Quill('#editor', {
        theme: 'snow',
        placeholder: 'Start writing your diary...',
        modules: {
            toolbar: [
                [{ header: [1, 2, 5, false] }],
                ['bold', 'italic', 'underline', 'strike'],
                ['color', 'background'],
                ['ordered'],
                [{ 'list': 'ordered' }, { 'list': 'bullet' }],
                [{ 'indent': '-1' }, { 'indent': '+1' }],
                ['clean']
            ]
        }
    });
    // Auto-save on text change
    quill.on('text-change', function() {
        localStorage.setItem('NewDiaryAutoSavedText', quill.root.innerHTML);
    });

    // Auto-load saved text
    var value = localStorage.getItem('NewDiaryAutoSavedText');
    if (value) {
        quill.root.innerHTML = value;
    }
}

function getQuillContent() {
    var quill = Quill.find(document.getElementById('editor'));
    return quill.root.innerHTML;
}

// Code for Export

async function downloadDiaryPdf(url) {
    const token = localStorage.getItem('token');
    
    fetch(`${url}`, {
    method: "GET",
    headers: {
        Authorization: `Bearer ${token.slice(1, -1)}`
    }
    })
    .then(res => {
        if (!res.ok) throw new Error("Failed to download");
        return res.blob();
    })
    .then(blob => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = url;
        a.download = "MyDiary.pdf";
        document.body.appendChild(a);
        a.click();
        a.remove();
        window.URL.revokeObjectURL(url);
    })
    .catch(console.error);
}


