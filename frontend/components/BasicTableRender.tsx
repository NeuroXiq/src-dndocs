'use client';
import { Box, Button, Checkbox, Dialog, DialogActions, DialogContent, DialogTitle, IconButton, List, ListItem, ListItemButton, ListItemIcon, ListItemText, Paper, Skeleton, Table, TableBody, TableCell, TableContainer, TableFooter, TableHead, TablePagination, TableRow } from "@mui/material";
import ViewWeekIcon from '@mui/icons-material/ViewWeek';
import { useState } from "react";
import DoneAllIcon from '@mui/icons-material/DoneAll';
import ClearAllIcon from '@mui/icons-material/ClearAll';

export default function BasicTableRender(props : any) {
    const config = props.config;
    let cols = config.cols;
    const tempSelectCols = config.tempSelectCols;
    const setTempSelectCols = config.setTempSelectCols;
    const setVisibleCols = config.setVisibleCols;
    const onSelectColsClick = config.onSelectColsClick;
    const onSelectColsSave = config.onSelectColsSave;
    const onSelectColsCancel = config.onSelectColsCancel
    const onSelectColsClearAll = config.onSelectColsClearAll;
    const onSelectColsSelectAll = config.onSelectColsSelectAll;
    const showSelectColsDialog = config.showSelectColsDialog;
    const visibleCols = config.visibleCols;
    const pagination = config.pagination;
    const data = config.data;
    const idKey = config.idKey;
    const showColumnsSelectionButton = config.showColumnsSelectionButton;
    const onPageChange = config.onPageChange;
    const onRowsPerPageChange = config.onRowsPerPageChange;
    
    const tableContainerSx = props.tableContainerSx || {};

    const getColById = function (id: any) {
        const col = cols.find((c: any) => c.id === id);

        if (!col) {
            throw new Error(`column with id: "${id}" was not found `);
        }

        return col;
    }

    const theadTitle = function (id: any) {
        const col = getColById(id);

        if (!('title' in col)) {
            throw new Error('"title" property no found in column config. column config id: ' + id);
        }

        return col.title;
    }

    const tbodyCellValue = function (colid: any, dataRow: any) {
        let col = getColById(colid);

        if (!(col.key in dataRow)) {
            console.log(dataRow);
            throw new Error(`${col.key} property not found in row object. Row id: ${dataRow[idKey]}`);
        }

        return dataRow[col.key];
    }

    const onSelectColClick = function (colid: any) {
        let curIndex = tempSelectCols.indexOf(colid);
        let newcols = [...tempSelectCols];

        if (curIndex > -1) {

            newcols.splice(curIndex, 1);
        } else {
            newcols.push(colid)
        }

        setTempSelectCols(newcols);
    }

    const selectColListItem = function (colid: any) {
        let colConfig = cols.find((a: any) => a.id === colid);
        let checked = tempSelectCols.some((c: any) => { return c === colid });

        return (
            <ListItem key={colConfig.id}>
                <ListItemButton onClick={() => onSelectColClick(colConfig.id)}>
                    <ListItemIcon>
                        <Checkbox edge="start" checked={checked}></Checkbox>
                    </ListItemIcon>
                    <ListItemText primary={colConfig.title}></ListItemText>
                </ListItemButton>
            </ListItem>
        );
    }

    const getCellKey = function (colid: any, rowData: any) {
        return rowData[idKey].toString() + colid.toString();
    }
    
    return (
        <>
            {!!showColumnsSelectionButton && <Dialog open={showSelectColsDialog}>
                <DialogTitle>Select Columns</DialogTitle>
                <DialogContent>
                    <List>
                        {cols.map((c: any) => selectColListItem(c.id))}
                    </List>
                </DialogContent>
                <DialogActions>
                    <Button variant="outlined" startIcon={<DoneAllIcon />} onClick={onSelectColsClearAll}>Clear All</Button>
                    <Button variant="outlined" startIcon={<ClearAllIcon />} onClick={onSelectColsSelectAll}>Select All</Button>
                    <Button variant="outlined" onClick={onSelectColsCancel}>Cancel</Button>
                    <Button variant="outlined" onClick={onSelectColsSave}>Save</Button>
                </DialogActions>
            </Dialog>
            }
            <TableContainer sx={tableContainerSx }>
                <Box>
                    <IconButton onClick={onSelectColsClick}>
                        <ViewWeekIcon />
                    </IconButton>
                </Box>
                <Table>
                    <TableHead>
                        <TableRow>
                            {visibleCols.map((c: any) => (<TableCell key={c}>{theadTitle(c)}</TableCell>))}
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {data.map((d: any) => (
                            <TableRow key={d[idKey]}>
                                {visibleCols.map((c: any) => (
                                    <TableCell
                                        key={getCellKey(c, d)}
                                        id={getCellKey(c, d)}>
                                        {tbodyCellValue(c, d)}
                                    </TableCell>
                                ))}
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </TableContainer>
            <TablePagination
                rowsPerPageOptions={pagination.rowsPerPageOptions}
                component="div"
                count={pagination.count}
                rowsPerPage={pagination.rowsPerPage}
                page={pagination.page}
                onPageChange={onPageChange}
                onRowsPerPageChange={onRowsPerPageChange}>
            </TablePagination>
        </>
    );
}