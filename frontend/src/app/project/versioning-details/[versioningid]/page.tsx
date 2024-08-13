'use client'
import { FID } from "@/app/help/HelpData";
import DetailsList, { ItemCfg } from "@/components/DetailsList";
import Layout from "@/components/Layout";
import PageLoading from "@/components/PageLoading";
import SimpleTable, { TableCfg } from "@/components/SimpleTable";
import SystemMessagesList from "@/components/SystemMessagesList";
import ProjectVersioningInfoDto from "@/services/APIModel/Project/ProjectVersioningInfoDto";
import UseApi, { useQueryEffect } from "@/services/Api";
import ProjectVersioningDto from "@/services/ApiDTO/Project/ProjectVersioningDto";
import { Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle, Divider, Grid, Paper } from "@mui/material";
import Urls from "config/Urls";
import consts from "config/const";
import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { useEffect, useState } from "react";

export default function Page() {
    const params = useParams();
    const versioningId = +(params?.versioningid ?? -1);
    const api = UseApi();
    const router = useRouter();

    const [pvreq, setPvreq] = useState<any>({
        pageNo: 0,
        projectVersioningId: versioningId
    });

    const [deleteState, setDeleteState] = useState<any>({
        loading: false,
        show: false
    });

    useEffect(() => {
        setPvreq({ pageNo: 0, projectVersioningId: versioningId });
    }, [versioningId])

    const { loading: ldver, result: verResult } = useQueryEffect(() => api.ProjectManage_GetProjectVersioningById(versioningId));
    const { loading: ldtags, result: tagsResult } = useQueryEffect(() => api.ProjectManage_GetProjectsVersioningInfo(pvreq.projectVersioningId, pvreq.pageNo), [pvreq]);
    const helpid = FID.projectVersioning;
    const loading = ldtags || ldver || deleteState.loading;

    const latestsDocsUrl = Urls.robiniadocsUrl(verResult?.urlPrefix);
    const shieldsBadgeMd = `[![Static Badge](https://img.shields.io/badge/API%20Docs-${consts.AppNameUI}-190088?logo=readme&logoColor=white)](${latestsDocsUrl})`
    const shieldsIoImgUrl = `https://img.shields.io/badge/API%20Docs-${consts.AppNameUI}-190088?logo=readme&logoColor=white`

    const listcfg: ItemCfg<ProjectVersioningDto>[] = [
        { title: 'Id', val: (v) => v.id, helpFieldId: helpid.id },
        { title: 'Autoupgrade', val: (v) => v.autoupgrage, helpFieldId: helpid.autoupgrade },
        { title: 'Url Prefix', val: v => v.urlPrefix, helpFieldId: helpid.urlprefix },
        { title: 'Git Repo Url', val: (v) => v.gitDocsRepoUrl, helpFieldId: helpid.gitrepourl },
        { title: 'Git Branch Name', val: (v) => v.gitDocsBranchName, helpFieldId: helpid.gitbranchname },
        { title: 'Git Docs Path', val: (v) => v.gitDocsRelativePath, helpFieldId: helpid.gitdocspath },
        { title: 'Git README path', val: (v) => v.gitHomepageRelativePath, helpFieldId: helpid.gitreadmepath },
        { title: 'Project Name', val: (v) => v.projectName, helpFieldId: helpid.projectname },
        { title: 'Project Website Url', val: (v) => v.projectWebsiteUrl, helpFieldId: helpid.projectwebsiteurl },
        {
            title: 'Nuget Packages',
            stp: { component: 'pre' }, val: v => v.nugetPackages?.map(p => `${p.identityId}`).join('\r\n'),
            helpFieldId: helpid.nugetpackages
        },
        { title: 'Always-Newest Docs URL', val: v => <Link href={latestsDocsUrl}>{latestsDocsUrl}</Link>, helpFieldId: helpid.alwaysNewestDocsUrl },
        { title: 'Shields IO Image', val: v => <img src={shieldsIoImgUrl} /> },
        { title: 'Shields IO Ready Markdown', val: v => shieldsBadgeMd },
        { title: 'Delete', val: v => <Button fullWidth color="error" onClick={onDeleteClick}>Delete</Button> },
    ];

    function onDeleteClick() {
        setDeleteState({...deleteState, show: true});
    }

    function DeleteProjectVersioning() {
        setDeleteState({...deleteState, show: false, loading: true});

        api.ProjectManage_DeleteProjectVersioning(versioningId).then(r => {
            setDeleteState({...deleteState, show: false, loading: false});
            if (r.success) {
                router.push(Urls.account.details);
            }
        });
    }

    function ConfirmDeleteDialog() {
        if (!deleteState.show) {
            return;
        }

        return (
            <Dialog open={true}>
                <DialogTitle color="error">
                    Confirm Delete Project Versioning
                    <Divider />
                </DialogTitle>
                <DialogContent>
                    <DialogContentText>
                        Are You sure You want do delete this Project Versioning?
                        Projects (for safety purpose) will not be deleted.
                        After deleting versioning all projects must be deleted manually on My Account page.
                    </DialogContentText>
                </DialogContent>
                <DialogActions>
                    <Button variant="outlined" color="error" onClick={() => setDeleteState({...deleteState, show: false})}>Cancel</Button>
                    <Button variant="contained" color="error"  onClick={DeleteProjectVersioning}>Confirm</Button>
                </DialogActions>
            </Dialog>
        )
    }

    const tagsTable: TableCfg<ProjectVersioningInfoDto> = {
        data: tagsResult?.data || [],
        cols: [
            { title: 'Tag', val: t => t.gitTagName },
            { title: 'Project Details', val: t => t.projectId ? <a href={Urls.project.details(t.projectId)}>Project Details</a> : '-' },
            { title: 'Generate', val: t => t.projectId ? '-' : <Button onClick={() => onAutogenerateVersionClick(t)}>auto-generate</Button> }
        ],
        rowidkey: 'gitTagName',
        pagination: {
            count: tagsResult?.rowsCount || 0,
            page: tagsResult?.currentPage || 0,
            rowsPerPage: tagsResult?.rowsPerPage || 0,
            rowsPerPageOptions: [10]
        },
        events: {
            onPageChange: (e: any) => { setPvreq({ ...pvreq, pageNo: e.newValue }); }
        }
    }

    function onAutogenerateVersionClick(tagInfo: ProjectVersioningInfoDto) {
        api.CreateProjectVersionByGitTag(versioningId, tagInfo.gitTagName).then(r => {
            if (r.success) {
                router.push(Urls.account.bgjobWaiting(r.result));
            }
        });
    }

    return (
        <Layout title="Versioning Details">
            <PageLoading open={loading} />
            {ConfirmDeleteDialog()}
            <Grid container spacing={2}>
                <Grid item md={6} sm={12}>
                    <DetailsList loading={ldver} itemsConfig={listcfg} data={verResult}></DetailsList>
                </Grid>
                <Grid item md={6} sm={12}>
                    <Paper>
                        <SimpleTable loading={ldtags} config={tagsTable}></SimpleTable>
                    </Paper>
                </Grid>
                <Grid item md={12}>
                    <SystemMessagesList filter={{ projectVersioningId: versioningId }} />
                </Grid>
            </Grid>
        </Layout>
    );
}