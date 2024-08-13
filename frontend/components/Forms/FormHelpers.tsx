function getFieldErrors(name: string, errors: any, errorsFn: any) {
    let ers = null;
    let result = null;

    if (errorsFn) {
        ers = errorsFn({ name: name });
    } else {
        ers = errors;
    }

    if (!ers || ers.length === 0) {
        return null;
    }

    if (Array.isArray(ers)) {
        result = ers.join('. ');
    } else {
        result = ers;
    }

    return result;
}

function setFormState(currentFormState: any, setState: any, e: any) {
    if (!currentFormState) {
        throw new Error('current form state null');
    }

    if (!currentFormState.hasOwnProperty(e.name)) {
        throw new Error(`form state does not have property "${e.name}"`);
    }

    let newstate = {...currentFormState, [e.name]: e.newValue};

    setState(newstate);
}

function getFieldErrorFromApi(apiError: any, inputArg: any) {
    if (!apiError || !apiError.fieldErrors) {
        return;
    }

    const fieldErrors = apiError.fieldErrors;
    const name = inputArg.name.toLowerCase();

    let ferrors = fieldErrors.find((fe: any) => fe.fieldName.toLowerCase() === name);
    return ferrors?.errors;
}

function getInputValue(name: string, value: any, valueFn: any) {
    if (!name) {
        throw new Error('name is empty');
    }

    if (value === null || value === undefined) {
        if (valueFn) {
            return valueFn({ name: name });
        }

        return value;
    }

    return value;
}

function getApiErrorsForm(apiResult: any): any {
    let result = {
        errors: apiResult?.error?.errors || [],
        fieldErrors: apiResult?.error?.fieldErrors || []
    };

    return result;
}

export { getFieldErrors, getInputValue, getFieldErrorFromApi, getApiErrorsForm, setFormState }