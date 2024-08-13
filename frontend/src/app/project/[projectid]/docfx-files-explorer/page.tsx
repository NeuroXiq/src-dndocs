'use client'
import { Grid, ListItemIcon, MenuItem, Paper } from "@mui/material";
import Layout from "../../../../../components/Layout";
import TreeView from "../../../../../components/TreeView";
import NoteAddIcon from '@mui/icons-material/NoteAdd';
import EditIcon from '@mui/icons-material/Edit';
import DriveFileRenameOutlineIcon from '@mui/icons-material/DriveFileRenameOutline';
import DeleteForeverIcon from '@mui/icons-material/DeleteForever';
import CreateNewFolderIcon from '@mui/icons-material/CreateNewFolder';
import FolderDeleteIcon from '@mui/icons-material/FolderDelete';
import DrawerForm from "@/components/DrawerForm";
import DrawerAddDir from "./DrawerAddDir";
import { useContext, useEffect, useState } from "react";
import DrawerAddFile from "./DrawerAddFile";
import DrawerRename from "./DrawerRename";
import UseApi, { useQueryEffect } from "services/Api";
import { useParams } from "next/navigation";
import { ContentType } from "@/services/ApiDTO/Enum/Enums";
import UpdateFileDialog from "./UpdateFileDialog";
import UploadFileIcon from '@mui/icons-material/UploadFile';
import FolderIcon from '@mui/icons-material/Folder';
import InsertDriveFileIcon from '@mui/icons-material/InsertDriveFile';
import { GlobalAppContext } from "hooks/globalAppContext";
import FileUploadDialog, { DialogState, FileKind, FileUploadState } from "@/components/FileUploadDialog";
import { FullVPath, VPathCombine } from "./DocfxHelpers";
import ConfirmAction from "./ConfirmAction";
import PageLoading from "@/components/PageLoading";

interface UploadingDialogState {
    files: Array<any>,
    dialogState: DialogState
};

