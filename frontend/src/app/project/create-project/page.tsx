'use client';
import { Alert, Backdrop, Box, Button, Card, Checkbox, Divider, FormControl, FormControlLabel, FormHelperText, FormLabel, Input, InputLabel, OutlinedInput, Paper, Radio, RadioGroup, Snackbar, TextField, Typography } from "@mui/material";
import { useState } from "react";
import Api from '../../../../services/Api';
import PageLoading from "../../../../components/PageLoading";
import UploadFilesButton from "../../../../components/UploadFilesButton";
import RequestProjectModel from "../../../../services/APIModel/MyAccount/RequestProjectModel";
import getConfig from "next/config";
import { CheckBox } from "@mui/icons-material";
import UseApi from "../../../../services/Api";
import { useRouter } from "next/navigation";
import Urls from "config/Urls";
import InputCheckbox from "@/components/Forms/InputCheckbox";
import InputText from "@/components/Forms/InputText";
import InputNugetPackages, { NupkgStringToArray } from "@/components/Forms/InputNugetPackages";
import { getFieldErrorFromApi } from "@/components/Forms/FormHelpers";
import InputRadioGroup from "@/components/Forms/InputRadioGroup";
import { nof } from "@/services/Helpers";
import InputSelect from "@/components/Forms/InputSelect";
import Layout from "@/components/Layout";
import Form from "@/components/Forms/Form";
import InputGithubRepository from "@/components/Forms/InputGithubRepository";

interface FormState {
    filessource: string,
    projectName: string,
    description: string,
    urlPrefix: string,
    githubUrl: string,
    docfxTemplate: string,
    gitMdRepoUrl: string,
    gitMdRelativePathDocs: string,
    gitMdRelativePathReadme: string,
    gitMdBranchName: string,
    mddocsarticlesconfig: string,
    nugetPackages: string,
    psAutorebuild: boolean,
    customUrlPrefix: boolean,
    projectFiles: any
}

