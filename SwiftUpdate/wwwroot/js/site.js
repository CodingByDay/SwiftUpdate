document.getElementById('uploadForm').addEventListener('submit', function () {
    // Add 'loading' class to body to hide other content
    document.getElementById('main').classList.add('loading');
    // Show loader
    document.getElementById('loader').style.display = 'block';
});