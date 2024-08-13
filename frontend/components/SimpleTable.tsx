import { Box, Collapse, IconButton, LinearProgress, Menu, MenuItem, Skeleton, Table, TableBody, TableCell, TableContainer, TableHead, TablePagination, TableRow, Tooltip, Typography } from "@mui/material"
import MenuOpenIcon from '@mui/icons-material/MenuOpen';
import { useEffect, useState } from "react";
import React from "react";
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';
import KeyboardArrowUpIcon from '@mui/icons-material/KeyboardArrowUp';

export default function SimpleTable(props: any) {
    let config = props.config;
    let expand = config.expand;
    let cols: any = config.cols;
    let data: any = config.data;
    let rowidkey = config.rowidkey || 'id';
    let pagination: any = config.pagination;
    let contextMenu = config.contextMenu;
    let events = props.config.events;

    const [menuState, setMenuState] = useState<any>(null);
    const [expandedState, setExpandedState] = useState<any>([]);

    useEffect(() => { setExpandedState([]) }, [data]);

    if (pagination) {
        if (!pagination.rowsPerPageOptions) {
            pagination.rowsPerPageOptions = [10, 20, 50, 100, 200];
        }
    }

    if (contextMenu) {
        data = data.map((d: any) => {
            return { ...d, stContextMenuButton: createContextMenuButton(d) }
        });

        if (!cols.find((c: any) => c.dp === 'stContextMenuButton')) {
            cols.push({ id: 'stContextMenuButton', title: '', dp: 'stContextMenuButton', width: '48px' });
        }
    }

    if (expand && !cols.find((c: any) => c.id === '__expand')) {
        let cols2 = [...cols];
        cols2.unshift({ id: '__expand', title: '', width: '32px', val: (a: any) => { throw Error('special column') } });
        cols = cols2;
    }

    function createContextMenuButton(rowData: any) {
        return (
            <Tooltip title="Actions">
                <IconButton onClick={(e: any) => onOpenMenuClick(e, rowData)}>
                    <MenuOpenIcon fontSize="small" />
                </IconButton>
            </Tooltip>
        );
    }

    function onOpenMenuClick(e: any, rowData: any) {
        setMenuState({ anchor: e.currentTarget, rowData: rowData });
    }

    function getColKey(c: any) {
        if (c.id) {
            return c.id;
        } else if (c.name) {
            return c.name;
        } else if (c.title) {
            return c.title;
        }

        console.error('column does not have id and name', c);
        throw new Error('column does not have id and name');
    }

    function getRowKey(d: any) {
        let result = null;

        if (config.rowidval) {
            result = config.rowidval(d);
        }
        else if (d[rowidkey]) {
            result = d[rowidkey];
        }

        if (!result) {
            console.error('row does not have id', d);
            throw new Error('row does not have id');
        }

        return result;
    }

    function getCellKey(c: any, d: any) {
        return getRowKey(d) + '-' + getColKey(c);
    }

    function tbodyCellValue(c: any, d: any) {
        let val: any = null;

        if (c.id === '__expand') {
            const key = getRowKey(d);
            const expanded = expandedState.includes(key);
            return (
                <IconButton
                    aria-label="expand row"
                    size="small"
                    onClick={() => {
                        let newState = null;

                        if (expanded) {
                            newState = expandedState.filter((d: any) => d !== key);
                        } else {
                            newState = [...expandedState];
                            newState.push(key);
                        }

                        setExpandedState(newState);
                    }}>
                    {expanded ? <KeyboardArrowUpIcon /> : <KeyboardArrowDownIcon />}
                </IconButton>
            );
        }

        if (c.val) {
            val = c.val(d);
        } else {
            val = d[c.dp];
        }

        if (val === undefined) {
            console.error('val is underfined for table cell: ', c);
        }

        if (!val || typeof val === 'object') {
            return val;
        }

        let variant: any = 'body2';
        let color: any = null;
        let onclick: any = null;
        let sx = null;

        if (c.copyOnClick) {
            sx = { '&:hover': { color: 'grey.800', cursor: 'pointer' }, '&:active': { color: 'grey.700', cursor: 'pointer' } };
            onclick = () => navigator.clipboard.writeText(val);
        }

        let cell = (<Typography sx={sx} variant={variant} color={color} onClick={onclick}>{val}</Typography>);

        if (c.copyOnClick) {
            cell = <Tooltip title="Copy">{cell}</Tooltip>
        }

        return cell;
    }

    function theadTitle(c: any) {
        return c.title;
    }

    function onPageChange(e: any, newPage: any) {
        if (events?.onPageChange) {
            events?.onPageChange({
                newValue: newPage
            });
        }
    }

    function onRowsPerPageChange(e: any) {
        if (events?.onPageChange) {
            events?.onRowsPerPageChange({
                newValue: e.target.value
            });
        }
    }

    function paginationComponent() {
        if (!config.pagination) {
            return;
        }

        return (<TablePagination
            rowsPerPageOptions={pagination.rowsPerPageOptions}
            component="div"
            count={pagination.count}
            rowsPerPage={pagination.rowsPerPage}
            page={pagination.page}
            onPageChange={onPageChange}
            onRowsPerPageChange={onRowsPerPageChange}>
        </TablePagination>
        );
    }

    function loadingDiv() {
        return (<div style={{ zIndex: 1, position: 'absolute', width: '100%', height: '100%', backgroundColor: ' rgba(255,255,255,0.5)' }}></div>);
    }

    function onMenuItemClick(menuItem: any) {
        if (events?.onMenuItemClick) {
            events.onMenuItemClick({
                rowData: menuState.rowData,
                menuItem: menuItem,
                id: menuItem.id
            });
        }

        setMenuState(null);
    }

    function renderContextMenu() {
        if (!contextMenu) {
            return null;
        }

        const items = contextMenu.items;

        return (
            <Menu open={!!menuState?.anchor} anchorEl={menuState?.anchor} onClose={() => setMenuState(null)}>
                {items.map((a: any) => <MenuItem key={a.id} onClick={(e: any) => onMenuItemClick(a)}>{a.title}</MenuItem>)}
            </Menu>
        );
    }

    function renderExpandRow(data: any) {
        if (!expand) {
            return null;
        }

        const isExpanded = expandedState.includes(getRowKey(data));

        return (
            <TableRow>
                <TableCell colSpan={cols.length} style={{ paddingBottom: 0, paddingTop: 0 }}>
                    <Collapse in={isExpanded} timeout="auto" unmountOnExit>
                        {expand?.getExpandContent(data)}
                    </Collapse>
                </TableCell>
            </TableRow>
        )
    }

    function THead() {
        return cols.map((c: any) => (<TableCell width={c.width} key={getColKey(c)}>{theadTitle(c)}</TableCell>));
    }

    function TBody() {
        return (
            data.map((d: any) => (
                <React.Fragment key={getRowKey(d)}>
                    <TableRow>
                        {cols.map((c: any) => (
                            <TableCell
                                key={getCellKey(c, d)}
                                id={getCellKey(c, d)}
                                style={{
                                    whiteSpace: 'normal',
                                    wordBreak: 'break-word', // todo this also breaks lines in 'Id' column, lookgs ugly maybe fix somehow for Id?
                                }}
                                >
                                {tbodyCellValue(c, d)}
                            </TableCell>
                        ))}
                    </TableRow>
                    {renderExpandRow(d)}
                </React.Fragment>
            ))
        );
    }

    return (
        <>
            <Box sx={props.loading ? { position: 'relative' } : null}>
                {props.loading && <LinearProgress />}
                {props.loading && loadingDiv()}
                <TableContainer>
                    <Table size={props?.config?.size ?? 'medium'}>
                        <TableHead>
                            <TableRow>
                                {THead()}
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {TBody()}
                        </TableBody>
                    </Table>
                </TableContainer>
                {paginationComponent()}
                {renderContextMenu()}
            </Box>
        </>
    );
}

function emptyTable() {
    return {
        rowsCount: 0,
        rowsPerPage: 0,
        currentPage: 0,
        data: []
    }
}

interface ColsCfg<TDataType> {
    id?: string,
    title: string,
    dp?: string,
    val?(a: TDataType): any,
    copyOnClick?: boolean,
}

interface TableCfg<TDataType> {
    data: TDataType[] | null,
    cols: ColsCfg<TDataType>[],
    rowidkey?: string,
    rowidval?(a: TDataType): any,
    size?: string,
    pagination?: any,
    events?: any,
    contextMenu?: any,
    expand?: any
}

export { emptyTable };
export type { TableCfg };
