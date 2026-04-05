import { getById, isHiddenById, setInnerHTMLById, } from 'js-shared/tools';


console.log('init home index');
let stepper, form, packageName = null, packageVersion = null;

var Stepper = function (element) {
    let container = element;
    let steps = container.children;
    let inProgressTimeout = null;

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
        let step = steps[arg.index];

        step.classList.remove('is-success');
        step.classList.remove('is-danger');
        step.classList.remove('is-info');
        step.querySelector('.dots')?.remove();

        clearTimeout(inProgressTimeout);

        if (arg.status == 'success') {
            className = 'is-success';
        } else if (arg.status == 'error') {
            className = 'is-danger';
        } else if (arg.status == 'info') {
            className = 'is-info';
        } else if (arg.status == 'inprogress') {
            className = 'is-info';
            animationInProgress(steps[arg.index], 0);
        } else if (arg.status === 'default') {

        }

        step.classList.add(className);
    }

    return {
        setStep: setStep,
        reset: reset
    }
}

form = document.getElementById("form");
stepper = new Stepper(document.getElementById('progress-stepper'));

form.addEventListener("submit", onSubmit);

if (form.elements["packagename"].value?.length > 0 &&
    form.elements["packageversion"].value?.length > 0) {
    form.requestSubmit();
}

function onSubmit(e) {
    e.preventDefault();
    reset();
    let formData = Object.fromEntries(new FormData(form));
    packageName = formData.packagename;
    packageVersion = formData.packageversion;

    requestStatus().then(r => {
        let next = Promise.resolve();

        if (!r.result) {
            next = dnfetch("/api/Integration/NugetCreateProject", {
                method: "POST",
                headers: {
                    "content-type": "application/json"
                },
                body: JSON.stringify(formData)
            });
        }

        next.then(() => runRefreshingStatus());
    });
}

function dnfetch(url, paramsObject) {
    let fetchPromise = fetch(url, paramsObject);

    fetchPromise = fetchPromise.then(r => {
        let ok = r.ok;
        return r.json().then(result => {
            if (ok) {
                return Promise.resolve(result);
            } else {
                return Promise.reject(result);
            }
        });
    });

    fetchPromise.catch(e => {
        console.error('catch', e);

        setError(e.error);
    });

    return fetchPromise;
}

function reset() {
    stepper.setStep({ index: 0, status: 'default' });
    stepper.setStep({ index: 1, status: 'default' });
    stepper.setStep({ index: 2, status: 'default' });

   isHiddenById('section-errors', true);
   isHiddenById('id-success-section', true);
   isHiddenById('section-badge', true)
}

function setError(message) {
   isHiddenById('section-errors', false);
    setInnerHTMLById('error-message',
        'Error occured during request processing. <br />' + message +
        '<br /> You can report this issue on github.');
}

function runRefreshingStatus() {
    requestStatus().then(r => {
        console.log('refresh status', r);
        r = r.result;

        reset();

        if (!r) {
            return;
        }

        if (r.state === 2 || r.state === 3) {
            setTimeout(() => runRefreshingStatus(), 1000);
        }

        if (r.state === 1) {
            // online
            stepper.setStep({ index: 0, status: 'success' });
            stepper.setStep({ index: 1, status: 'success' });
            stepper.setStep({ index: 2, status: 'success' });
            isHiddenById('id-success-section', false);
            getById('id-success-url').href = r.projectApiFolderUrl;
            getById('badge-href').href = r.projectApiFolderUrl;
            setInnerHTMLById('id-success-text', r.projectApiFolderUrl);
            isHiddenById('section-badge', false);

            getById('badge-code').innerHTML =
                '[![Static Badge](https://img.shields.io/badge/API%20Docs-DNDocs-190088?logo=readme&logoColor=white)]' +
                `(${r.projectApiFolderUrl})`

        } else if (r.state === 2) {
            // waiting to start build
            stepper.setStep({ index: 0, status: 'inprogress' });
        } else if (r.state === 3) {
            // building
            stepper.setStep({ index: 0, status: 'success' });
            stepper.setStep({ index: 1, status: 'inprogress' });
        } else {
            stepper.setStep({ index: 0, status: 'success' });
            stepper.setStep({ index: 1, status: 'error' });
            setError('failed to generate documentation. Build process failed.');
        }
    });
}

function requestStatus() {
    return dnfetch(`/api/integration/nugetcreateprojectcheckstatus?packageName=${packageName}&packageVersion=${packageVersion}`, {
        method: 'GET'
    });
}
