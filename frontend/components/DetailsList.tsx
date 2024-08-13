'use client';
import { Divider, List, ListItem, ListItemIcon, ListItemText, Paper, Skeleton, Tooltip, Typography } from "@mui/material";
import React from "react";
import HelpOutlineIcon from '@mui/icons-material/HelpOutline';
import { getField } from "@/app/help/HelpData";

export default function DetailsList(props: any): any {
    let data = props.data;
    let itemsConfig = props.itemsConfig;
    const title = props.title || 'Details';

    const getReactKey = function (item: any, index: number) {
        let a = item.reactKey || index.toString();
        return a;
    }

    const getListItemValue = function (itemCfg: any): any {
        let key = itemCfg.key;
        let val = null;

        if (!itemCfg.val && !(key in data)) {
            throw new Error(`'${key}' not found in object`);
        }


        if (itemCfg.val) {
            val = itemCfg.val(data);
        } else {
            val = data[key];
        }
        
        if (typeof val === 'object') {
            return val;
        }
        else {
            return val?.toString();
        }
    }

    function GetHelpIcon(c: any) {
        if (!c.helpFieldId) {
            return null;
        }

        const title = getField(c.helpFieldId)?.description;

        if (!title) { 
            return null;
        }

        return (
            <Tooltip title={<Typography variant="body2">{title}</Typography>}>
                <ListItemIcon sx={{cursor: 'help'}}>
                    <HelpOutlineIcon />
                </ListItemIcon>
            </Tooltip>);
    }

    function getSecondaryTypographyProps(c: any): any {
        const stp = c.stp;
        if (!stp) {
            return null;
        }

        return stp;

        // let res: any = {};

        // if (stp === 'error') {
        //     res.color = "error";
        // } else if (stp === 'warning') {
        //     res.color = "warning.main";
        // } else { res = c.stp; }

    }

    function getContent(c: any, index: number) {
        return (<React.Fragment key={getReactKey(c, index)}>
            <ListItem disablePadding>
                <ListItemText
                    primary={c.title}
                    secondary={getListItemValue(c)}
                    secondaryTypographyProps={getSecondaryTypographyProps(c)}>
                </ListItemText>
                {GetHelpIcon(c)}
            </ListItem>
            {index < itemsConfig.length - 1 ? <Divider /> : null}
        </React.Fragment>)
    }

    return (
        <Paper sx={{ padding: '0 1rem' }}>
            <Typography variant="h6" align="center" padding={1}>{title}</Typography>
            <Divider />
            {props.loading &&
                itemsConfig.map((a: any, i: number) => (
                    <Skeleton key={i} variant="text" sx={{ fontSize: '3rem' }} />
                ))
            }

            {!props.loading && <List>
                {itemsConfig.map((c: any, index: number) => (getContent(c, index)))}
            </List>}
        </Paper>
    )
}

interface ItemCfg<TType> {
    title: string,
    key?: string,
    stp?: any,
    helpFieldId?: any,
    val?(p: TType): any
}

export type { ItemCfg }