'use client'
import { useParams, useRouter } from "next/navigation";
import Api from "../../../../../services/Api"
import useApiCallEffect from "../../../../../hooks/useApiCall";
import PageLoading from "../../../../../components/PageLoading";
import Layout from "../../../../../components/Layout";
import { Accordion, AccordionDetails, AccordionSummary, Box, Button, Card, CardContent, CardMedia, Divider, Grid, List, ListItemButton, ListItemIcon, ListItemText, Paper, Stack, Typography } from "@mui/material";
import BasicTable from "../../../../../components/BasicTable";
import DetailsList, { ItemCfg } from "../../../../../components/DetailsList";
import RefreshIcon from '@mui/icons-material/Refresh';
import OpenInNewIcon from '@mui/icons-material/OpenInNew';
import Urls from "../../../../../config/Urls";
import UseApi from "../../../../../services/Api";
import { useQueryEffect } from "../../../../../services/Api";
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import { EID, ProjectStatus, UIName } from '../../../../../services/ApiDTO/Enum/Enums';
import { dateTimeUI, dateUI } from "services/Helpers";
import Link from "next/link";
import { useContext, useMemo, useState } from "react";
import { GlobalAppContext, IGlobalAppContextValue } from "hooks/globalAppContext";
import SystemMessagesList from "@/components/SystemMessagesList";
import consts from "config/const";
import ProjectDto from "@/services/ApiDTO/Project/ProjectDto";
import { BgProjectHealthCheckStatus } from "@/services/ApiDTO/Enum/BgProjectHealthCheckStatus";
import { FID } from "@/app/help/HelpData";

