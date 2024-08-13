'use client';
import Layout from "@/components/Layout";
import PageLoading from "@/components/PageLoading";
import UseApi from "@/services/Api";
import { Alert, Box, Button, Card, CardContent, CardHeader, CircularProgress, Stack, Step, StepContent, StepLabel, Stepper, TextField, Typography } from "@mui/material";
import { useSearchParams } from "next/navigation";
import { useEffect, useMemo, useRef, useState } from "react";
import ErrorIcon from '@mui/icons-material/Error';
import CheckCircleOutlinedIcon from '@mui/icons-material/CheckCircleOutlined';
import HourglassEmptyIcon from '@mui/icons-material/HourglassEmpty';
import Typoheader from "@/components/Typoheader";
import React from "react";
import Urls from "config/Urls";
import LinkIcon from '@mui/icons-material/Link';
import { BgJobStatus } from "@/services/ApiDTO/Enum/BgJobStatus";


export default function Page() {
    const api = UseApi();
    const searchParams: any = (useSearchParams());
    const projectId = +searchParams.get('projectid');
    const [refresh, setRefresh] = useState<number>(0);
    const [result, setResult] = useState<any>(null);

    useEffect(() => {
        let abort = false;

        api.MyAccount_GetBgJob(projectId).then((r: any) => {
            if (abort) { return; }

            setResult(r);

            if (!r || r.result?.bgJobStatus === 1 || r.result?.bgJobStatus === 2) {
                setTimeout(() => setRefresh((x) => x + 1), 3000);
            }
        });

        return () => { abort = true }
    }, [refresh, setRefresh]);

    JSON.stringify(result, null, 2);

    return (
        <>
            <pre style={{ fontSize: '12px' }}>
                TODO: This page need better UI than raw JSON

                Refresh counter: {refresh}
                {!!result ? JSON.stringify(result, null, 2) : '<no-result>'}
                
            </pre>
            {result?.result?.commandHandlerSuccess === true &&
                    <Alert severity="success">
                        Job Completed successfully. API Explorer URL: <a href={result?.result?.projectApiFolderUrl}>{result?.result?.projectApiFolderUrl}</a>
                    </Alert>
                }
        </>
    );
}

// export default function Page() {
//     const api = UseApi();
//     const searchParams: any = (useSearchParams());
//     const [refreshTick, setRefreshTick] = useState<number>(0);
//     const [statusResponse, setStatusResponse] = useState<any>(null);
//     const [statusResponseSuccess, setStatusResponseSuccess] = useState<any>(null);
//     const type = +searchParams.get('type');

//     const requestForm: any = {
//         bgjobid: +searchParams.get('bgjobid'),
//         type: type
//     };

//     useEffect(() => {
//         let ignore = false;

//         api.MyAccount_WaitingForBgJob(requestForm).then(r => {
//             if (ignore) { return; }
//             setStatusResponse(r.result);
//             setStatusResponseSuccess(r.success);
//         });

//         return () => { ignore = true };
//     }, [refreshTick]);

//     function refreshStatus() {
//         if (statusResponse?.bgJobStatus === BgJobStatus.Completed ||
//             statusResponse?.bgJobStatus === BgJobStatus.Failed) {
//             return;
//         }

//         setRefreshTick(refreshTick + 1);
//     }

//     useEffect(() => {
//         var interval = setInterval(refreshStatus, 2000);

//         return () => clearInterval(interval);
//     }, [refreshStatus]);

//     const messagesComponent = useMemo(memMessagesComponent, [statusResponse?.messages?.length]);

//     if (!statusResponse) {
//         return (<h1>loading</h1>);
//     }

//     let statusStep1, statusStep2, statusStep3;
//     function getStep(c: any) {
//         let avatar = null;
//         let textColor = '';

//         switch (c.status) {
//             case 'todo':
//                 textColor = 'text.disabled';
//                 avatar = <HourglassEmptyIcon fontSize="large" color="disabled" />;
//                 break;
//             case 'inprogress':
//                 textColor = 'text';
//                 avatar = <CircularProgress size="2rem" />
//                 break;
//             case 'done_ok':
//                 textColor = 'success';
//                 avatar = <CheckCircleOutlinedIcon color="success" fontSize="large" />;
//                 break;
//             case 'fail':
//                 textColor = 'error';
//                 avatar = <ErrorIcon fontSize="large" color="error" />;
//                 break;
//         }

