var homeIndex = function () {
    let form = document.getElementById("form");
    form.addEventListener("submit", onSubmit);

    function onSubmit(e) {
        e.preventDefault();

        let formData = Object.fromEntries(new FormData(form));

        fetch("/api/Integration/NugetCreateProject", {
            method: "POST",
            headers: {
                "content-type": "application/json"
            },
            body: JSON.stringify(formData)
        }).then(r => r.json());


        console.log('submit');
    }
};

window.dndocs.onReady(homeIndex);