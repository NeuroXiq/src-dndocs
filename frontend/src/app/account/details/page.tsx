'use client'
import AddIcon from '@mui/icons-material/Add';
import BasicTable from "@/components/BasicTable";
import DetailsList from "@/components/DetailsList";
import Layout from "@/components/Layout";
import Typoheader from "@/components/Typoheader";
import { Box, Button, Grid, Link, List, ListItemButton, ListItemIcon, ListItemText, Paper, Stack, Typography } from "@mui/material";
import Urls from "config/Urls";
import useApiCallEffect from "hooks/useApiCall";
import { useEffect, useMemo, useState } from "react";
import useApi from "services/Api";
import { useQueryEffect } from "services/Api";
import MuiLink from '@mui/material/Link';
import { EID, UIName } from '@/services/ApiDTO/Enum/Enums';
import ProjectDto from '@/services/ApiDTO/Project/ProjectDto';
import SellIcon from '@mui/icons-material/Sell';
import ProjectVersioningDto from '@/services/ApiDTO/Project/ProjectVersioningDto';
import SimpleTable, { TableCfg } from '@/components/SimpleTable';
import ProjectVersionDto from '@/services/ApiDTO/Project/ProjectVersionDto';
import { idText } from 'typescript';
import SystemMessagesList from '@/components/SystemMessagesList';

export default function Details() {
    const listConfig = [
        { title: 'Id', key: 'id' },
        { title: 'Login', key: 'login' },
        { title: 'Email', key: 'primaryEmail' },
    ];

    const api = useApi();

    const { loading: ldAccount, result: resultAccount } = useQueryEffect(() => api.MyAccountGetAccountDetails());
    const { loading: ldProjs, result: rProjs } = useQueryEffect<ProjectDto[]>(() => api.getAllUserProjects());
    const { loading: ldVers, result: rVers } = useQueryEffect<ProjectVersioningDto[]>(() => api.Project_GetAllProjectVersioning());
    let resultProjects: any = [];

    if (!ldProjs) {
        resultProjects = rProjs?.map((r: ProjectDto) => {
            return {
                ...r,
                status: UIName(EID.ProjectStatus, r.status),
                projectDetails: <Link href={Urls.project.details(r.id)}>Project Details</Link>,
                apiexplorer: <Link href={Urls.robiniadocsUrl(r.urlPrefix)}>DNDocs</Link>,
                deleteProject: <MuiLink color="error" href={Urls.project.delete(r.id)}><Button color="error">DELETE</Button></MuiLink>
            }
        });
    }

    resultProjects = resultProjects.sort((a: any, b: any) => b.id - a.id);

    function VerTable() {
        const tblCfgVers: TableCfg<ProjectVersioningDto> = {
            data: rVers || [],
            cols: [
                { id: 'id', title: 'Id', val: (t) => t.id },
                { id: 'i2', title: 'Project Name', val: (t) => t.projectName },
                { id: 'i3', title: 'Git URL', val: (t) => t.gitDocsRepoUrl },
                { id: 'i4', title: 'Details', val: (t) => <a href={Urls.project.versioningDetails(t.id)}>Details</a> },
                { id: 'i5', title: 'Create New Version', val: (t) => <a href={Urls.project.createProjectVersion(t.id)}>Manual Create Version</a> },
            ]
        }

        return (<SimpleTable config={tblCfgVers} loading={ldVers}></SimpleTable>)
    }

    const tableConfig = {
        idKey: 'id',
        data: resultProjects || [],
        cols: [{
            title: 'Id',
            key: 'id'
        }, {
            title: 'Project Name',
            key: 'projectName'
        }, {
            title: 'Url Prefix',
            key: 'urlPrefix'
        }, {
            title: 'Status',
            key: 'status'
        }, {
            title: 'Details',
            key: 'projectDetails'
        }, {
            title: 'API Explorer',
            key: 'apiexplorer'
        }, {
            title: 'Delete',
            key: 'deleteProject'
        }],
    };

    return (
        <Layout title="Account Details">
            <Grid container spacing={2}>
                <Grid item sm={12} xs={12}>
                    {<DetailsList loading={ldAccount} itemsConfig={listConfig} data={resultAccount}></DetailsList>}
                </Grid>
                <Grid item sm={12} xs={12}>
                    <Paper>
                        <Typography variant="h6" textAlign="center">Versioning</Typography>
                        <Stack alignItems="flex-end" padding={1}>
                            <Button variant="outlined" startIcon={<SellIcon />} href={Urls.project.createVersioning}>Create New Versioning</Button>
                        </Stack>
                        {VerTable()}
                    </Paper>
                </Grid>
                <Grid item sm={12} xs={12}>
                    <Paper sx={{ mt: 2, p: 2 }}>
                        <Typography variant="h6" textAlign="center">All Projects</Typography>
                        <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
                            <Link href={Urls.project.createProject}>
                                <Button startIcon={<AddIcon />} variant="outlined" sx={{ m: '1rem 0' }}>
                                    Create New Project
                                </Button>
                            </Link>
                        </Box>
                        <BasicTable config={tableConfig} loading={ldProjs}>
                        </BasicTable>
                    </Paper>
                </Grid>
                <Grid item sm={12} xs={12}>
                    <SystemMessagesList />
                </Grid>
                <Grid item sm={12} xs={12}></Grid>
            </Grid>
        </Layout>
    )
}