export default function Index() {
    const [formInput, setFormInput] = useState<FormState>({
        filessource: 'filesfromnuget',
        projectName: '',
        description: '',
        urlPrefix: '',
        githubUrl: '',
        docfxTemplate: '<DEFAULT>',
        gitMdRepoUrl: '',
        gitMdRelativePathDocs: 'docs',
        gitMdRelativePathReadme: 'README.md',
        gitMdBranchName: 'main',
        mddocsarticlesconfig: 'include',
        nugetPackages: '',
        psAutorebuild: true,
        projectFiles: null,
        customUrlPrefix: false,
    });

    const api = UseApi();
    const router = useRouter();

    const [formErrors, setFormErrors] = useState<any>({});
    const [submitSuccess, setSubmitSuccess] = useState<boolean>(false);
    const [isLoading, setIsLoading] = useState<boolean>(false);

    const docfxTemplates = [
        { text: 'DEFAULT (modern)', value: '<DEFAULT>' },
        { text: 'discordfx', value: 'discordfx' },
        { text: 'docfx-minimal-main', value: 'docfx-minimal-main' },
        { text: 'material', value: 'material' },
        { text: 'singulinkfx', value: 'singulinkfx' },
    ]

    const onInput = function (e: any) {
        let name = e.name;
        let newv = e.newValue;
        let news = { ...formInput, [name]: newv };

        if ((name === 'projectName' && !news.customUrlPrefix) ||
            (name === 'customUrlPrefix' && !newv)) {
            news.urlPrefix = news.projectName;
        }

        if (name === 'filessource' && newv === 'filesfromdisk') {
            news.nugetPackages = '';
        }

        if (name === 'projectFiles') {
            news.projectFiles = e.target.files;
        }

        news.urlPrefix = normalizeUrlPrefix(news.urlPrefix);

        setFormInput(news);
    }

    function formVal(arg: any) {
        if (!formInput.hasOwnProperty(arg.name)) {
            throw Error('prop not exists: ' + arg.name);
        }

        return (formInput as any)[arg.name];
    }

    const normalizeUrlPrefix = function (val: string) {
        if (!val) {
            return '';
        }

        val = val.toLowerCase();
        let result = '';

        for (let i = 0; i < val.length; i++) {
            let c = val[i];

            if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c === '-') {
                result += val[i];
            } else {
                result += '-';
            }
        }

        if (result.length > 31) { result = result.substring(0, 30); }

        return result;
    }

    const onSubmit = function (e: any) {
        e.preventDefault();
        setIsLoading(true);

        let ignore = [
            'nugetPackages',
            'projectFiles',
            'filessource',
            'mddocsgithubconfig',
            'mddocsarticlesconfig',
        ];

        let request = {} as RequestProjectModel;

        for (var k in formInput) {
            if (ignore.includes(k)) {
                continue;
            }

            (request as any)[k] = (formInput as any)[k];
        }

        request.nugetPackages = NupkgStringToArray(formInput.nugetPackages)?.map(t => t.identityId);
        request.docfxTemplate = request.docfxTemplate === '<DEFAULT>' ? '' : request.docfxTemplate;

        if (!formInput.gitMdRepoUrl) {
            request.gitMdRepoUrl = 
            request.gitMdBranchName = 
            request.gitMdRelativePathReadme =
            request.gitMdRelativePathDocs = '';
        }

        api.ProjectManage_RequestProject(request)
            .then((r: any) => {
                setIsLoading(false);
                setFormErrors(r.error);
                if (r.success) {
                    setSubmitSuccess(true);
                    router.push(Urls.account.bgjobWaiting(r.result));
                } else {
                    window.scrollTo({
                        top: 0,
                        left: 0,
                        behavior: "smooth",
                    });
                }
            });
    }

    const submitOkSnackbar = function () {
        return (<Snackbar open={submitSuccess} autoHideDuration={6000} message="Submit success" />);
    }

    let filesFromNuget = formInput.filessource === 'filesfromnuget';

    const getFieldError = function (field: string) {
        if ((formErrors?.fieldErrors?.length ?? 0) === 0) {
            return null;
        }

        return formErrors?.fieldErrors.find((e: any) => e.fieldName.toLowerCase() === field);
    }

    const hasError = function (field: string) {
        return getFieldError(field) != null;
    }

    const errorText = function (field: string) {
        const errors = getFieldError(field)?.errors;

        if (errors?.length > 0) {
            return errors.join('. ');
        }
    }

    const errorsFn = function (arg: any) {
        return getFieldErrorFromApi(formErrors, arg);
    }

    return (
        <>
            {submitSuccess && submitOkSnackbar()}
            <PageLoading open={isLoading} />
            <Layout>
                <Form submitButton={{ onSubmit: onSubmit }} title="Create new project">
                    <InputText
                        name="projectName"
                        errorsFn={errorsFn}
                        valueFn={formVal}
                        onChange={onInput}
                        id="projectname"
                        label="Project name" />

                    <InputText
                        errorsFn={errorsFn}
                        onChange={onInput}
                        valueFn={formVal}
                        label="Description"
                        name="description" />

                    <InputCheckbox
                        name="customUrlPrefix"
                        onChange={onInput}
                        label="Custom url prefix" />

                    <InputText
                        name="urlPrefix"
                        disabled={!formInput.customUrlPrefix}
                        errorsFn={errorsFn}
                        valueFn={formVal}
                        onChange={onInput}
                        id="robiniaurlprefix"
                        type="text"
                        label="Robinia Url Prefix"
                        value={formInput.urlPrefix} />

                    <InputText
                        name="githubUrl"
                        errorsFn={errorsFn}
                        valueFn={formVal}
                        onChange={onInput}
                        id="githuburl"
                        label="Project Github Url" />

                    <InputSelect
                        name="docfxTemplate"
                        label="Docfx Template"
                        valueFn={formVal}
                        onChange={onInput}
                        items={docfxTemplates}
                        helperText={(<>See docfx templates: <a target="_blank" href="https://dotnet.github.io/docfx/extensions/templates.html">Docfx Templates URL</a></>)}
                    >
                    </InputSelect>

                    <InputGithubRepository
                        valueFn={formVal}
                        onChange={onInput}
                        errorsFn={errorsFn}
                        label="Github url with *.md documentation (can be same as project github repository)"
                        name="gitMdRepoUrl" />

                    <Box sx={{ display: formInput.gitMdRepoUrl ? 'block' : 'none' }}>
                        <InputText
                            name="gitMdBranchName"
                            label="Branch name"
                            valueFn={formVal}
                            onChange={onInput}
                            errorsFn={errorsFn} />

                        <InputText
                            valueFn={formVal}
                            onChange={onInput}
                            errorsFn={errorsFn}
                            name="gitMdRelativePathDocs"
                            label="Md articles repo relative path"
                            helperText="Folder path where *.md docs are located,
                            for example 'docs', 'api-documentation', 'other/documentation', 'files/examples/docs' etc.
                            Leave empty if project does not have .md docs" />

                        <InputText
                            onChange={onInput}
                            valueFn={formVal}
                            errorsFn={errorsFn}
                            name="gitMdRelativePathReadme"
                            label="README.md relative path"
                            helperText="Relative path (relative to git directory) e.g. 'README.md, 'Readme.md', 'docs/README.md', 'documentation/README.md' etc.
                                Leave empty if there is not README.md file or do not include README.md as main page" />
                    </Box>

                    <FormLabel id="files-source">Files Source</FormLabel>

                    <RadioGroup
                        onChange={onInput}
                        row name="filessource" defaultValue="filesfromnuget">
                        <FormControlLabel value="filesfromdisk" control={<Radio disabled={true} />} label={'Upload files from disk'} />
                        <FormControlLabel value="filesfromnuget" control={<Radio />} label={'Nuget Package'} />
                    </RadioGroup>

                    <InputNugetPackages
                        valueFn={formVal}
                        onChange={onInput}
                        errorsFn={errorsFn}
                        name="nugetPackages"
                        label="Nuget Package(s) without version"
                        id="nugetpackages"
                        disabled={!filesFromNuget}
                    />

                    <UploadFilesButton
                        disabled={filesFromNuget}
                        onChange={onInput}
                        name="projectFiles"
                        hasError={hasError('projectfiles')}
                        errorText={errorText('projectfiles')} />
                    <Divider />

                    <InputCheckbox
                        valueFn={formVal}
                        onChange={onInput}
                        name="psAutorebuild"
                        label="Auto-rebuild github docs"
                        helperText="Periodically check changes of *.md docs and automatically rebuild latest documents" />

                    <Alert severity="info" sx={{ mt: 1 }}>
                        Depending on amount of Nuget Packages and Nuget Packages sizes creating a project can took 1 minute.<br />
                        If Validation error occured, please check if project was not created in My Account and delete it.
                        Then try to create project again
                    </Alert>
                </Form>
            </Layout>
        </>
    );
}