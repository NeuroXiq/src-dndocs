'use client'
import DetailsList from "@/components/DetailsList";
import Layout from "@/components/Layout";
import { Button, Grid, InputLabel, List, ListItemButton, ListItemIcon, ListItemText, MenuItem, OutlinedInput, Select, Stack, TextField, Typography } from "@mui/material";
import RotateRightIcon from '@mui/icons-material/RotateRight';
import PlayCircleOutlineIcon from '@mui/icons-material/PlayCircleOutline';
import UseApi from "services/Api";
import BasicTable from "@/components/BasicTable";
import { useEffect, useMemo, useState } from "react";

export default function Logs() {
    const api = UseApi();

    const logLevelIds = [
        { value: '1', name: '1' },
        { value: '2', name: '2' },
        { value: '3', name: '3' },
        { value: '4', name: '4' },
        { value: '5', name: '5' },
    ];

    const filterTypes = [
        { value: 'in', name: 'IN' },
        { value: 'like', name: 'LIKe' },
        { value: 'notlike', name: 'NOT LIKE' },
        { value: 'eq', name: 'EQUAL' },
        { value: 'neq', name: 'NOT EQUAL' },
        { value: 'gte', name: 'GREATER OR EQUAL' },
        { value: 'lte', name: 'LESS OR EQUAL' },
    ];

    let filterCols = [
        { value: 'Id', name: 'Id' },
        { value: 'CategoryName', name: 'Category Name' },
        { value: 'Date', name: 'Date' },
        { value: 'EventId', name: 'Event Id' },
        { value: 'LogLevelId', name: 'Log Level Id' },
        { value: 'Message', name: 'Message' },
    ];

    const [filters, setFilters] = useState<any>({
        logLevelIds: [],
        column: '',
        type: '',
    });

    const [getForm, setGetForm] = useState<any>({
        tableName: 'app_log',
        filters: [],
        orderBy: [],
        getRowsCount: true,
        pageNo: 0,
        rowsPerPage: 25,
    });

    // const [tableResponse, setTableResponse] = useState<any>({
    //     rowsCount: 0,
    //     currentPage: 0,
    //     rowsPerPage: 25,
    //     result: []
    // })

    const [state, setState] = useState<any>({
        tableLoading: false
    });

    const onPageChange = function (e: any) {
        setGetForm({
            ...getForm,
            pageNo: e.newPage
        });

        reloadTable();
    }

    const onRowsPerPageChange = function (e: any) {
        setGetForm({
            ...getForm,
            rowsPerPage: e.newRowsPerPage
        });

        reloadTable();
    }

    const [tableConfig, setTableConfig] = useState<any>({
        data: [],
        idKey: 'id',
        cols: [
            { id: 'id', title: 'Id', key: 'id' },
            { id: 'cn', title: 'Category Name', key: 'categoryName' },
            { id: 'date', title: 'Date', key: 'date' },
            { id: 'eventId', title: 'Event Id', key: 'eventId' },
            { id: 'eventName', title: 'Event Name', key: 'eventName' },
            { id: 'logLevelId', title: 'Log Level Id', key: 'logLevelId' },
            { id: 'message', title: 'Message', key: 'message' },
        ],
        pagination: {
            count: 0,
            page: 0,
            rowsPerPage: 0,
            rowsPerPageOptions: [25, 50, 100, 200],
            onPageChange: onPageChange,
            onRowsPerPageChange: onRowsPerPageChange
        }
    });



    const reloadTable = function () {
        const filtersData = [];
        const orderBy: any = [];

        if (filters.logLevelIds.length > 0) {
            filtersData.push({ column: 'LogLevelId', type: 'in', value: JSON.stringify(filters.logLevelIds) });
        }

        if (filters.column && filters.type && filters.searchtext) {
            let value = filters.searchtext;

            if (filters.type === 'in') {
                value = JSON.stringify(value.split(','));
            }

            filtersData.push({ column: filters.column, type: filters.type, value: value });
        }


        const model = {
            ...getForm,
            filters: filtersData,
            orderBy: orderBy,
        }

        setState({ ...state, tableLoading: true });

        api.Admin_GetTableData(model).then(dr => {
            const r = dr.result;
            const newTableConfig = {
                ...tableConfig,
                data: r.result,
                pagination: {
                    ...tableConfig.pagination,
                    count: r.rowsCount,
                    page: r.currentPage,
                    rowsPerPage: r.rowsPerPage
                }
            };

            console.log(newTableConfig);

            setTableConfig(newTableConfig);
            setState({ ...state, tableLoading: false });
        });
    }

    useEffect(() => reloadTable(), [getForm]);

    const onFiltersChange = function (e: any) {
        const name = e.target.name;

        setFilters({
            ...filters,
            [name]: e.target.value
        });

    }

    const select = function (name: string, label: string, values: any, multiple: boolean) {
        return (
            <>
                <InputLabel id="">{label}</InputLabel>
                <Select
                    name={name}
                    multiple={multiple}
                    value={filters[name]}
                    fullWidth
                    onChange={onFiltersChange}
                    input={<OutlinedInput />}
                >
                    {values.map((c: any) => {
                        return (
                            <MenuItem value={c.value} key={c.value}>{c.name}</MenuItem>
                        );
                    })}
                </Select>
            </>
        );
    }

    const tableMemo = useMemo(() => { return (<BasicTable config={tableConfig}></BasicTable>) }, [tableConfig])

    return (
        <Grid container spacing={2}>
            <Grid item md={2}>
                <Typography textAlign="center" variant="h5">Filters</Typography>
                <Stack spacing={1}>
                    {select('column', 'Columns', filterCols, false)}
                    {select('type', 'Filter Type', filterTypes, false)}
                    <InputLabel>Search Text</InputLabel>
                    <TextField fullWidth margin="normal" onChange={onFiltersChange} name="searchtext"></TextField>
                    {select('logLevelIds', 'Log Level Ids', logLevelIds, true)}
                    <Button fullWidth variant="contained" onClick={reloadTable}>Apply</Button>
                </Stack>
            </Grid>
            <Grid item md={10}>
                {tableMemo}
            </Grid>
        </Grid>
    );
}