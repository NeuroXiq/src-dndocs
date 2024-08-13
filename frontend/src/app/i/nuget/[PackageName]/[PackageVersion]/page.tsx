'use client'
import Layout from "@/components/Layout";
import Typoheader from "@/components/Typoheader";
import { Grid, Paper, Box, Typography, Divider, Stack, ListItemButton, Button, Link } from "@mui/material";
import * as React from 'react';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemText from '@mui/material/ListItemText';
import ListItemAvatar from '@mui/material/ListItemAvatar';
import Avatar from '@mui/material/Avatar';
import FmdBadIcon from '@mui/icons-material/FmdBad';
import { Check } from "@mui/icons-material";
import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import UseApi from "@/services/Api";
import LinearProgress from '@mui/material/LinearProgress';
import HourglassEmptyIcon from '@mui/icons-material/HourglassEmpty';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';

import SourceIcon from '@mui/icons-material/Source';
import WorkHistoryIcon from '@mui/icons-material/WorkHistory';
import PendingIcon from '@mui/icons-material/Pending';
import PauseCircleOutlineIcon from '@mui/icons-material/PauseCircleOutline';
import AutoModeIcon from '@mui/icons-material/AutoMode';
import Alert from '@mui/material/Alert';
import LinkIcon from '@mui/icons-material/Link';
import { BgJobStatus } from "@/services/ApiDTO/Enum/BgJobStatus";
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline';
import LaunchIcon from '@mui/icons-material/Launch';
import BgJobViewModel from "@/services/ApiDTO/Other/BgJobViewModel";

