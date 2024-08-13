'use client';
import Form from "@/components/Forms/Form";
import { getFieldErrorFromApi } from "@/components/Forms/FormHelpers";
import InputCheckbox from "@/components/Forms/InputCheckbox";
import InputNugetPackages, { NupkgStringToArray } from "@/components/Forms/InputNugetPackages";
import InputText from "@/components/Forms/InputText";
import Layout from "@/components/Layout";
import CreateProjectVersioningModel from "@/services/APIModel/Project/CreateProjectVersioningModel";
import UseApi from "@/services/Api";
import { useState } from "react";
import { useParams, useRouter } from "next/navigation";
import Urls from "config/Urls";
import PageLoading from "@/components/PageLoading";
import InputGithubRepository from "@/components/Forms/InputGithubRepository";
import { IconButton } from "@mui/material";

export default function Page(props: any) {
    const [form, setForm] = useState<any>({
        projectName: '',
        projectWebsiteUrl: '',
        urlPrefix: '',
        gitDocsRepoUrl: '',
        gitDocsBranchName: 'main',
        gitDocsRelativePath: 'docs',
        gitHomepageRelativePath: 'README.md',
        autoupgrade: true,
        nugetPackages: ''
    });

    const [submitResult, setSubmitResult] = useState<any>(null);
    const [loading, setLoading] = useState<boolean>(false);
    const api = UseApi();
    const [apiResult, setApiResult] = useState<any>(null);
    const router = useRouter();

    function onSubmit(e: any) {
        e.preventDefault();
        const nugetPackages = NupkgStringToArray(form.nugetPackages);
        const model = { ...form } as CreateProjectVersioningModel;
        model.nugetPackages = nugetPackages;

        setLoading(true);
        api.ProjectManage_CreateProjectVersioning(model)
            .then(t => {
                setLoading(false);
                setSubmitResult(t);
                if (t.success) {
                    router.push(Urls.account.details);
                }
                setApiResult(t);
            });
    }

    const inputProps = {
        valueFn: valueFn,
        onChange: onInputChange,
        errorsFn: (arg: any) => getFieldErrorFromApi(apiResult?.error, arg)
    }

    function valueFn(arg: any) {
        if (!form.hasOwnProperty(arg.name)) {
            throw new Error('error ' + arg.name);
        }
        return form[arg.name];
    }

    function onInputChange(arg: any) {
        setForm({ ...form, [arg.name]: arg.newValue });
    }

    return (
        <Layout>
            <PageLoading open={loading}/>
            <Form  title="Create Versioning" submitButton={{ onSubmit: onSubmit }} autoApiResult={submitResult}>
                <InputText label="Name" name="projectName" {...inputProps}></InputText>
                <InputText label="Project Website Url" name="projectWebsiteUrl" {...inputProps}></InputText>
                <InputText label="Url prefix" name="urlPrefix" {...inputProps} />
                <InputGithubRepository name="gitDocsRepoUrl" {...inputProps}/>
                <InputText label="Git Docs Branch Name" name="gitDocsBranchName" {...inputProps}></InputText>
                <InputText label="Git Docs directory path" name="gitDocsRelativePath" {...inputProps}></InputText>
                <InputText label="Git Docs Homepage file path" name="gitHomepageRelativePath" {...inputProps}></InputText>
                <InputCheckbox name="autoupgrade" label="Automatic detect latest tags and publish docs" {...inputProps}/>
                <InputNugetPackages name="nugetPackages" label="Nuget Package(s) without version(s)" {...inputProps} />
            </Form>
        </Layout>
    )
}