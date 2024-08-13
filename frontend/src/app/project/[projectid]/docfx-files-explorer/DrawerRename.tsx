import DrawerForm from "@/components/DrawerForm";
import { FormLabel, TextField } from "@mui/material";
import { useContext, useEffect, useRef, useState } from "react";
import { FullVPath, VPathCombine } from "./DocfxHelpers";
import { stat } from "fs";
import UseApi from "services/Api";
import { GlobalAppContext } from "hooks/globalAppContext";

export default function DrawerAddDir(props: any) {
    const [formState, setFormState] = useState<any>({
        vpathSrc: '',
        vpathDest: ''
    });

    const api = UseApi();

    const [state, setState] = useState<any>({
        projectId: '',
        title: '',
        labelCurVPath: '',
        labelNewVPath: ''
    });

    useEffect(() => {
        if (!props.open) {
            return;
        }

        const fd = props.data.fileData;
        const info = props.data.info;

        if (!fd) {
            return;
        }

        if (!['renamedir', 'renamefile', 'movedir', 'movefile'].some((c: any) => c === info)) {
            throw new Error('invalid info');
        }

        let newVpath = '', curVpath = '';

        let title = '', newvpathLabel = '', curvpathLabel = '';

        switch (info) {
            case 'renamedir':
                curVpath = fd.name
                title = 'Rename Directory';
                newvpathLabel = 'New directory name';
                curvpathLabel = 'Current directory name';
                break;
            case 'renamefile':
                title = 'Rename File';
                curVpath = fd.name
                curvpathLabel = 'Current file name'
                newvpathLabel = 'New file name'
                break;
            case 'movedir':
                curVpath = FullVPath(fd);
                title = 'Move Directory';
                curvpathLabel = 'Current Directory Path';
                newvpathLabel = 'New Directory Path';
                break;
            case 'movefile':
                curVpath = FullVPath(fd);
                title = 'Move File';
                curvpathLabel = 'Current File Path';
                newvpathLabel = 'New File Path';
                break;
            default: break;
        }

        setFormState({
            vpathSrc: curVpath,
            vpathDest: newVpath,
        });

        setState({
            projectId: props.data.projectId,
            renameType: info,
            title: title,
            fileData: fd,
            labelCurVPath: curvpathLabel,
            labelNewVPath: newvpathLabel
        });

        // move file

        // move dir
    }, [props.data])

    function onSubmitPostCompleted(r: any) {
        if (!r.success) {
            return;
        }

        props.config.onSuccess();
    }

    function onSubmit() {
        let form = {
            vpathSrc: '',
            vpathDest: '',
            projectId: state.projectId,
        }

        form.vpathSrc = formState.vpathSrc;

        switch (state.renameType) {
            case 'renamedir':
                form.vpathSrc = FullVPath(state.fileData);
                form.vpathDest = VPathCombine(state.fileData.directory, formState.vpathDest);
                api.ProjectManage_DocfxMoveDirectory(form).then(onSubmitPostCompleted);
                break;
            case 'renamefile':
                form.vpathDest = VPathCombine(state.fileData.directory, formState.vpathDest);
                api.ProjectManage_DocfxMoveFile(form).then(onSubmitPostCompleted);
                break;
            case 'movedir':
                form.vpathDest = formState.vpathDest;
                api.ProjectManage_DocfxMoveDirectory(form).then(onSubmitPostCompleted);
                break;
            case 'movefile':
                break;
        }
    }

    const config = {
        onCloseClick: props.config.onCloseClick,
        onSubmitClick: onSubmit,
        title: state.title
    };

    function onVPathDestChange(e: any) {
        setFormState({...formState, vpathDest: e.target.value})
    }

    return (
        <DrawerForm open={props.open} config={config}>
            <FormLabel>{state.labelCurVPath}</FormLabel>
            <TextField
                margin="normal"
                value={formState.vpathSrc}
                disabled={true}>
            </TextField>
            <FormLabel>{state.labelNewVPath}</FormLabel>
            <TextField
                margin="normal"
                value={formState.vpathDest}
                onChange={onVPathDestChange}></TextField>
        </DrawerForm>
    )
}