export default function DocfxMain() {
    const projectid = +(useParams() || {}).projectid;
    const router = useRouter();
    const api = UseApi();
    const gc = useContext<IGlobalAppContextValue>(GlobalAppContext);
    // const { loading, result, error } = useApiCallEffect(() => Api.docfxGetMainInfo(projectid));
    const [loading, setLoading] = useState<boolean>(false);
    const [refresh, setRefresh] = useState<number>(1);
    const { loading: detailsLoading, result: rproject } = useQueryEffect<ProjectDto>(() => api.ProjectManage_GetProjectById(projectid), [refresh]);

    let project: any = {};
    let bgCheckOk = true;
    let statusOk = true;
    let lastBuildErrorText = 'Last Build Error';

    if (rproject) {
        bgCheckOk = rproject.bgHealthCheckHttpGetStatus === BgProjectHealthCheckStatus.HttpGetOk;
        statusOk = rproject.status === ProjectStatus.Active;
        const rdocsUrl = Urls.robiniadocsUrl(rproject.urlPrefix);
        const p = rproject;
        const shieldsBadgeMd =
            `[![Static Badge](https://img.shields.io/badge/API%20Docs-${consts.AppNameUI}-190088?logo=readme&logoColor=white)](${rdocsUrl})`

        const shieldsIoImgUrl = `https://img.shields.io/badge/API%20Docs-${consts.AppNameUI}-190088?logo=readme&logoColor=white`

        let nugetPackages = (p.projectNugetPackages || []).map((p: any) => `${p.identityId} ${p.identityVersion}`);

        project = {
            ...rproject,
            bgHealthCheckHttpGetStatus: UIName(EID.BgProjectHealthCheckStatus, rproject.bgHealthCheckHttpGetStatus),
            statusUI: UIName(EID.ProjectStatus, rproject.status),
            lastDocfxBuildTime: dateUI(rproject.lastDocfxBuildTime),
            bgHealthCheckHttpGetDateTime: dateUI(p.bgHealthCheckHttpGetDateTime),
            robiniadocsHref: <Link href={rdocsUrl}>{rdocsUrl}</Link>,
            shieldsBadgeImg: <img src={shieldsIoImgUrl} />,
            shieldsBadgeMd: shieldsBadgeMd,
            nugetPackages: nugetPackages.join('\r\n'),
            autoRebuildLatestNupkgRebuildDatetime: dateTimeUI(p.nupkgAutorebuildLastDateTime)
        };

        if (p.lastDocfxBuildErrorDateTime) {
            const buildErrorDateUI = dateTimeUI(p.lastDocfxBuildErrorDateTime);
            let a = dateUI(p.lastDocfxBuildErrorDateTime);
            let b = dateUI(new Date());
            if (dateUI(new Date()) === dateUI(p.lastDocfxBuildErrorDateTime)) {
                lastBuildErrorText += ' - TODAY: ' + buildErrorDateUI;
            } else {
                lastBuildErrorText += ' ' + buildErrorDateUI;
            }
        }
    }

    const hid = FID.project;

    let projectConfig: ItemCfg<ProjectDto>[] = [
        { title: 'Project Id', val: (p) => p.id, helpFieldId: hid.id },
        { title: 'Name', val: p => p.projectName, helpFieldId: hid.name },
        { title: 'Description', val: p => p.description, helpFieldId: hid.description },
        { title: 'Github Url', val: p => p.githubUrl, helpFieldId: hid.githubUrl },
        { title: 'Shields.io badge', key: 'shieldsBadgeImg' },
        {
            title: 'Shields.io Badge Markdown (ready to copy-paste to README.md)',
            key: 'shieldsBadgeMd',
            helpFieldId: hid.shieldsBadgeMd
        },
        {
            title: 'Current Nuget Pakcages',
            key: 'nugetPackages',
            helpFieldId: hid.nugetPackages,
            stp: { component: 'pre' } }
    ];

    let projectConfig2: ItemCfg<ProjectDto>[] = [
        { title: 'Last Build Time', val: p => p.lastDocfxBuildTime, helpFieldId: hid.lastDocfxBuildTime},
        { title: 'Background Health Date', val: p => p.bgHealthCheckHttpGetDateTime, helpFieldId: hid.bgHealthCheckHttpGetDateTime },
        {
            title: 'Background Health Status', val: p => p.bgHealthCheckHttpGetStatus,
            stp: bgCheckOk ? null : { color: 'error' },
            helpFieldId: hid.bgHealthCheckHttpGetStatus
        },
        { title: 'Url Prefix', key: 'urlPrefix', helpFieldId: hid.urlPrefix },
        {
            title: 'Status',
            key: 'statusUI',
            stp: statusOk ? null : { color: 'warning' },
            helpFieldId: hid.status },
        { title: 'URL', key: 'robiniadocsHref', helpFieldId: hid.docsUrl },
        {
            title: 'Auto rebuild nuget packages last rebuild date', val: p => p.nupkgAutorebuildLastDateTime
            , helpFieldId: 'project_autoRebuildLatestNupkgRebuildDatetime'
        }
    ]

    const detailsListsMemo = useMemo(renderDetailsLists, [rproject]);

    function renderDetailsLists() {
        return (
            <>
                <Grid item xs={6}>
                    <DetailsList title="Details 1" loading={detailsLoading} itemsConfig={projectConfig} data={project} />
                </Grid>

                <Grid item xs={6}>
                    <DetailsList title="Details 2" loading={detailsLoading} itemsConfig={projectConfig2} data={project} />
                </Grid>
            </>
        )
    }

    const onRebuildClick = function () {
        setLoading(true);
        api.ProjectManage_RequestAutoupgrade(projectid).then(r => {
            setLoading(false);
            setRefresh(refresh + 1);
            if (r.success) {
                throw new Error('not implemented');
                // router.push(Urls.account.bgjobWaiting.buildProject(r.result));
            }
        });
    }

    return (
        <Layout title="Project Details">
            <PageLoading open={loading || detailsLoading} />
            <Grid container spacing={2}>
                {detailsListsMemo}
                <Grid item xs={6}>
                    <Paper>
                        <Typography variant="h6" align="center" padding={1}>Actions</Typography>
                        <div style={{ padding: '0 1rem' }}>
                            <Divider />
                        </div>
                        <List>
                            <ListItemButton onClick={onRebuildClick} disabled={project.status === ProjectStatus.Building}>
                                <ListItemIcon>
                                    <RefreshIcon />
                                </ListItemIcon>
                                <ListItemText primary="Autoupgrade"></ListItemText>
                            </ListItemButton>
                        </List>
                    </Paper>
                </Grid>
                <Grid item xs={12}>
                    <Accordion>
                        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                            <Typography>{lastBuildErrorText}</Typography>
                        </AccordionSummary>
                        <AccordionDetails>
                            <Typography variant="body2" component="pre" sx={{ whiteSpace: "pre-wrap" }}>
                                {project.lastDocfxBuildErrorLog}
                            </Typography>
                        </AccordionDetails>
                    </Accordion>
                </Grid>

                <Grid item xs={12}>
                    <SystemMessagesList filter={{projectId: projectid}} />
                </Grid>
            </Grid>
        </Layout>
    );
}