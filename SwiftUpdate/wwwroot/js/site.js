document.getElementById('uploadForm').addEventListener('submit', function () {
    // Add 'loading' class to body to hide other content
    document.getElementById('main').classList.add('loading');
    // Show loader
    document.getElementById('loader').style.display = 'block';
});


function confirmDelete() {
    swal("Are you sure you want to delete this application?", {
        buttons: {
            cancel: "No",
            confirm: "Yes"
        },
    })
        .then((value) => {
            if (value) {
                document.getElementById('deleteForm').submit();

            }
        });
}