export default function DocfxFilesExplorer() {
    const gctx = useContext<any>(GlobalAppContext);
    const api = UseApi();
    const projectid = +(useParams() || {}).projectid;
    const [refreshTree, setRefreshTree] = useState<number>(1);

    const [fileUploadDialogState, setFileUploadDialogState] = useState<UploadingDialogState>({
        files: [],
        dialogState: DialogState.Prepare
    });

    const [filesUploadingIndex, setFilesUploadingIndex] = useState<number>(-1);

    const [state, setState] = useState<any>({
        menuItemAction: null,
        projectId: projectid,
        loading: false
    });

    const [actionState, setActionState] = useState<any>({
        projectId: projectid,
        fileData: null
    });

    const { result, loading } = useQueryEffect(() => api.ProjectManage_GetDocfxContentItems(projectid), [refreshTree]);

    let data: any = result || [];

    let fileMenu = [
        { info: 'updatefile', icon: <EditIcon />, text: 'Update File' }, //.
        { info: 'renamefile', icon: <DriveFileRenameOutlineIcon />, text: 'Rename File' }, //
        { info: 'movefile', icon: <DriveFileRenameOutlineIcon />, text: 'Move File' },
        { info: 'deletefile', icon: <DeleteForeverIcon />, text: 'Delete File' },
    ];

    let dirMenu = [
        { info: 'addfile', icon: <NoteAddIcon />, text: 'Create File' },
        { info: 'adddir', icon: <CreateNewFolderIcon />, text: 'Create Directory' },
        { info: 'movedir', icon: <CreateNewFolderIcon />, text: 'Move Directory' },
        { info: 'renamedir', icon: <DriveFileRenameOutlineIcon />, text: 'Rename Directory' },
        { info: 'uploadfile', icon: <UploadFileIcon />, text: 'Upload File(s)' },
        { info: 'deletedir', icon: <FolderDeleteIcon />, text: 'Delete Directory' },
    ];

    function onMenuItemClick(e: any) {
        const info = e.info;
        const drawers = [
            'addfile', 'deletefile', 'renamefile', 'adddir', 'deletedir', 'renamedir',
            'movedir'];

        setActionState({
            ...actionState,
            fileData: e.nodeData,
            info: info
        });

        setState({
            ...state,
            menuItemAction: info,
        });

        if (info === 'uploadfile') {
            setFileUploadDialogState({
                ...fileUploadDialogState,
                files: [],
                dialogState: DialogState.Prepare
            });
        }
    }

    function onEditFileDialogClose() {
        onActionCompleted();
    }

    function isAction(actionName: string) {
        return state.menuItemAction === actionName;
    }

    function getMenuItems(e: any) {
        if (!e) {
            return [];
        }

        if (e.type === ContentType.Folder) {
            return dirMenu;
        } else if (e.type > 0) {
            return fileMenu;
        }
    }

    function onActionCompleted() {
        setRefreshTree(refreshTree + 1);
        setState({ ...state, menuItemAction: null, loading: false });
    }

    let config = {
        expandedNodes: [],
        getUid: (a: any) => a.directory + a.name + a.type,
        getChildren: (a: any) => a.children,
        getText: function (node: any) { return node.name; },
        menu: {

        },
        menuItems: getMenuItems,
        onMenuItemClick: onMenuItemClick,
        menuOpenIcon: (node: any) => {
            if (node.type === 1) {
                return (<FolderIcon fontSize="small"></FolderIcon>);
            } else {
                return (<InsertDriveFileIcon fontSize="small"></InsertDriveFileIcon>);
            }
        }
    }

    function onDrawerClose() {
        setState({ ...state, menuItemAction: null });
    }

    function drawerConfig(drawerName: string) {
        return {
            onCloseClick: onDrawerClose,
            data: actionState,
            onSuccess: onActionCompleted
        };
    }

    useEffect(() => {
        if (filesUploadingIndex === fileUploadDialogState.files.length) {
            setFileUploadDialogState({ ...fileUploadDialogState, dialogState: DialogState.UploadCompleted });
            return;
        }

        if (fileUploadDialogState.dialogState !== DialogState.Uploading || filesUploadingIndex === -1) {
            setFilesUploadingIndex(-1);
            return;
        }

        const allFiles = fileUploadDialogState.files;
        const nextToUploadIndex = filesUploadingIndex;
        const nextToUpload = allFiles[nextToUploadIndex];

        const formdata = new FormData();
        let vpath = FullVPath(actionState.fileData);

        if (nextToUpload.inputFile.webkitRelativePath) {
            let rp = nextToUpload.inputFile.webkitRelativePath;
            rp = rp.substring(0, rp.length - nextToUpload.inputFile.name.length - 1);
            vpath = VPathCombine(FullVPath(actionState.fileData), rp);
        }

        formdata.append('projectid', projectid.toString());
        formdata.append('vpath', vpath);
        formdata.append('file', nextToUpload.inputFile);

        const onComplete = (r: any) => {
            const newfiles = [...allFiles];
            let newstatus = FileUploadState.UploadSuccess;
            let statusMsg = 'Completed';

            if (!r.success) {
                newstatus = FileUploadState.UploadFail;
                statusMsg = r.error?.errors?.join(', ');
            }

            newfiles[nextToUploadIndex].status = newstatus;
            newfiles[nextToUploadIndex].statusMsg = statusMsg;

            let nextIndex = (nextToUploadIndex + 1);

            setFilesUploadingIndex(nextIndex);
        };

        api.ProjectManage_DocfxUploadFile(formdata).then(onComplete);
    }, [filesUploadingIndex]);

    function onFilesChanged(e: any) {
        setFileUploadDialogState({ ...fileUploadDialogState, files: e });
    }

    function onFilesUploadClick(e: any) {
        setFileUploadDialogState({ ...fileUploadDialogState, dialogState: DialogState.Uploading });
        setFilesUploadingIndex(0);
    }

    function onFileUploadingClose() {
        setFilesUploadingIndex(-1);
        onActionCompleted();
    }

    const fileUploadConfig = {
        onAbortUploading: onFileUploadingClose,
        onFilesChanged: onFilesChanged,
        onUpload: onFilesUploadClick,
        onClose: onFileUploadingClose,
        files: fileUploadDialogState.files,
        dialogState: fileUploadDialogState.dialogState
    }

    function getConfirmDialogContent() {
        let msg = 'Are You sure You want to delete following ';

        if (state.menuItemAction === 'deletefile') {
            msg += 'file "' + FullVPath(actionState.fileData) + "'?";
        } else if (state.menuItemAction === 'deletedir') {
            msg += 'directory "' + FullVPath(actionState.fileData) + "'?";
        } else {
            msg = '';
        }

        return msg;
    }

    function getConfirmTitle() {
        if (state.menuItemAction === 'deletefile') {
            return 'Confirm Delete File';
        } else if (state.menuItemAction === 'deletedir') {
            return 'Confirm Delete Directory'
        }

        return null;
    }

    function onConfirmClick() {
        setState({ ...state, loading: true });
        const filepath = actionState.fileData ? FullVPath(actionState.fileData) : null;
        let promise = Promise.resolve(null);

        if (state.menuItemAction === 'deletefile') {
            promise = api.ProjectManage_DocfxRemoveFile({ projectId: projectid, filepath: filepath })
        } else if (state.menuItemAction === 'deletedir') {
            promise = api.ProjectManage_DocfxRemoveDirectory({ projectId: projectid, directorypath: filepath })
        } else {

        }

        promise.then(onActionCompleted);
    }

    function onConfirmCancel() {
        setState({
            ...state,
            menuItemAction: null
        });
    }

    return (
        <>
            <PageLoading open={state.loading} />
            <UpdateFileDialog open={isAction('updatefile')} onClose={onEditFileDialogClose} data={actionState}></UpdateFileDialog>
            <FileUploadDialog config={fileUploadConfig} open={isAction('uploadfile')}></FileUploadDialog>
            <DrawerAddFile open={isAction('addfile')} config={drawerConfig('addfile')} data={actionState} />
            <DrawerRename
                open={isAction('renamefile') || isAction('renamedir') || isAction('movefile') || isAction('movedir')}
                config={drawerConfig('rename')}
                data={actionState} />
            <DrawerAddDir open={isAction('adddir')} config={drawerConfig('adddir')} data={actionState} />
            <ConfirmAction
                open={isAction('deletefile') || isAction('deletedir')}
                title={getConfirmTitle()}
                content={getConfirmDialogContent()}
                onConfirm={onConfirmClick}
                onCancel={onConfirmCancel} />

            <Layout title="Docfx Files">
                <Grid container spacing={2}>
                    <Grid item xs={12}>
                        <Paper>
                        </Paper>
                    </Grid>
                    <Grid item xs={12}>
                        <Paper sx={{ height: '75vh' }}>
                            <TreeView config={config} data={data}>
                            </TreeView>
                        </Paper>
                    </Grid>
                </Grid>
            </Layout>
        </>
    );
}