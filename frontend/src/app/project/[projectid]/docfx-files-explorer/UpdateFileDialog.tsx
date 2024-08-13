import { Box, Button, Dialog, DialogActions, DialogContent, DialogTitle, Grid, TextField, TextareaAutosize } from "@mui/material";
import { useEffect, useState } from "react";
import { FullVPath } from "./DocfxHelpers";
import UseApi from "services/Api";

export default function UpdateFileDialog(props: any) {
    let filename = '';
    const api = UseApi();

    const [state, setState] = useState<any>({
        vpath: '',
        newContent: ''
    });

    useEffect(() => {
        const fd = props.data.fileData;
        const d = props.data;
        
        if (!props.open) {
            return;
        }

        const vpath = FullVPath(fd);

        api.ProjectManage_DocfxGetFileContent(d.projectId, vpath).thenResult((result: any) => {
            setState({
                ...state,
                vpath: vpath,
                newContent: result.fileContentResult.content,
                projectId: d.projectId
            });
        });


    }, [props.open]);

    function onSaveClick() {
        api.ProjectManage_DocfxUpdateFile(state).then((r: any) => {
            if (r.success) {
                props.onClose();
            }
        });
    }

    function onContentChange(e: any) {
        setState({
            ...state,
            newContent: e.target.value
        });
    }

    return (
        <Dialog fullScreen open={props.open}>
            <DialogTitle>Update File: {state.vpath}</DialogTitle>
            <DialogContent sx={{ display: 'flex' }}>
                <textarea
                onChange={onContentChange}
                value={state.newContent}
                style={{ resize: 'none', width: '100%' }}></textarea>
            </DialogContent>
            <DialogActions>
                <Button variant="outlined" onClick={props.onClose}>Cancel</Button>
                <Button variant="contained" onClick={onSaveClick}>Save</Button>
            </DialogActions>
        </Dialog>
    )
}