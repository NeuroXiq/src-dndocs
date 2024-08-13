import { Backdrop, CircularProgress } from "@mui/material";
import consts from '../config/const'

export default function PageLoading(props: any) {
    return (
        <Backdrop open={props.open} sx = {{ color: "white", zIndex: consts.zIndexPageBusy }}>
            <CircularProgress color="inherit" />
        </Backdrop>
    )
}