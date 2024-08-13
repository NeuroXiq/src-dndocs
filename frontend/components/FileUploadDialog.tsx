import { AppBar, Box, Button, Dialog, DialogActions, DialogContent, DialogTitle, Divider, FormControlLabel, Icon, IconButton, InputLabel, Radio, RadioGroup, Toolbar, Typography } from "@mui/material";
import { useEffect, useRef, useState } from "react";
import BasicTable from "./BasicTable";
import CloseIcon from '@mui/icons-material/Close';
import CheckCircleOutlineIcon from '@mui/icons-material/CheckCircleOutline';
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline';
import HourglassEmptyIcon from '@mui/icons-material/HourglassEmpty';
import FileUploadIcon from '@mui/icons-material/FileUpload';

export default function FileUploadDialog(props: any) {
    const config = props.config;
    const [state, setState] = useState<any>({
        fileKind: FileKind.File,
    });

    let tableData = config.files || [];

    function tableStatusCellValue(d: any) {
        let icon = null;
        let color = "black";

        if (d.status === FileUploadState.UploadSuccess) {
            icon = (<CheckCircleOutlineIcon color="primary"></CheckCircleOutlineIcon>)
            color = "primary"
        } else if (d.status === FileUploadState.UploadFail) {
            icon = (<ErrorOutlineIcon color="error"></ErrorOutlineIcon>);
            color = "error";
        } else if (d.status === FileUploadState.ReadyToUpload) {
            icon = (<HourglassEmptyIcon></HourglassEmptyIcon>)
        } else {
            throw new Error(`unknown status: "${d.status}"`)
        }

        return (
            <Box sx={{ display: 'flex', alignItems: 'center' }}>
                {icon}
                {<Typography paddingLeft={1} variant="body2" color={color}>{d.statusMsg}</Typography>}
            </Box>
        );
    }

    if (tableData.length > 0) {
        tableData = tableData.map((d: any) => {
            return {
                id: d.name + d.size + d.status,
                name: d.name,
                size: d.size,
                status: tableStatusCellValue(d)
            };
        });
    }

    const tableConfig = {
        cols: [
            { title: 'File Name', key: 'name' },
            { title: 'Size', key: 'size' },
            { title: 'Upload status', key: 'status' }
        ],
        idKey: 'id',
        data: tableData
    };

    const filesInputRef = useRef<any>(null);
    const dirsInputRef = useRef<any>(null);

    useEffect(() => {
        if (!dirsInputRef.current) {
            return;
        }
        
        // avoid typescript compilation errors
        dirsInputRef.current.setAttribute("directory", "");
        dirsInputRef.current.setAttribute("webkitdirectory", "");
    }, [dirsInputRef]);

    function onFileTypeChange(e: any) {
        setState({ ...state, fileKind: e.target.value })
    }

    function onSelectFilesClick() {
        if (state.fileKind === FileKind.File) {
            filesInputRef?.current?.click();
        } else {
            dirsInputRef?.current?.click();
        }
    }

    function onFilesChanged(newfiles: any) {
        const fileList = newfiles;
        const fileKind = state.fileKind;
        const result = [];

        for (let i = 0; i < fileList.length; i++) {
            const f = fileList[i];
            const s = Math.round((f.size / 1000) * 100) / 100;
            result.push({
                id: Math.random(),
                name: fileKind === FileKind.File ? f.name : f.webkitRelativePath,
                size: s.toString() + ' KB',
                inputFile: f,
                status: FileUploadState.ReadyToUpload
            });
        }

        // setState({ ...state, files: result });
        config.onFilesChanged(result);
    }

    function onInputFilesChange(e: any) {
        onFilesChanged(e.target.files);
        // config.onFilesSelected({ files: e.target.files, fileKind: state.fileKind });
    }

    function onInputDirsChange(e: any) {
        onFilesChanged(e.target.files);
        // config.onFilesSelected({ files: e.target.files, fileKind: state.fileKind });
    }

    function onUpload() {
        config.onUpload({});
    }

    function onAbortUploading() {
        config.onAbortUploading();
    }

    function onClose() {
        config.onClose();
    }

    return (
        <Dialog open={props.open} maxWidth="md" fullWidth={true}>
            <AppBar sx={{ position: 'relative' }}>
                <Toolbar disableGutters>
                    <Box sx={{ display: 'flex', pl: 2, pr: 2, alignItems: 'center', width: '100%' }}>
                        <Icon sx={{ pr: 1 }}>
                            <FileUploadIcon></FileUploadIcon>
                        </Icon>
                        <Typography variant="h5" sx={{ flex: 1 }}>Upload files</Typography>
                        <IconButton color="inherit" edge="end" onClick={onClose}>
                            <CloseIcon />
                        </IconButton>
                    </Box>
                </Toolbar>
            </AppBar>
            <DialogTitle>
            </DialogTitle>
            <DialogContent >
                <Box sx={{ pt: 1, pb: 1, display: 'flex' }}>
                    <RadioGroup
                        row
                        value={state.fileKind}
                        onChange={onFileTypeChange}>
                        <FormControlLabel disabled={config.DialogState === DialogState.Uploading} value={FileKind.File} control={<Radio />} label="Select files"></FormControlLabel>
                        <FormControlLabel disabled={config.DialogState === DialogState.Uploading} value={FileKind.Directory} control={<Radio />} label="Select directories"></FormControlLabel>
                    </RadioGroup>
                    <Button variant="outlined" sx={{ flex: 1 }} onClick={onSelectFilesClick}>Select Files</Button>
                </Box>
                <Divider />

                <form style={{ display: "none" }}>
                    <input ref={filesInputRef} id="input-files" type="file" onChange={onInputFilesChange} multiple></input>
                    <input ref={dirsInputRef} id="input-dirs" type="file" onChange={onInputDirsChange}></input>
                </form>

                <BasicTable config={tableConfig}></BasicTable>
            </DialogContent>
            <DialogActions>
                <Button disabled={config.dialogState === DialogState.Uploading}
                    variant="outlined"
                    onClick={onClose}>Close</Button>
                <Button disabled={config.dialogState !== DialogState.Uploading}
                    variant="contained"
                    color="error"
                    onClick={onAbortUploading}>Abort upload</Button>
                <Button disabled={config.dialogState !== DialogState.Prepare}
                    variant="contained"
                    onClick={onUpload}>Upload</Button>
            </DialogActions>
        </Dialog>
    );
}

enum DialogState {
    Prepare = 1,
    Uploading = 2,
    UploadCompleted = 3
}

enum FileUploadState {
    ReadyToUpload = 1,
    UploadFail = 2,
    UploadSuccess = 3,
    Uploading = 4
}

enum FileKind {
    File = 1,
    Directory = 2
}

export { FileUploadState, FileKind, DialogState }