export default function Page() {
    const urlParams = useParams() || {};
    const urlPackageName = urlParams.PackageName;
    const urlPackageVersion = urlParams.PackageVersion;

    const api = UseApi();
    const data = useState<any>(null);
    const [refresh, setRefresh] = useState<number>(0);
    const [jobStatus, setJobStatus] = useState<BgJobViewModel | null>(null); // todo fix any
    const [createResult, setCreateResult] = useState<any>(null);


    function requestCreateProject() {
        api.Integration_NuGetCreateProject({
            packageName: urlPackageName,
            packageVersion: urlPackageVersion,
        })
        .then((r) => {
            setCreateResult(r);
            setJobStatus(null);

            if (r.success) {
                setRefresh((x) => x + 1);
            }
         });
    }

    useEffect(() => {
        let abort = false;

        if (jobStatus != null && jobStatus.state !== 2 && jobStatus.state !== 3) { return; }

        api.Integration_NugetCreateProjectCheckStatus(urlPackageName as string, urlPackageVersion as string)
            .then((r: any) => {
                if (abort) { return; }
                // console.log(result);
                setJobStatus(r.result);

                if (r.result == null) {
                    console.log('requestprojectj');
                    requestCreateProject();
                } else {
                    setTimeout(() => { setRefresh((x) => x + 1); }, 5000);
                }
            });

        return () => { abort = true; };
    }, [urlParams.PackageName, urlParams.PackageVersion, refresh]);

    let jobRenderInfo = { stepNo: -1, status: '' };

    if (createResult?.success === false) {
        jobRenderInfo = {stepNo: 1, status: 'error' }
    }
    else if (jobStatus == null) {
        jobRenderInfo = { stepNo: 1, status: 'loading' }
    } else if (jobStatus.stateDetails === 2) {
        //waiting to process
        jobRenderInfo = { stepNo: 2, status: 'loading' }
    } else if (jobStatus.stateDetails === 3) {
        // job running in progress
        jobRenderInfo = { stepNo: 3, status: 'loading' }
    }
    else {
        //completed
        if (jobStatus.state === 1) {
            jobRenderInfo = { stepNo: 4, status: 'success' }
            setTimeout(() => window.location.href = jobStatus.projectApiFolderUrl, 2000);
        } else {
            jobRenderInfo = { stepNo: 4, status: 'error' }
        }
    }

    const BuildStep = function (title: string, description: string, stepNo: number) {
        let mode = null;
        let color = "success";//text.primary";
        let progressColor = "primary";

        if (jobRenderInfo.stepNo > stepNo) {
            mode = 'success';
        } else if (jobRenderInfo.stepNo < stepNo) {
            mode = 'disabled';
        } else {
            mode = jobRenderInfo.status;
        }

        let icon = null;
        switch (mode) {
            case 'loading':
                icon = <HourglassEmptyIcon />
                break;
            case 'disabled':
                icon = <Check />
                color = 'text.disabled';
                break;
            case 'success':
                icon = <CheckCircleIcon color="success" />
                progressColor = "success"
                break;
            case 'error':
                progressColor = "error";
                icon = <FmdBadIcon color="error" />
                break;
            default: break;
        }

        return (
            <Box marginTop={1}>
                <Paper elevation={mode === 'disabled' ? 1 : 2}>
                    {mode === 'loading' && <LinearProgress color="primary" />}
                    {mode === 'success' && <LinearProgress color="success" variant="determinate" value={100} />}
                    {mode === 'error' && <LinearProgress color="error" variant="determinate" value={100} />}
                    <Box padding={1} color={color}>
                        <Box marginTop={1} marginBottom={1} display="flex" alignItems="center">
                            {icon}
                            <Typography marginTop={0} variant="h6" marginLeft={1}>{title}</Typography>
                        </Box>
                        <Divider />
                        <Typography margin={1} variant="subtitle1">{description}</Typography>
                    </Box>
                </Paper>
            </Box>
        );
    }

    return (
        <Layout title="NuGet API Explorer">
            <Grid container rowSpacing={1} columnSpacing={1}>
                <Grid item xs={8} alignItems="stretch">
                    <Stack
                        display="flex"
                        flexDirection="column"
                        alignItems="stretch">
                        <Typoheader margin="0">Generating a project</Typoheader>
                        {BuildStep('Requesting a new job', 'Requesting to start generate project', 1)}
                        {BuildStep('Waiting to start', 'Waiting to start a job', 2)}
                        {BuildStep('Waiting to complete a job', 'Waiting job to complete', 3)}
                        {BuildStep('Job Execution completed', 'Job completed', 4)}
                    </Stack>
                </Grid>
                <Grid item xs={4}>
                    <Paper style={{ height: "100%" }}>
                        <Box
                            padding={1}
                            display="flex"
                            flexDirection="column"
                        >
                            <Box
                                display="flex"
                                justifyContent="center"
                                margin={"1rem 0"}>
                                <img src="/nuget-lockup-dark-fill.svg" /></Box>
                            <Divider />
                            <List>
                                <ListItem>
                                    <ListItemAvatar>
                                        <Avatar>
                                            <SourceIcon />
                                        </Avatar>
                                    </ListItemAvatar>
                                    <ListItemText primary="NuGet Package" secondary={`${urlPackageName} ${urlPackageVersion}`} />
                                </ListItem>
                                <ListItem>
                                    <ListItemAvatar>
                                        <Avatar>
                                            <WorkHistoryIcon />
                                        </Avatar>
                                    </ListItemAvatar>
                                    <ListItemText primary="Project Identifier" secondary={jobStatus?.projectId} />
                                </ListItem>
                                <ListItem>
                                    <ListItemAvatar>
                                        <Avatar>
                                            <PendingIcon />
                                        </Avatar>
                                    </ListItemAvatar>
                                    <ListItemText primary="Jobs before count" secondary={jobStatus?.estimateOtherJobsBeforeThis ?? '-'} />
                                </ListItem>
                                <ListItem>
                                    <ListItemAvatar>
                                        <Avatar>
                                            <PauseCircleOutlineIcon />
                                        </Avatar>
                                    </ListItemAvatar>
                                    <ListItemText primary="Estimated to start" secondary={( jobStatus == null ? '-' : (jobStatus?.estimateStartIn + "s"))} />
                                </ListItem>
                                <ListItem>
                                    <ListItemAvatar>
                                        <Avatar>
                                            <AutoModeIcon />
                                        </Avatar>
                                    </ListItemAvatar>
                                    <ListItemText primary="Estimated build time" secondary={( jobStatus == null ?  "-" : (jobStatus?.estimateBuildTime + 's'))} />
                                </ListItem>
                            </List>
                            {(jobStatus?.state === 1) &&

                            // <Link component={<Button />} variant="body2" color="success">asdf</Link>
                            <Button
                                startIcon={<LaunchIcon />}
                                href={jobStatus?.projectApiFolderUrl}
                                size="large" 
                                sx={{flexGrow:"1"}}
                                variant="contained" color="success">Open API Explorer</Button>
                                // <Alert icon={<LinkIcon />} severity="success">
                                //     <Link href={jobStatus?.projectApiFolderUrl} variant="body1">DNDocs URL</Link>
                                //     {/* <Typography></Typography> */}
                                // </Alert>
                            }
                            {(jobStatus?.stateDetails === 4 || createResult?.success === false) &&
                                <>
                                    <Button variant="contained" color="error" onClick={() => requestCreateProject()}>Force Run Again</Button>
                                </>
                            }
                        </Box>
                    </Paper>
                </Grid>
            </Grid>
            {(createResult?.success === false) && <Alert sx={{marginTop: 1, fontSize: '1rem'}} severity="error"> 
                Error: {createResult?.errorMessage}
                {createResult?.fieldErrors && <pre>{JSON.stringify(createResult?.fieldErrors, null, 2)}</pre>}
            </Alert>
            }
            {createResult?.success === false  && <Alert sx={{marginTop: 1, fontSize: '1rem'}} severity="error"> 
                Error: {createResult.errorMEssage}
            </Alert>
            }
        </Layout>
    );
}