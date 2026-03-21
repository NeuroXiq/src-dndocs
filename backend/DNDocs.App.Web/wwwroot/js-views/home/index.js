import { toggleHide, toggleHideById, setInnerHTMLById } from '/js-shared/tools.js';


var homeIndex = function () {
    var Stepper = function (element) {
        let container = element;
        let steps = container.children;
        let inProgressTimeout = null;

        function resetStep(index) {
            steps[index].classList.remove('is-success');
            steps[index].classList.remove('is-danger');
            steps[index].classList.remove('is-info');
            clearTimeout(inProgressTimeout);
        }

        function animationInProgress(stepElement, i) {

            let dotsDiv = stepElement.querySelector('.dots');

            if (!dotsDiv) {
                dotsDiv = document.createElement('span');
                dotsDiv.classList.add('dots');
                stepElement.appendChild(dotsDiv);
            }

            dotsDiv.innerHTML = ' ' + '.'.repeat((i % 4));
            inProgressTimeout = setTimeout(() => { animationInProgress(stepElement, i + 1) }, 500);
        }

        function setStep(arg) {
            let className = null;

            resetStep(arg.index);

            if (arg.status == 'success') {
                className = 'is-success';
            } else if (arg.status == 'error') {
                className = 'is-danger';
            } else if (arg.status == 'info') {
                className = 'is-info';
            } else if (arg.status == 'inprogress') {
                className = 'is-info';
                animationInProgress(steps[arg.index], 0);
            }

            container.children[arg.index].classList.add(className);
        }

        return {
            setStep: setStep
        }
    }

    let form = document.getElementById("form");
    form.addEventListener("submit", onSubmit);

    let stepper = new Stepper(document.getElementById('progress-stepper'));

    stepper.setStep({ index: 2, status: 'inprogress' });

    function onSubmit(e) {
        reset();
        e.preventDefault();
        let formData = Object.fromEntries(new FormData(form));

        dnfetch(`/api/integration/nugetcreateprojectcheckstatus?packageName=${formData.packageName}&packageVersion=${formData.packageVersion}`, {
            method: 'GET'
        }).then(r => {
            console.log(r);
        });

        return;
        let f2 = fetch("/api/Integration/NugetCreateProject", {
            method: "POST",
            headers: {
                "content-type": "application/json"
            },
            body: JSON.stringify(formData)
        });
    }

    function dnfetch(url, paramsObject) {
        let fetchPromise = fetch(url, paramsObject);

        fetchPromise = fetchPromise.then(r => {
            if (r.ok) {
                return r.json();
            } else {
                console.error(r);

                return r.json()
                    .then(errorResult => {
                        console.error(errorResult);

                        toggleHideById('section-errors', false);
                        setInnerHTMLById('error-message', 'Error occured during request processing. <br />' + errorResult?.error);

                        return Promise.reject();
                    });
            }
        });

        fetchPromise.catch(e => {
            console.error('catch', e);

            toggleHideById('section-errors', false);
            setInnerHTMLById('error-message', 'Error occured during request processing. <br />' + e.message);
        });

        return fetchPromise;
    }

    function reset() {
        toggleHideById('section-errors', true);
    }

    function refreshStatus() {

    }
};


window.dndocs.onReady(homeIndex);