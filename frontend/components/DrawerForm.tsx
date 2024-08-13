import { Box, Button, Divider, Drawer, Grid, Stack, Typography } from "@mui/material";

export default function DrawerForm(props: any) {
    const config = props.config;
    const title = config.title;
    const onCloseClick = config.onCloseClick;
    const onSubmitClick = config.onSubmitClick;

    return (
        <Drawer open={props.open} anchor="right" >
            <Typography width={384} variant="h4" padding={2} bgcolor="primary.main" color="primary.contrastText">{title}</Typography>
            <form>
                <Stack spacing={2} padding={2}>
                    {props.children}
                    <Divider />
                    <Box sx={{ display: 'flex', justifyContent: "end", gap: 1}}>
                        <Button variant="outlined" sx={{flex: '1 0 1px'}} onClick={onCloseClick}>Close</Button>
                        <Button variant="contained" sx={{flex: '2 0 1px'}} onClick={onSubmitClick}>Submit</Button>
                    </Box>
                </Stack>
            </form>
        </Drawer>
    );
}