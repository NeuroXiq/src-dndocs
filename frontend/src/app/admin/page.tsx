'use client'
import DetailsList from "@/components/DetailsList";
import Layout from "@/components/Layout";
import { Button, Grid, List, ListItemButton, ListItemIcon, ListItemText, Paper, Typography } from "@mui/material";
import RotateRightIcon from '@mui/icons-material/RotateRight';
import PlayCircleOutlineIcon from '@mui/icons-material/PlayCircleOutline';
import useApiCallEffect, { apiCallNoEffect } from "hooks/useApiCall";
import Api from "services/Api";
import UseApi from "services/Api";
import { useQueryEffect } from "services/Api";
import BasicTable from "@/components/BasicTable";
import Link from "next/link";
import MUILink from "@mui/material/Link";
import ApiUrls from "config/ApiUrls";
import { useCallback, useContext, useEffect, useMemo, useState } from "react";
import FileDownloadDialog from "@/components/FileDownloadDialog";
import Urls from "config/Urls";
import AddLinkIcon from '@mui/icons-material/AddLink';
import { GlobalAppContext, IGlobalAppContextValue } from "hooks/globalAppContext";
import { EID, TableDataRequestOrderDir, UIName } from "@/services/ApiDTO/Enum/Enums";
import ArchiveIcon from '@mui/icons-material/Archive';
import MapIcon from '@mui/icons-material/Map';
import SimpleTable, { TableCfg, emptyTable } from "@/components/SimpleTable";
import { dateTimeAgoUI, nof } from "@/services/Helpers";
import PageLoading from "@/components/PageLoading";
import useGlobalAppContext from "hooks/useGlobalAppContext";
import MuiLink from '@mui/material/Link';
import { useRouter } from "next/navigation";
import TableDataDto from "@/services/ApiDTO/Shared/TableDataDto";
import ProjectDto from "@/services/ApiDTO/Project/ProjectDto";

