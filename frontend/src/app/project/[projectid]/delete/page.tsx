'use client'
import DetailsList, { ItemCfg } from "@/components/DetailsList";
import Layout from "@/components/Layout";
import Typoheader from "@/components/Typoheader";
import UseApi, { useQueryEffect } from "@/services/Api";
import ProjectDto from "@/services/ApiDTO/Project/ProjectDto";
import { Box, Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle, InputLabel, Paper, Stack, TextField, Typography } from "@mui/material";
import Urls from "config/Urls";
import { useParams, useRouter } from "next/navigation";
import { useState } from "react";

export default function Delete() {
    const api = UseApi();
    const params = useParams() || {};
    const router = useRouter();
    const projectid = +params.projectid;
    const [state, setState] = useState<any>({ confirmCode: '', showSuccessDialog: false });

    const { loading, result } = useQueryEffect<ProjectDto>(() => api.ProjectManage_GetProjectById(projectid));

    let validConfirmCode = '';

    if (!loading) {
        validConfirmCode = (result.urlPrefix + '_' + result.id).toUpperCase();
    }

    const dlc : ItemCfg<ProjectDto>[] = [
        { title: 'Id', val: (p) => p.id },
        { title: 'URL prefix', val: p => p.urlPrefix },
        { title: 'Project name', val: p => p.projectName },
        { title: 'Github url', val: p => p.githubUrl },
    ]

    function onConfirm() {
        api.ProjectManage_DeleteProject(projectid).then((r: any) => {
            if (r.success) {
                setState({ ...state, showSuccessDialog: true })
            }
        });
    }

    function onCancel() {
        router.push(Urls.account.details);
    }

    function onConfirmCodeChange(e: any) {
        setState({ ...state, confirmCode: e.target.value });
    }

    return (
        <Layout>
            <Dialog open={state.showSuccessDialog}>
                <DialogTitle>Success</DialogTitle>
                <DialogContent><DialogContentText>Project has been deleted successfully</DialogContentText></DialogContent>
                <DialogActions>
                    <Button onClick={() => router.push(Urls.account.details)}>Account</Button>
                </DialogActions>
            </Dialog>
            <Typoheader mode="error">Delete project</Typoheader>
            <Typoheader variant="h6" mode="error" textAlign="center">Are You sure You want to delete this project?</Typoheader>
            <DetailsList loading={loading} data={result} itemsConfig={dlc}>
            </DetailsList>
            <Paper sx={{ padding: 2, mt: 1 }}>
                <form>
                    <Stack spacing={2}>
                        <Stack spacing={1}>
                            <InputLabel>Code</InputLabel>
                            <TextField fullWidth disabled={true} value={validConfirmCode}></TextField>
                            <InputLabel>Rewrite above code to field below and confirm to delete project</InputLabel>
                            <TextField color="error" fullWidth onChange={onConfirmCodeChange}>asdf</TextField>
                        </Stack>
                        <Box sx={{ display: 'flex', gap: 1, justifyContent: 'flex-end' }}>
                            <Button onClick={onCancel} color="warning" variant="outlined">Cancel</Button>
                            <Button
                                onClick={onConfirm}
                                disabled={state.confirmCode !== validConfirmCode}
                                color="error"
                                variant="contained">Confirm</Button>
                        </Box>
                    </Stack>
                </form>

            </Paper>
        </Layout>
    );
}