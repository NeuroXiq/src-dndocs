'use client'
import BasicTable from "@/components/BasicTable";
import Layout from "@/components/Layout";
import { Box, Button, FormControlLabel, Grid, Paper, Radio, RadioGroup, TextField, Typography } from "@mui/material";
import { useContext, useState } from "react";
import FormLabel from '@mui/material/FormLabel';
import UseApi from '@/services/Api';
import { GlobalAppContext, IGlobalAppContextValue } from "hooks/globalAppContext";
import RawHtmlTable from "@/components/RawHtmlTable";

export default function Sql() {
    const api = UseApi();
    const gc = useContext<IGlobalAppContextValue>(GlobalAppContext);

    const [state, setState] = useState<any>({
        mode: 1,
        sqlCode: '',
        dbname: 'app/appdb.sqlite'
    });

    const [tableConfig, setTableConfig] = useState<any>({
        cols: [],
        data: []
    });

    const [error, setError] = useState<any>(null);

    const onSqlChange = function (e: any) {
        setState({ ...state, sqlCode: e.target.value })
    }

    const onModeChange = function (e: any) {
        setState({ ...state, mode: e.target.value });
    }

    const onKeyDown = function (e: any) {
        if (e.ctrlKey && e.keyCode === 13) {
            executeSql();
        }
    }

    function getArrayLastExecuted(): any {
        if (!localStorage) {
            return;
        }

        let lastExecuted: any = localStorage.getItem('LAST_EXECUTED_SQLS');

        if (!lastExecuted) {
            lastExecuted = [];
        } else {
            lastExecuted = JSON.parse(lastExecuted);
        }

        return lastExecuted;
    }

    function saveLastExecSql() {
        if (!localStorage) {
            return;
        }

        let lastExecuted: any = getArrayLastExecuted();

        if (lastExecuted?.length > 0 && lastExecuted[lastExecuted.length - 1] === state.sqlCode) {
            // return because does not make sens saving same 
            return;
        }

        lastExecuted.push(state.sqlCode);

        if (lastExecuted.length > 30) {
            lastExecuted = lastExecuted.slice(-40);
        }

        localStorage.setItem('LAST_EXECUTED_SQLS', JSON.stringify(lastExecuted));
    }

    const executeSql = function () {
        const form = {
            ...state,
            mode: +state.mode
        }

        saveLastExecSql();

        api.Admin_ExecuteRawSql(form).then(r => {
            const result = r.result;
            if (result.success) {
                setError(null);

                if (form.mode === 2) {
                    gc.showSuccessMessage("action success");
                    return;
                }

                let idKey = result.columns.indexOf((c: any) => c === 'id');
                if (idKey < 0) {
                    idKey = null;
                }

                const newConfig = {
                    cols: result.columns.map((c: any, i: any) => { return { id: i, title: c, key: i, val: (p: any) => { <pre>{p[c]}</pre>} } }),
                    idKey: idKey,
                    data: result.rows
                }

                setTableConfig(newConfig);
            } else {
                setError(result.exception);
            }
        });
    }

    const onDbNameChange = function (e: any) {
        setState({ ...state, dbname: e.target.value });
    }

    function renderLastExecutedSqls(): any {
        if (!localStorage) {
            null;
        }

        let lastSql = getArrayLastExecuted().reverse();

        return (
            <Box>
                {lastSql.map((sql: any, index: any) => {
                    return (
                        <div key={index} onClick={() => setState({ ...state, sqlCode: sql })}>
                            <Paper  sx={{ p: 1, m: 1 }} >
                                <pre >{sql}</pre>
                            </Paper>
                        </div>
                    )
                })}
            </Box>
        );

    }

    return (
        <Grid container>
            <Grid item md={12}>
                <Typography color="error.main">
                    {error}
                </Typography>
            </Grid>
            <Grid item md={4}>
                <TextField
                    onKeyDown={onKeyDown}
                    onChange={onSqlChange}
                    multiline
                    rows={8}
                    fullWidth
                    value={state.sqlCode}
                    margin="normal">
                </TextField>

                <Grid container marginTop={1} spacing={2}>
                    <Grid item md={6}>
                        <FormLabel>Mode</FormLabel>
                        <RadioGroup
                            onChange={onModeChange}
                            value={state.mode}
                            row>
                            <FormControlLabel value={1} control={<Radio />} label="Execute Reader" />
                            <FormControlLabel value={2} control={<Radio />} label="Execute Non Query" />
                        </RadioGroup>
                    </Grid>
                    <Grid item md={6}>
                        <FormLabel>Execute code</FormLabel>
                        <Button variant="outlined" fullWidth onClick={executeSql}>Execute (ctrl + enter)</Button>
                    </Grid>
                    <Grid item md={6}>
                        <FormLabel>Db name</FormLabel>
                        <TextField
                            fullWidth
                            margin="normal"
                            value={state.dbname}
                            onChange={onDbNameChange}></TextField>
                    </Grid>
                </Grid>
                <Grid md={12} item overflow="auto" maxHeight="500px">
                    {renderLastExecutedSqls()}
                </Grid>
            </Grid>
            <Grid item md={8}>
                {tableConfig?.data != null ? <RawHtmlTable data={tableConfig.data} cols={tableConfig.cols} /> : null}
            </Grid>

        </Grid>
    );
}