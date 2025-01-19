import { Alert, Button, Dialog, DialogActions, DialogContent, DialogTitle, IconButton, Paper, Snackbar, Typography } from '@mui/material';
import { createContext, useMemo, useState } from 'react';
import CloseIcon from '@mui/icons-material/Close';
import { IUser } from '../services/UserService';
import { useNavigate } from 'react-router-dom';

export const GlobalAppContext = createContext<IGlobalAppContextValue>({} as IGlobalAppContextValue);

interface IGlobalAppContextValue {
    showErrMsg(msg: string): any,
    ShowSuccessMsg(msg: string): any
    onApiFetchError(info: any): any,
    user: IUser
};

export { type IGlobalAppContextValue };

export default function GlobalAppContextProvider({ children }: any) {
    const [gacState, setGacState] = useState<any>({
        globalError: null,
        // globalAlertErrorText: null,
        // globaSuccessMessage: null,
        user: null
    });

    const [globalSnackError, setGlobalSnackError] = useState<string | null>('');
    const [globalSnackSuccess, setGlobalSnackSuccess] = useState<string | null>('');
    const memoGlobaErrorDialog = useMemo(globaErrorDialog, [gacState]);

    const navigate = useNavigate();

    const gacValue : IGlobalAppContextValue = {
        user: gacState.user,
        showErrMsg: function (msg: string) {
            if (!msg) {
                msg = 'Action succees';
            }
            setGlobalSnackSuccess(msg);
        },
        ShowSuccessMsg(msg: string) {
            setGlobalSnackError(msg);
        },
        onApiFetchError: (info: any) => {
            if (info.fetchResult?.status === 401) {
                navigate('/login')
                return;
            }

            if (!!info.handlerResult) {
                // this is known exceptions, should be handler by
                // frontend ok (e.g. invalid form values etc.)
                if (info.handlerResult.errorMessage) {
                    setGlobalSnackError(info.handlerResult.errorMessage);
                }
                
                return;
            }

            setGacState({ ...gacState, globalError: info });
        }
    };

    function globaErrorDialog(): any {
        const e = gacState.globalError;
        const fr = e?.fetchResult;
        const fc = e?.fetchConfig;
        const err = e?.error;

        if (!e) {
            return null;
        }

        return (
            <Dialog open={true}>
                <DialogTitle variant='h4' bgcolor="error.main" color="error.contrastText">
                    Global Error
                    {!!fr ? (' / Status Code: ' + e.fetchResult.status) : ''}
                </DialogTitle>
                <DialogContent>
                    {err && <Typography variant="h5" >Message:</Typography>}
                    {err && <Typography variant="h6">{err.message}</Typography>}
                    {e.url && <Typography variant="h5" >URL:</Typography>}
                    {e.url && <Typography variant="body2">{e.url}</Typography>}
                    {fc && <Typography variant="h5" >Fetch Config:</Typography>}
                    {fc && <Typography variant="body2" component="pre" style={{overflow: 'auto' }}>{JSON.stringify(fc, null, 2)}</Typography>}
                    <Typography variant="h5" >Stack:</Typography>
                    {e.error && <Typography variant="body2"> {e.error.stack} </Typography>}
                </DialogContent>
                <DialogActions>
                    <Button
                        color="error"
                        onClick={() => window.location.reload()}>Refresh Page</Button>
                        <Button color="error" onClick={() => setGacState({...gacState, globalError: null})}>Close Dialog</Button>
                </DialogActions>
            </Dialog>
        );
    }

    function onErrorSnackHide() {
        setGlobalSnackError(null);
    }

    function onSuccessSnackClose() {
        setGacState({ ...gacState, globalSuccessMessage: null });
    }

    return (
        <GlobalAppContext.Provider value={gacValue}>
            {children}
            {memoGlobaErrorDialog}
            {<Snackbar
                autoHideDuration={10000}
                open={!!globalSnackError} onClose={onErrorSnackHide}
                anchorOrigin={{ vertical: 'bottom', horizontal: "center" }}>
                    <Paper elevation={1}>
                <Alert
                    severity="error"
                    action={
                        <IconButton
                            size="small"
                            aria-label="close"
                            color="inherit"
                            onClick={onErrorSnackHide}
                        >
                            <CloseIcon fontSize="small" />
                        </IconButton>
                    }>{globalSnackError}</Alert>
                    </Paper>
            </Snackbar>
            }
            {
                <Snackbar
                    anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
                    autoHideDuration={5000}
                    open={!!globalSnackSuccess}
                    onClose={onSuccessSnackClose}>
                    <Alert severity="success">
                        {globalSnackSuccess}
                    </Alert>
                </Snackbar>
            }
        </GlobalAppContext.Provider>
    );
}
