import DrawerForm from "@/components/DrawerForm";
import { FormLabel, TextField } from "@mui/material";
import { useEffect, useState } from "react";
import { VPathCombine } from "./DocfxHelpers";
import UseApi from "services/Api";

export default function DrawerAddDir(props: any) {
    const [state, setState] = useState<any>({
        projectId: '',
        parentDir: '',
        newDirName: '' 
    });

    const api = UseApi();

    useEffect(() => {
        if (!props.open) {
            return;
        }
        const fd =  props.data.fileData;

        setState({
            projectId: props.data.projectId,
            parentDir: VPathCombine(fd.directory, fd.name),
            newDirName: ''
        });


    }, [props.data])

    function onSubmit() {
        const newDir = VPathCombine(state.parentDir, state.newDirName);
        const form = {
            directory: newDir,
            projectId: state.projectId
        };

        api.ProjectManage_DocfxCreateDirectory(form).then(r => {
            if (r.success) {
                props.config.onSuccess();
            }
        });
        
    }

    function onNewDirChange(e: any) {
        setState({
            ...state,
            newDirName: e.target.value
        })
    }

    const config = {
        onSubmitClick: onSubmit,
        onCloseClick: props.config.onCloseClick,
        title: 'Create Directory'
    }

    return (
        <DrawerForm open={props.open} config={config}>
            <FormLabel>Parent directory</FormLabel>
            <TextField value={state.parentDir} disabled={true} margin="normal"></TextField>
            <FormLabel>New Directory Name</FormLabel>
            <TextField value={state.newDirName} onChange={onNewDirChange} margin="normal"></TextField>
        </DrawerForm>
    )
}