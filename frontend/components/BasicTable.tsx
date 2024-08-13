'use client';
import { Box, Button, Checkbox, Dialog, DialogActions, DialogContent, DialogTitle, IconButton, List, ListItem, ListItemButton, ListItemIcon, ListItemText, Paper, Skeleton, Table, TableBody, TableCell, TableContainer, TableFooter, TableHead, TablePagination, TableRow } from "@mui/material";
import ViewWeekIcon from '@mui/icons-material/ViewWeek';
import { useMemo, useState } from "react";
import DoneAllIcon from '@mui/icons-material/DoneAll';
import ClearAllIcon from '@mui/icons-material/ClearAll';
import BasicTableRender from "./BasicTableRender";

export default function BasicTable(props: any) {
    // if (props.loading) {
    //     return (
    //         <Skeleton variant="rectangular" sx={{ height: '4rem' }}></Skeleton>
    //     );
    // }

    let config = props.config;
    let idKey = config.idKey;
    let data = config.data;
    let pagination = config.pagination;

    if (!data) {
        data = [];
    }

    if (!Array.isArray(data)) {
        throw new Error('"data" field is not an array');
    }

    if (!idKey) {
        data = data.map((d: any) => {
            return {
                ...d,
                BASICTABLE_AUTOID: Math.random()
            }
        });

        idKey = 'BASICTABLE_AUTOID';
    }

    if (!pagination) {
        pagination = {};
        pagination.count = data.length;
        pagination.page = 0;
        pagination.rowsPerPage = data.length;
        pagination.rowsPerPageOptions = [data.length];
    } else {
        const validate = [
            pagination.count, pagination.page,
            pagination.rowsPerPage];

        validate.forEach(c => {
            if (typeof c !== 'number') {
                throw new Error('pagination invalid values');
            }
        });
    }

    const [showSelectColsDialog, setShowSelectColsDialog] = useState<boolean>(false);
    const [tempSelectCols, setTempSelectCols] = useState<any>([]);
    const [visibleCols, setVisibleCols] = useState<any>([]);
    const [cols, setCols] = useState<any>([]);
    const [propsCols, setPropsCols] = useState<any>([]);

    let colsChanged = cols.length !== config.cols.length;

    for (let i = 0; i < cols.length && !colsChanged; i++) {
        colsChanged = cols[i] !== config.cols[i]
    }

    if (colsChanged) {
        setCols(config.cols);

        let newPropsCols: any = config.cols.map((c: any) => {
            if (!c.id) {
                return {
                    ...c,
                    id: c.title
                }
            }

            return c;
        });

        const newVisible = config.visibleCols || newPropsCols.map((c: any) => { return c.id });
        setVisibleCols(newVisible);
        setPropsCols(newPropsCols);
        setTempSelectCols(newVisible);
    }

    const onPageChange = function (e: any, newPage: any) {
        if (config.pagination.onPageChange) {
            config.pagination.onPageChange({
                event: e,
                newPage: newPage
            });
        }
    }

    const onRowsPerPageChange = function (e: any) {
        if (config.pagination?.onRowsPerPageChange) {
            config.pagination.onRowsPerPageChange({
                event: e,
                newRowsPerPage: e.target.value
            });
        }
    }

    const getColById = function (id: any) {
        const col = propsCols.find((c: any) => c.id === id);

        if (!col) {
            throw new Error(`column with id: "${id}" was not found `);
        }

        return col;
    }

    const onSelectColsClick = function () {
        setShowSelectColsDialog(p => !p);
    }

    const onSelectColClick = function (colid: any) {
        let curIndex = tempSelectCols.indexOf(colid);
        let newcols = [...tempSelectCols];

        if (curIndex > 0) {

            newcols.splice(curIndex, 1);
        } else {
            newcols.push(colid)
        }

        setTempSelectCols(newcols);
    }

    const onSelectColsSave = function () {
        const newVisible = tempSelectCols.sort((a: any, b: any) => {
            let ai = -1, bi = -1;

            for (let i = 0; i < propsCols.length; i++) {
                let w = propsCols[i];
                if (w.id === a) {
                    ai = i;
                }

                if (w.id === b) {
                    bi = i;
                }
            }

            return ai - bi;

        });

        setVisibleCols(newVisible);
        setShowSelectColsDialog(false);
    }

    const onSelectColsCancel = function () {
        setTempSelectCols(visibleCols);
        setShowSelectColsDialog(false);
    }

    const onSelectColsClearAll = function () {
        setTempSelectCols([]);
    }

    const onSelectColsSelectAll = function () {
        setTempSelectCols(propsCols.map((c: any) => c.id));
    }

    const getCellKey = function (colid: any, rowData: any) {
        return rowData[idKey].toString() + colid.toString();
    }

    const renderConfig = {
        tempSelectCols: tempSelectCols,
        setTempSelectCols: setTempSelectCols,
        setVisibleCols: setVisibleCols,
        onSelectColsClick: onSelectColsClick,
        onSelectColsSave: onSelectColsSave,
        onSelectColsCancel: onSelectColsCancel,
        onSelectColsClearAll: onSelectColsClearAll,
        onSelectColsSelectAll: onSelectColsSelectAll,
        onPageChange: onPageChange,
        onRowsPerPageChange: onRowsPerPageChange,
        showSelectColsDialog: showSelectColsDialog,
        visibleCols: visibleCols,
        pagination: pagination,
        cols: propsCols,
        data: data,
        idKey: idKey,
        showColumnsSelectionButton: true
    };
    
    return (
        <BasicTableRender config={renderConfig} tableContainerSx={props.tableContainerSx}></BasicTableRender>
    );

}