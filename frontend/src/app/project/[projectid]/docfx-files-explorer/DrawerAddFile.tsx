import DrawerForm from "@/components/DrawerForm";
import { PermDeviceInformation } from "@mui/icons-material";
import { InputLabel, MenuItem, Select, TextField } from "@mui/material";
import { useEffect, useState } from "react";
import UseApi from "services/Api";

export default function DrawerAddFile(props: any) {
    const [state, setState] = useState<any>({
        parentDirectory: '',
        fileName: '',
        extension: 'md'
    });

    const api = UseApi();

    useEffect(() => {
        if (!props.config.data?.fileData) {
            return;
        }

        const fileData = props.config.data.fileData;
        const projectId = props.config.data.projectId;
        
        let pdir = fileData.directory;
        
        if (!pdir.endsWith('/')) {
            pdir += '/';
        }

        pdir += fileData.name;

        setState({ parentDirectory: pdir, fileName: '', extension: 'md', projectid: projectId });
    }, [props.config])

    function onSubmit(e: any) {
        api.ProjectManage_DocfxCreateFile(state).then(r => {
            if (r.success) {
                props.config.onSuccess();
            }
        });
    }

    function onClose(e: any) {
        props.config.onCloseClick();
    }

    const config = {
        title: 'Add File',
        onSubmitClick: onSubmit,
        onCloseClick: onClose
    }

    function onFileNameChange(e: any) {
        setState({...state, fileName: e.target.value});
    }

    function onExtensionChange(e: any) {
        setState({...state, extension: e.target.value})
    }

    return (
        <DrawerForm open={props.open} config={config}>
            <InputLabel>Parent Directory</InputLabel>
            <TextField value={state.parentDirectory} disabled={true} margin="normal"></TextField>
            <InputLabel>New File Name</InputLabel>
            <TextField value={state.fileName} onChange={onFileNameChange}></TextField>
            <InputLabel onChange={onExtensionChange}>Extension</InputLabel>
            <Select value={state.extension}>
                <MenuItem value="md">md</MenuItem>
                <MenuItem value="yml">yml</MenuItem>
            </Select>
        </DrawerForm>
    )
}