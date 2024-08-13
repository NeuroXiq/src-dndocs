'use client';
import Form from "@/components/Forms/Form";
import { setFormState } from "@/components/Forms/FormHelpers";
import InputNugetPackages, { NupkgStringToArray } from "@/components/Forms/InputNugetPackages";
import InputText from "@/components/Forms/InputText";
import Layout from "@/components/Layout";
import PageLoading from "@/components/PageLoading";
import UseApi, { useQueryEffect } from "@/services/Api";
import { Alert, Typography } from "@mui/material";
import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import ProjectVersioningDto from "@/services/ApiDTO/Project/ProjectVersioningDto";
import InputSelect from "@/components/Forms/InputSelect";
import Urls from "config/Urls";

export default function Page(props: any) {
    const [form, setForm] = useState<any>({
        projectVersioningId: -1,
        gitTagName: '',
        nugetPackages: '',
    });

    const [submitResult, setSubmitResult] = useState<any>(null);
    const [tags, setTags] = useState<any>([]);
    const [submitLoading, setSubmitLoading] = useState<boolean>(false);

    const api = UseApi();
    const params = useParams();
    const router = useRouter();
    const versioningid = +(params?.versioningid || -1);

    const { loading: versloading, result, response } = useQueryEffect<ProjectVersioningDto>(() => api.ProjectManage_GetProjectVersioningById(versioningid));
    const { loading: tagloading, result: tagresult } = useQueryEffect<string[] | null>(() => api.ProjectManage_GetVersioningGitTags(versioningid));

    useEffect(() => {
        if (tagloading || versloading) {
            return;
        }
        const gittags = (tagresult || []).map((tag: any) => { return { text: tag, value: tag }}).sort().reverse();
        const first = gittags.length > 0 ? gittags[0].value : '';

        setTags(gittags);
        setForm({...form, gitTagName: first, projectVersioningId: versioningid});

    }, [tagloading, tagresult])

    const loading = versloading || tagloading || submitLoading;

    function valueFn(e: any) {
        return form[e.name];
    }
    function onChange(e: any) { setFormState(form, setForm, e); }

    function onSubmit() {
        const model = { ...form };
        model.nugetPackages = NupkgStringToArray(model.nugetPackages);
        
        setSubmitLoading(true);
        api.ProjectManage_CreateProjectVersion(model).then(r => {
            setSubmitResult(r);
            setSubmitLoading(false);

            if (r.success) {
                router.push(Urls.account.bgjobWaiting(r.result))
            }
        });
    }

    let title = `Create Project Version` + (!result ? '' : ` / ${result.projectName} | ${result.id}`);

    return (
        <Layout>
            <PageLoading open={loading} />
            {(!loading && tagresult?.length === 0) && <Alert severity="error">There is no git tags in repository, version cannot be created</Alert>}
            <Form title={title} submitButton={{ onSubmit: onSubmit }} autoApiResult={submitResult}>
                <InputSelect name="gitTagName" label="Git Tag Name" onChange={onChange} items={tags} valueFn={valueFn}/>
                <InputNugetPackages name="nugetPackages" label="Nuget Package(s) with version(s)" onChange={onChange} valueFn={valueFn}/>
            </Form>
        </Layout>
    )
}