//         return (
//             <Card key={c.title} variant="outlined" sx={{ flex: '1 0 1px' }}>
//                 <CardHeader title={c.title} titleTypographyProps={{ variant: "h6", color: textColor }} avatar={avatar}>
//                 </CardHeader>
//                 <CardContent><Typography variant="body1">{c.description}</Typography></CardContent>
//             </Card>
//         );
//     }

//     switch (statusResponse.bgJobStatus) {
//         case BgJobStatus.WaitingToProcess:
//             statusStep1 = 'inprogress';
//             statusStep2 = 'todo';
//             statusStep3 = 'todo';
//             break;
//         case BgJobStatus.InProgress:
//             statusStep1 = 'done_ok';
//             statusStep2 = 'inprogress';
//             statusStep3 = 'todo';
//             break;
//         case BgJobStatus.Completed:
//             statusStep1 = 'done_ok';
//             statusStep2 = 'done_ok';
//             statusStep3 = 'done_ok';
//             break;
//         case BgJobStatus.Failed:
//             statusStep1 = 'done_ok';
//             statusStep2 = 'done_ok';
//             statusStep3 = 'fail';
//             break;
//     }

//     const steps = [{
//         title: 'Waiting to run',
//         description: 'Waiting to start a job',
//         status: statusStep1
//     }, {
//         title: 'Running',
//         description: 'Job is running',
//         status: statusStep2
//     }, {
//         title: 'Completed',
//         description: 'Job completed execution',
//         status: statusStep3
//     }];

//     if (!statusResponseSuccess) {
//         return (
//             <Typoheader variant="h5" mode="error">Failed to request Job status</Typoheader>
//         );
//     }

//     let title = "Work in progress, please wait...";

//     if (statusResponse.commandHandlerSuccess) {
//         title = "Job completed successfully";
//     }

//     function subheader() {
//         let title = '';
//         let mode = 'primary';
//         let openProjPage = null;

//         if (statusResponse.commandHandlerSuccess) {
//             title = 'Success.';

//             if (type === 1 || type === 2) {
//                 let projUrl = Urls.robiniadocsUrl(statusResponse.createdProject.urlPrefix);
//                 openProjPage = <Button
//                     sx={{margin: "0.5rem 0"}}
//                     href={projUrl}
//                     target="_blank"
//                     variant="outlined"
//                     fullWidth
//                     size="large"
//                     startIcon={<LinkIcon fontSize="large" />}>Click Here to open Project page</Button>
//             }
//         } else if (statusResponse.bgJobStatus === BgJobStatus.Failed) {
//             title = 'Failed to run Job (System Internal Error).';
//             mode = 'error';
//         } else if (statusResponse.bgJobStatus === BgJobStatus.Completed && statusResponse.commandHandlerSuccess === false) {
//             title = 'Job failed, system processed job but error(s) occured.'
//             mode = 'error';
//         } else {
//             return null;
//         }

//         title += ' You can go back to Your account page';

//         let subtitle = (<Typoheader variant="h6" mode={mode}>{title}</Typoheader>);

//         return (
//             <>
//             {subtitle}
//             {openProjPage}
//             </>
//         )
//     }

//     function memMessagesComponent() {
//         if (!statusResponse) {
//             return null;
//         }

//         return (
//             <Box sx={{ maxHeight: "30rem", overflowY: 'auto', borderLeft: '0.25rem solid lightgray', padding: 1 }}>
//                 <Typography component="pre" variant="body2" color="text.secondary" fontFamily="Consolas">
//                     {statusResponse.messages.reverse().map((r: any) => `${r}\r\n- - - - - - - -\r\n`)}
//                 </Typography>
//             </Box>
//         );
//     }

//     return (
//         <Layout title={title}>
//             {subheader()}
//             <Stack direction="row" gap={1}>
//                 {steps.map((s: any) => getStep(s))}
//             </Stack>
//             <Typography marginTop={1} variant="h5">System messages:</Typography>
//             {messagesComponent}
//         </Layout>);
// }