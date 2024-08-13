import { Button, Dialog, DialogActions, DialogContent, DialogTitle, Typography } from "@mui/material";

export default function ConfirmAction(props: any) {
    return (
        <Dialog maxWidth="sm" fullWidth open={props.open}>
            <DialogTitle>{props.title}</DialogTitle>
            <DialogContent><Typography variant="body1">{props.content}</Typography></DialogContent>
            <DialogActions>
                <Button variant="outlined" onClick={props.onCancel}>Cancel</Button>
                <Button variant="contained" color="warning" onClick={props.onConfirm}>Confirm</Button>
            </DialogActions>
        </Dialog>
    );
}