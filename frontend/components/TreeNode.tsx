import { Collapse, Divider, IconButton, List, ListItem, ListItemButton, ListItemText } from "@mui/material";
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ExpandLessIcon from '@mui/icons-material/ExpandLess';
import MenuOpenIcon from '@mui/icons-material/MenuOpen';
import { useEffect, useState } from "react";

// export  function MapToTreeNodeData (data: any, getUid)  {
//     return data.map((d: any) => {
//         return {
//             uid: getUid(d),
//             data: d,
//             expanded: false
//         } as TreeNodeData;
//     })
// }

export default function TreeNode(props: any) {
    const config = props.config;
    const data = props.data;

    const [children, setChildren] = useState<any>(null);
    const [expanded, setExpanded] = useState<boolean>(false);
    const node = data;
    
    useEffect(() => {
        setChildren(null);
        setExpanded(false);
    }, [data])

    const onOpenMenuClick = function (e: any, data: any) {
        config.onOpenMenuClick(e, data);
    }

    const onExpandCollapseClick = function (e: any) {
        if (children === null) {
            let chi = config.getChildren(data);
            setChildren(chi);
        }

        setExpanded(currentExpanded => !currentExpanded);
    }

    function menuOpenIcon(e: any) {
        return config.menuOpenIcon(e);
    }

    return (
        <>
            <ListItem component="div">
                <IconButton onClick={(e: any) => onExpandCollapseClick(e)}>
                    { expanded ? <ExpandLessIcon fontSize="small" /> : <ExpandMoreIcon fontSize="small" /> }
                </IconButton>
                <Divider orientation="horizontal" />
                <IconButton onClick={(e: any) => onOpenMenuClick(e, node)}>
                    {/* <MenuOpenIcon fontSize="small" /> */}
                    {menuOpenIcon(node)}
                </IconButton>
                <ListItemText sx={{ml: 1}}>{config.getText(node)}</ListItemText>
            </ListItem>
            <Collapse sx={{ pl: 2 }} in={expanded} timeout="auto" unmountOnExit>
                <List dense={true} component="div">
                    {children && children.map((c: any) => (<TreeNode config={config} key={config.getUid(c)} data={c}></TreeNode>))}
                </List>
            </Collapse>
        </>
    );
}