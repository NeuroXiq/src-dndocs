import { Box, Button, CircularProgress, Dialog, DialogActions, DialogContent, DialogTitle, Typography } from "@mui/material";
import DownloadIcon from '@mui/icons-material/Download';
import CloseIcon from '@mui/icons-material/Close';
import CancelIcon from '@mui/icons-material/Cancel';
import { useState } from "react";
import FileDownloadIcon from '@mui/icons-material/FileDownload';

export default function FileDownloadDialog(props: any) {
    const wait_confirm = 'wait_confirm', inprogress = 'inprogress', completed_ok = 'completed_ok';
    const state = props.state || wait_confirm;
    // console.log(props);

    // const [state, setState] = useState<any>();

    let title, content, actions;


    const onConfirmClick = function () {
        if (props.onCancelClick) {
            props.onConfirmClick();
        }
    }

    const onAbortClick = function () {
        if (props.onAbortClick) {
            props.onAbortClick();
        }
    }

    const onCancelClick = function () {
        if (props.onCancelClick) {
            props.onCancelClick();
        }
    }

    const closebutton = <Button onClick={onCancelClick} startIcon={<CloseIcon />}>Close</Button>;


    if (state === wait_confirm) {
        title = 'File Download';

        content = (
            <>
                <Typography margin={2}>Are Your sure You want to download following file: {props.fileName}</Typography>
            </>
        );

        actions = (
            <>
                {closebutton}
                <Button onClick={onConfirmClick} startIcon={<DownloadIcon />}>
                    Download
                </Button>

            </>
        );

    } else if (state === inprogress) {
        title = 'Download file';

        content = (
            <Box sx={{ p: 2, display: 'flex', flexDirection: 'column', alignItems: 'center', }}>
                <CircularProgress/>
                <Typography margin={1}>Downloading files please wait</Typography>
            </Box>
        );

        actions = (
            <Button startIcon={<CancelIcon />}>Abort</Button>
        );

    } else if (state === completed_ok) {
        title = 'Download success'

        content = (
            <Typography margin={2}>Download files completed successfully</Typography>
        );

        actions = (closebutton);

    } else {
        throw new Error('invalid "state"');
    }

    return (
        <Dialog open={props.show}>
            <DialogTitle sx={{ display: 'flex', alignItems: 'center' }} variant="h5" bgcolor="primary.main" color="primary.contrastText">
                <FileDownloadIcon sx={{ marginRight: 1 }} />
                {title}
            </DialogTitle>
            <DialogContent>{content}</DialogContent>
            <DialogActions>{actions}</DialogActions>
        </Dialog>
    );
}