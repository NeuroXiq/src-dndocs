import { Accordion, AccordionDetails, AccordionSummary, Alert, Box, Divider, Grid, LinearProgress, Pagination, Paper, Skeleton, Stack, Typography } from "@mui/material";
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline';
import UseApi from "@/services/Api";
import { useEffect, useState } from "react";
import { TableDataRequest, TableDataRequestOrderDir } from "@/services/APIModel/Shared/TableDataRequest";
import { dateTimeUI } from "@/services/Helpers";
import ErrorIcon from '@mui/icons-material/Error';
import CheckCircleOutlineIcon from '@mui/icons-material/CheckCircleOutline';
import WarningIcon from '@mui/icons-material/Warning';
import GrainIcon from '@mui/icons-material/Grain';
import FiberManualRecordIcon from '@mui/icons-material/FiberManualRecord';
import { SystemMessageLevel } from "@/services/ApiDTO/Enum/SystemMessageLevel";

export default function SystemMessagesList(props: any) {
    const api = UseApi();
    const [loading, setLoading] = useState<boolean>(true);
    const [tableData, setTableData] = useState<any>({
        currentPage: 0,
        rowsCount: 0,
        rowsPerPage: 10,
        pagesCount: 0,
        result: []
    });

    const [tableDataRequest, setTableDataRequest] = useState<any>({
        pageNo: 0,
        rowsPerPage: 10,
        projectId: props?.filter?.projectId ?? '',
        projectVersioningId: props?.filter?.projectVersioningId ?? '',
    });

    useEffect(() => {
        setLoading(true);

        api.MyAccount_GetSystemMessages(tableDataRequest).then((r: any) =>
        {
            setTableData(r.result);
            setLoading(false);
        });

    }, [tableDataRequest]);

    function onPaginationChange(e: any, newValue: number) {
        if ((newValue - 1) === tableData.currentPage) {
            return;
        }

        setTableDataRequest({
            ...tableDataRequest,
            pageNo: newValue - 1
        });
    }

    const accordion = function (data: any) {
        let icon = null;
        let summaryColor = null;

        switch (data.level) {
            case SystemMessageLevel.Trace: 
                icon = <FiberManualRecordIcon color="disabled" />;
                break;
            case SystemMessageLevel.Error:
                icon = <ErrorIcon color="error"/>;
                break;
            case SystemMessageLevel.Information:
                icon = <ErrorOutlineIcon sx={{ color: 'info.light' }} />;
                summaryColor = 'rgb(237, 249, 254)'
                break;
            case SystemMessageLevel.Success:
                icon = <CheckCircleOutlineIcon color="success"/>;
                summaryColor = "";
                break;
            case SystemMessageLevel.Warning:
                icon = <WarningIcon color="warning"/>;
                break;
        }

        return (
            <Accordion key={data.id}>
                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                    <Stack direction="row" sx={{width: "100%"}} alignItems="center">
                        {icon}
                        <Typography sx={{ flexGrow: '1' }} variant="body2" marginLeft={2}>{data.title}</Typography>
                        <Typography marginRight={1} variant="body2">{dateTimeUI(data.dateTime)}</Typography>
                    </Stack> 
                </AccordionSummary>
                <AccordionDetails>
                    <Typography variant="body2" component="pre" style={{wordBreak: 'break-all', whiteSpace: 'pre-wrap'}}>{data.message}</Typography>
                </AccordionDetails>
            </Accordion>
        );
    }

    return (
        <Paper>
            {loading && <LinearProgress />}
            <Typography variant="h6" align="center" padding={1}>
                System Messages
            </Typography>
            <Divider />
            {!loading && tableData.data.map((m: any) => accordion(m))}
            {loading && <Skeleton sx={{m: 1}} variant="rectangular" height={128} />}
            <Divider />
            <Box paddingBottom={0.5} paddingTop={0.5} sx={{display: 'flex', justifyContent: 'flex-end'}}>
                {!loading && <Pagination onChange={onPaginationChange} count={tableData.pagesCount} page={tableData.currentPage + 1} sx={{marginTop: 1, marginBottom: 1}}/>}
            </Box>
        </Paper>
    );
}