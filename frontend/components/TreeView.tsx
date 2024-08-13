import { Box, List, ListItemIcon, Menu, MenuItem } from "@mui/material";
import { useState } from "react";
import TreeNode from "./TreeNode";

export default function TreeView(props: any) {
    const [menuAnchorEl, setMenuAnchorEl] = useState<any>(null);
    const [menuNodeData, setMenuNodeData] = useState<any>(null);
    const menuOpen = Boolean(menuAnchorEl);

    let config = props.config;
    let data = props.data;

    const onOpenMenuClick = function (e: any, n: any) {
        setMenuAnchorEl(e.target);
        setMenuNodeData({
            data: n
        });

        // config.onOpenMenuClick
    }

    const onMenuClose = function (e: any) {
        setMenuAnchorEl(null);
    }

    const onMenuItemClick = function (e: any, info: any) {
        setMenuAnchorEl(null);
        config.onMenuItemClick({
            event: e,
            info: info,
            nodeData: menuNodeData.data
        });
    }

    const onMenuClick = function (e: any) {
        setMenuAnchorEl(null);
    }

    const onExpandCollapseClick = function (e: any, n: any) {
    }

    let nodeConfig = {
        ...config,
        onOpenMenuClick: onOpenMenuClick
    };

    function renderMenuItems() {
        const result = [];
        const menuItems = config.menuItems(menuNodeData?.data);
        return menuItems.map((c: any, i: any) => {
            return (
                <MenuItem key={i} onClick={(e) => onMenuItemClick(e, c.info)}>
                    {c.icon ? <ListItemIcon>{c.icon}</ListItemIcon> : null}
                    {c.text}
                </MenuItem>
            );
        });
    }

    return (
        <Box sx={{ maxHeight: '100%', display: 'flex', overflowY: 'auto' }}>
            <List dense={true}>
                {data.map((d: any) => {
                    return (<TreeNode key={config.getUid(d)} config={nodeConfig} data={d}></TreeNode>)
                })}
            </List>
            <Menu
                anchorEl={menuAnchorEl}
                id="treeview-menu"
                open={menuOpen}
                onClose={onMenuClose}
                onClick={onMenuClick}
                transformOrigin={{ horizontal: 'right', vertical: 'top' }}
                anchorOrigin={{ horizontal: 'right', vertical: 'top' }}>
                {renderMenuItems()}
            </Menu>
        </Box>
    );
}