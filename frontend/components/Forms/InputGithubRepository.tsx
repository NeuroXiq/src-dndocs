'use client'
import { Box, CircularProgress, IconButton, Stack } from "@mui/material";
import { getFieldErrors, getInputValue } from "./FormHelpers";
import InputSelect from "./InputSelect";
import InputText from "./InputText";
import RefreshIcon from '@mui/icons-material/Refresh';
import { useCallback, useEffect, useState } from "react";
import UseApi from "@/services/Api";
import useGlobalAppContext from "hooks/useGlobalAppContext";

export default function InputGithubRepository(props: any) {
    const value = getInputValue(props.name, props.value, props.valueFn);
    const error = getFieldErrors(props.name, props.errors, props.errorsFn);
    const api = UseApi();

    const [state, setState] = useState<any>({
        repos: [],
        initLoaded: false,
        loading: false
    });

    const refresh = useCallback((flush: boolean) => {
        if ((state.initLoaded && !flush) || state.loading) {
            return;
        }

        setState((s: any) => { return {...s, loading: true}});

        api.MyAccount_GetGithubRepositories(flush).then((r: any) => {
            const items = r.result.map(((r: any) => { return { text: r.cloneUrl, value: r.cloneUrl } }));
            setState({loaded: true, loading: false, repos: items});
        });
    }, [state, api]);

    useEffect(() => { refresh(false)}, []);

    function onRefreshClick() {
        refresh(true);
    }

    function onRepoChange(e: any) {
        props?.onChange?.({
            name: props.name,
            newValue: e.newValue
        });
    }

    return (
        <>
            <Stack direction="row" spacing={1} alignItems="center">
                <Box sx={{ flex: '1' }}>
                    <InputSelect
                        disabled={state.loading}
                        items={state.repos}
                        name={props.name + "_nupkgs"}
                        label="Github repository with .MD docs" value={value} onChange={onRepoChange}></InputSelect>
                </Box>
                <Box sx={{ minWidth: "4rem" }}>
                    <IconButton disabled={state.loading} onClick={onRefreshClick}>
                        {state.loading && <CircularProgress />}
                        {!state.loading && <RefreshIcon fontSize="large" />}
                    </IconButton>
                </Box>
            </Stack>
            <InputText name={props.name} label="Github repository with .MD docs" value={value} onChange={onRepoChange} errors={error}>Repo</InputText>
        </>
    );
}