export default function AdminDashboard() {
    const gc = useContext<IGlobalAppContextValue>(GlobalAppContext);
    const { loading: ldbi, result: dbi } = useQueryEffect<any>(() => api.Admin_GetDashboardInfo());
    const [globalLoading, setGlobalLoading] = useState<boolean>(false);
    const gac = useGlobalAppContext();
    const api = UseApi();
    const router = useRouter();

    const [tableRequest, setTableRequest] = useState<any>({
        rowsPerPage: 10,
        page: 0,
    });

    const { loading: projectsLoading, result: projectsTable } = useQueryEffect<TableDataDto<ProjectDto>>(() => api.Admin_GetTableData({
        tableName: 'project',
        filters: [],
        orderBy: [{ column: 'id', dir: TableDataRequestOrderDir.Desc }],
        getRowsCount: true,
        page: tableRequest.page,
        rowsPerPage: tableRequest.rowsPerPage
    }), [tableRequest]);

    const tOnPageChange = useCallback(memoOnPageChange, [projectsTable, setTableRequest]);
    const tOnRowsPerPageChange = useCallback(memoOnRowsPerPageChange, [projectsTable, setTableRequest]);
    const tOnMenuItemClick = useCallback(memoOnMenuItemClick, [projectsTable, setTableRequest, api, setGlobalLoading, gac]);
    const tableComponent = useMemo(tableComponentMemo, [projectsTable, tOnMenuItemClick]);

    function memoOnMenuItemClick(e: any) {
        let promise : any = null;

        if (e.id === 'delete') {
            setGlobalLoading(true);
            router.push(Urls.project.delete(e.rowData.id));

        } else if (e.id === 'recreate') {
            promise = api.Admin_HardRecreateProject(e.rowData.id);
        } else throw new Error('not implemented');

        if (promise) {
            setGlobalLoading(true);
            promise.then((r: any) => {
                setGlobalLoading(false);
                gac.showSuccessMessage('Action success');
            });
        }
    }

    function memoOnRowsPerPageChange(e: any) {
        setTableRequest({ ...tableRequest, rowsPerPage: e.newValue, page: 0 });
    }

    function memoOnPageChange(e: any) {
        setTableRequest({ ...tableRequest, page: e.newValue });
    }

    const bgworkerConfig = [
        { title: 'Is Normal Running', key: 'backgroundDoWorkIsRunning' },
        { title: 'Is Important Running', key: 'backgroundDoImportantWorkIsRunning' },
        { title: 'Waiting to save App Logs Count', key: 'backgroundQueueHttpLogsCount' },
        { title: 'Waiting to save HTTP Logs Count', key: 'backgroundQueueAppLogsCount' },
        { title: 'Status Text', key: 'backgroundStatusText' },
    ]

    const statsListConfig = [
        { title: 'Problems Count', key: 'problemsCount' },
        { title: 'Unique Visitors 24H', key: 'uniqueVisitors24H' },
        { title: 'Unique Visitors 7 Days', key: 'uniqueVisitors7Days' },
        { title: 'In Memory Queued Logs To Save', key: 'inMemoryQueuedLogsToSave' },
        { title: 'App Logs Count', key: 'appLogsCount' },
        { title: 'HttpLogsCount', key: 'httpLogsCount' },
    ];

    const [downloadDialog, setDownloadDialog] = useState<any>({ state: 'wait_confirm', show: false, fileName: '', projectId: -1 });

    const getProjectFiles = function (projectid: any) {
        setDownloadDialog({ state: 'wait_confirm', show: true, fileName: 'Project ' + projectid, projectId: projectid })
    }

    if (!ldbi) {

    }

    const bgWorkAction = function (action: string) {
        var bgWorkForm = {
            ForceAll: false,
            ForceQueuedItems: false,
            ForceCheckHttpStatusForProjects: action === 'refreshHttpStatus',
            ForceAutoupgradeProjects: action === 'ForceAutoupgradeProjects',
            ForceAllProjectsRebuildDocfx: action === 'ForceAllProjectsRebuildDocfx',
            ForceGenerateSitemap: action === 'ForceGenerateSitemap',
        };
        api.Admin_DoBackgroundWorkNow(bgWorkForm).then(r => {
            gc.showSuccessMessage('handled');
        });

        if (action === 'runBgWorker') {
            throw new Error('not impleented');
        }
    }

    function onAttachTenantClick() {
    }

    const onConfirmDownloadClick = function () {
        setDownloadDialog({
            ...downloadDialog,
            state: 'inprogress'
        });

        api.Admin_GetProjectFiles(downloadDialog.projectId)
            .then(fileUrl => {
                window.open(fileUrl, '_blank');
                setDownloadDialog({ ...downloadDialog, state: 'completed_ok' });
            });
    }

    const onAbortDownloadClick = function () {

    }

    const onCancelDownloadClick = function () { setDownloadDialog({ ...downloadDialog, show: false }) }

    function tableComponentMemo() {
        let table = projectsTable || emptyTable();

        let data = table.data.map<ProjectDto>(r => {
            return {
                ...r,
                bgHealthCheckHttpGetStatus: UIName(EID.BgProjectHealthCheckStatus, r.bgHealthCheckHttpGetStatus),
                status: UIName(EID.ProjectStatus, r.status),
                robiniadocsUrl: <MUILink href={Urls.robiniadocsUrl(r.urlPrefix)}>{r.urlPrefix}</MUILink>,
                downloadTenantFiles: <Button onClick={() => getProjectFiles(r.id)}>Download</Button>,
                lastDocfxBuildTime: dateTimeAgoUI(r.lastDocfxBuildTime),
                bgHealthCheckHttpGetDateTime: dateTimeAgoUI(r.bgHealthCheckHttpGetDateTime),
            }
        });

        const config : TableCfg<ProjectDto> = {
            data: data,
            cols: [
                { id: 'id', title: 'Id', dp: 'id' },
                { id: 'uuid', title: 'UUID', val: p => p.uuid, copyOnClick: true },
                { id: 'name', title: 'Name', val: p => p.projectName },
                { id: 'status', title: 'Status', dp: 'status' },
                { id: 'rurl', title: 'RobiniaDocs', val: p => p.urlPrefix },
                { id: 'dtf', title: 'Download Tenant Files', dp: 'downloadTenantFiles' },
                { id: 'hs', title: 'Health status', dp: 'bgHealthCheckHttpGetStatus' },
                { id: 'ht', title: 'Health Check Date', dp: 'bgHealthCheckHttpGetDateTime' },
                { id: 'lbt', title: 'Build Time', dp: 'lastDocfxBuildTime'}
                // { id: 'gurl', title: 'Github', key: 'githubUrl' },
            ],
            pagination: {
                count: table.rowsCount,
                rowsPerPage: table.rowsPerPage,
                page: table.currentPage
            },
            events: {
                onPageChange: tOnPageChange,
                onRowsPerPageChange: tOnRowsPerPageChange,
                onMenuItemClick: tOnMenuItemClick
            },
            contextMenu: {
                items: [{ id: 'delete', title: 'Delete' },
                { id: 'recreate', title: 'Hard Recreate' }]
            }
        };

        return <SimpleTable config={config} loading={projectsLoading}></SimpleTable>;
    }

    return (
        <Layout title="Dashboard">
            <PageLoading open={globalLoading} />
            <FileDownloadDialog
                show={downloadDialog.show}
                fileName={downloadDialog.fileName}
                onConfirmClick={onConfirmDownloadClick}
                onCancelClick={onCancelDownloadClick}
                state={downloadDialog.state} />
            <Grid container spacing={2}>
                <Grid item md={4}>
                    <DetailsList loading={ldbi} data={dbi} itemsConfig={statsListConfig}></DetailsList>
                </Grid>
                <Grid item md={4}>
                    <DetailsList loading={ldbi} data={dbi} itemsConfig={bgworkerConfig}></DetailsList>
                </Grid>
                <Grid item md={4}>
                    <Paper>
                        <Typography textAlign="center" variant="h5">Actions</Typography>
                        <List>
                            <ListItemButton onClick={() => bgWorkAction('ForceAllProjectsRebuildDocfx')}>
                                <ListItemIcon>
                                    <RotateRightIcon />
                                </ListItemIcon>
                                <ListItemText primary="Rebuild all projects"></ListItemText>
                            </ListItemButton>
                            <ListItemButton onClick={() => bgWorkAction('refreshHttpStatus')}>
                                <ListItemIcon>
                                    <RotateRightIcon />
                                </ListItemIcon>
                                <ListItemText primary="Refresh HTTP status"></ListItemText>
                            </ListItemButton>
                            <ListItemButton onClick={() => bgWorkAction('runBgWorker')}>
                                <ListItemIcon>
                                    <PlayCircleOutlineIcon />
                                </ListItemIcon>
                                <ListItemText primary="Run background worker"></ListItemText>
                            </ListItemButton>
                            <ListItemButton onClick={() => bgWorkAction('ForceAutoupgradeProjects')}>
                                <ListItemIcon><ArchiveIcon /></ListItemIcon>
                                <ListItemText primary="ForceAutoupgradeProjects"></ListItemText>
                            </ListItemButton>
                            <ListItemButton onClick={onAttachTenantClick}>
                                <ListItemIcon>
                                    <AddLinkIcon />
                                </ListItemIcon>
                                <ListItemText primary="Attach Tenant"></ListItemText>
                            </ListItemButton>
                            <ListItemButton onClick={() => bgWorkAction('ForceGenerateSitemap')}>
                                <ListItemIcon><MapIcon /></ListItemIcon>
                                <ListItemText primary="Force Generate Sitemap"></ListItemText>
                            </ListItemButton>
                        </List>
                    </Paper>
                </Grid>
                <Grid md={12} item>
                    <Paper>
                        {tableComponent}
                        {/* <BasicTable config={projectsTableConfig}></BasicTable> */}
                    </Paper>
                </Grid>
            </Grid>
        </Layout>
    );
}